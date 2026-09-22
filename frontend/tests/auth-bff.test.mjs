import { test, afterEach } from "node:test";
import assert from "node:assert/strict";
import { readFileSync, existsSync } from "node:fs";
import { createRequire } from "node:module";
import { dirname, resolve } from "node:path";
import { fileURLToPath } from "node:url";
import { runInThisContext } from "node:vm";
import { createHash } from "node:crypto";

const require = createRequire(import.meta.url);
const ts = require("typescript");
const { NextRequest, NextResponse } = require("next/server");
const root = resolve(dirname(fileURLToPath(import.meta.url)), "../src");
// Load real TS route handlers without a Next dev server or new test dependencies.
// Only the server-only marker is stubbed; NextRequest/NextResponse remain real.
function loader() {
  const cache = new Map();
  function load(path) {
    if (!existsSync(path)) path += ".ts";
    if (cache.has(path)) return cache.get(path).exports;
    const loadedModule = { exports: {} };
    cache.set(path, loadedModule);
    const compiled = ts.transpileModule(readFileSync(path, "utf8"), {
      compilerOptions: {
        module: ts.ModuleKind.CommonJS,
        target: ts.ScriptTarget.ES2022,
        esModuleInterop: true,
      },
    }).outputText;
    const localRequire = (name) =>
      name === "server-only"
        ? {}
        : name.startsWith("@/")
          ? load(resolve(root, name.slice(2)))
          : name.startsWith(".")
            ? load(resolve(dirname(path), name))
            : require(name);
    runInThisContext(`(function(require,module,exports){${compiled}\n})`, {
      filename: path,
    })(localRequire, loadedModule, loadedModule.exports);
    return loadedModule.exports;
  }
  return (path) => load(resolve(root, path));
}
const originalFetch = globalThis.fetch;
process.env.APP_ORIGIN = "http://localhost:3000";
process.env.BACKEND_PUBLIC_ORIGIN = "https://localhost:7160";
process.env.INTERNAL_API_URL = "http://localhost:5184";
afterEach(() => {
  globalThis.fetch = originalFetch;
});
const profile = {
  id: "member-id",
  email: "member@example.test",
  displayName: "Member",
  status: "Active",
  roles: ["Member"],
};
const tokens = {
  accessToken: "test-access",
  refreshToken: "test-refresh",
  expiresIn: 900,
  user: profile,
};
function request(
  path,
  { method = "GET", cookie, body, origin = "http://localhost:3000" } = {}
) {
  return new NextRequest(`http://localhost:3000${path}`, {
    method,
    headers: {
      origin,
      ...(cookie ? { cookie } : {}),
      "content-type": "application/json",
    },
    ...(body ? { body: JSON.stringify(body) } : {}),
  });
}

test("production session cookies are Secure, HttpOnly and host-only", () => {
  const environment = process.env.NODE_ENV;
  try {
    process.env.NODE_ENV = "production";
    const { writeSession } = loader()("core/session/bff.server.ts");
    const response = writeSession(new NextResponse(null, { status: 204 }), tokens);
    for (const name of ["accessToken", "refreshToken"]) {
      const cookie = response.cookies.get(name);
      assert.equal(cookie.secure, true);
      assert.equal(cookie.httpOnly, true);
      assert.equal(cookie.sameSite, "lax");
      assert.equal(cookie.domain, undefined);
    }
  } finally {
    if (environment === undefined) delete process.env.NODE_ENV;
    else process.env.NODE_ENV = environment;
  }
});

test("a late 401 from the previous session cannot invalidate the new user", async () => {
  const load = loader();
  const axios = require("axios");
  const { httpClient } = load("core/http/http-client.ts");
  const { publicHttpClient } = load("core/http/public-http-client.ts");
  const { useSessionStore } = load("core/session/session.store.ts");
  useSessionStore.getState().setUser(profile);
  let release;
  let started;
  const ready = new Promise(resolve => { started = resolve; });
  const gate = new Promise(resolve => { release = resolve; });
  httpClient.defaults.adapter = async config => {
    started();
    await gate;
    throw new axios.AxiosError("expired", "ERR_BAD_RESPONSE", config, null,
      { status: 401, statusText: "", data: {}, headers: {}, config });
  };
  publicHttpClient.defaults.adapter = async () => { assert.fail("Old request must not refresh"); };
  const result = httpClient.get("/api/profile/me");
  await ready;
  useSessionStore.getState().clearSession();
  useSessionStore.getState().setUser({ ...profile, id: "new-user" });
  release();
  await assert.rejects(result, error => error.status === 401);
  assert.equal(useSessionStore.getState().user.id, "new-user");
  assert.equal(useSessionStore.getState().status, "authenticated");
});

test("proxy rejects encoded auth paths and only forwards business endpoints", async () => {
  const load = loader();
  const { proxyPath } = load("core/http/proxy-path.ts");
  for (const path of [
    "/api/auth/login",
    "/api/%61uth/login",
    "/api/%2561uth/login",
    "/api/auth%2flogin",
    "/api/v1/../auth/login",
    "/api/v1/%2e%2e/auth/login",
    "/api//auth/login",
    "/api/v1/\\auth/login",
  ])
    assert.equal(proxyPath(path), null, path);
  assert.equal(
    proxyPath("/api/v1/interview-question/123"),
    "/api/v1/interview-questions/123"
  );
  const route = load("app/api/[...path]/route.ts");
  globalThis.fetch = async () => {
    assert.fail("Blocked auth path must never reach backend");
  };
  assert.equal((await route.GET(request("/api/%61uth/login"))).status, 404);
});

test("proxy uses server cookie, strips upstream cookies and preserves upload body", async () => {
  const route = loader()("app/api/[...path]/route.ts");
  globalThis.fetch = async (url, init) => {
    assert.equal(new URL(url).origin, "http://localhost:5184");
    assert.equal(init.headers.get("authorization"), "Bearer test-access");
    assert.equal(init.headers.get("cookie"), null);
    assert.equal(
      new TextDecoder().decode(init.body),
      JSON.stringify({ name: "file" })
    );
    return new Response("ok", {
      headers: { "set-cookie": "accessToken=leak", etag: "version1" },
    });
  };
  const response = await route.POST(
    request("/api/profile/me/avatar", {
      method: "POST",
      cookie: "accessToken=test-access",
      body: { name: "file" },
    })
  );
  assert.equal(response.status, 200);
  assert.equal(response.headers.get("set-cookie"), null);
  assert.equal(response.headers.get("etag"), "version1");
});

test("safe return path rejects external, encoded and authentication destinations", () => {
  const { safeReturnPath } = loader()("core/session/safe-return-path.ts");
  for (const path of [
    null,
    "https://evil.test",
    "//evil.test",
    "/\\evil.test",
    "/%2f%2fevil.test",
    "/%5cevil.test",
    "/%61pi/auth/logout",
    "/login",
    "/api/auth/google/start",
    "/%zz",
  ])
    assert.equal(safeReturnPath(path), "/");
  assert.equal(safeReturnPath("/roadmaps?id=7#node"), "/roadmaps?id=7#node");
});

test("start binds SHA256 challenge to HttpOnly verifier, with safe return path", async () => {
  const response = await loader()("app/api/auth/google/start/route.ts").GET(
    request("/api/auth/google/start?returnUrl=%2Fprofile")
  );
  const verifier = response.cookies.get("google_verifier");
  assert.equal(verifier.httpOnly, true);
  assert.equal(verifier.path, "/api/auth/google");
  assert.equal(verifier.maxAge, 300);
  const redirect = new URL(response.headers.get("location"));
  assert.equal(redirect.origin, "https://localhost:7160");
  assert.equal(
    redirect.searchParams.get("challenge"),
    createHash("sha256").update(verifier.value).digest("base64url")
  );
  assert.equal(response.cookies.get("google_return_to").value, "/profile");
});

test("callback exchanges code server-side, keeps tokens out of redirect and body", async () => {
  const route = loader()("app/api/auth/google/callback/route.ts");
  const code = "a".repeat(43),
    verifier = "b".repeat(43);
  globalThis.fetch = async (url, init) => {
    assert.equal(new URL(url).pathname, "/api/auth/google/exchange");
    assert.deepEqual(JSON.parse(init.body), { code, verifier });
    return Response.json(tokens);
  };
  const response = await route.GET(
    request(`/api/auth/google/callback?code=${code}`, {
      cookie: `google_verifier=${verifier}; google_return_to=/profile`,
    })
  );
  assert.equal(
    response.headers.get("location"),
    "http://localhost:3000/profile"
  );
  assert.equal(await response.text(), "");
  assert.equal(response.cookies.get("accessToken").httpOnly, true);
  assert.equal(response.cookies.get("refreshToken").httpOnly, true);
  assert.equal(response.cookies.get("google_verifier").maxAge, 0);
  assert.equal(response.headers.get("cache-control"), "no-store");
});

test("invalid or rejected callback clears both temporary and session cookies", async () => {
  const route = loader()("app/api/auth/google/callback/route.ts");
  globalThis.fetch = async () => new Response(null, { status: 401 });
  for (const req of [
    request("/api/auth/google/callback?error=denied"),
    request(`/api/auth/google/callback?code=${"a".repeat(43)}`, {
      cookie: `google_verifier=${"b".repeat(43)}`,
    }),
  ]) {
    const response = await route.GET(req);
    assert.equal(new URL(response.headers.get("location")).pathname, "/login");
    for (const key of [
      "accessToken",
      "refreshToken",
      "google_verifier",
      "google_return_to",
    ])
      assert.equal(response.cookies.get(key).maxAge, 0);
  }
});

test("login returns only whitelisted user fields and rejects cross-origin POST", async () => {
  const route = loader()("app/api/auth/login/route.ts");
  let calls = 0;
  globalThis.fetch = async () => {
    calls++;
    return Response.json({
      ...tokens,
      user: { ...profile, providerToken: "must-not-leak" },
    });
  };
  const denied = await route.POST(
    request("/api/auth/login", {
      method: "POST",
      origin: "https://evil.test",
      body: {},
    })
  );
  assert.equal(denied.status, 403);
  assert.equal(calls, 0);
  const response = await route.POST(
    request("/api/auth/login", {
      method: "POST",
      body: { email: "member@example.test", password: "dummy" },
    })
  );
  assert.deepEqual(await response.json(), { user: profile });
  assert.equal(response.cookies.get("refreshToken").httpOnly, true);
});

test("concurrent server refresh requests share one rotation; browser response contains no tokens", async () => {
  const route = loader()("app/api/auth/refresh/route.ts");
  let calls = 0;
  let release;
  const gate = new Promise((resolve) => {
    release = resolve;
  });
  globalThis.fetch = async () => {
    calls++;
    await gate;
    return Response.json(tokens);
  };
  const makeRequest = () =>
    request("/api/auth/refresh", {
      method: "POST",
      cookie: "refreshToken=old-refresh",
    });
  const pending = [route.POST(makeRequest()), route.POST(makeRequest())];
  release();
  const responses = await Promise.all(pending);
  assert.equal(calls, 1);
  for (const response of responses) {
    assert.equal(response.status, 204);
    assert.equal(await response.text(), "");
    assert.equal(response.cookies.get("refreshToken").value, "test-refresh");
  }
});

test("refresh clears cookies on rejection, preserves them on temporary outage", async () => {
  for (const status of [401, 403, 503]) {
    const route = loader()("app/api/auth/refresh/route.ts");
    globalThis.fetch = async () => new Response(null, { status });
    const response = await route.POST(
      request("/api/auth/refresh", {
        method: "POST",
        cookie: `refreshToken=token-${status}`,
      })
    );
    if (status === 503) {
      assert.equal(response.status, 502);
      assert.equal(response.headers.get("set-cookie"), null);
    } else {
      assert.equal(response.status, 401);
      assert.equal(response.cookies.get("refreshToken").maxAge, 0);
    }
  }
});

test("client coalesces simultaneous 401s, retries once, and leaves 403 alone", async () => {
  const load = loader();
  const axios = require("axios");
  const { httpClient } = load("core/http/http-client.ts");
  const { publicHttpClient } = load("core/http/public-http-client.ts");
  const { useSessionStore } = load("core/session/session.store.ts");
  useSessionStore.getState().setUser(profile);
  let refreshes = 0;
  const success = (config, status = 200) => ({
    status,
    statusText: "",
    data: {},
    headers: {},
    config,
  });
  const fail = (config, status) =>
    Promise.reject(
      new axios.AxiosError(
        "test",
        "ERR_BAD_RESPONSE",
        config,
        null,
        success(config, status)
      )
    );
  publicHttpClient.defaults.adapter = async (config) => {
    if (config.url === "/api/auth/me") return fail(config, 401);
    refreshes++;
    return success(config, 204);
  };
  httpClient.defaults.adapter = async (config) =>
    config._retry ? success(config) : fail(config, 401);
  await Promise.all([
    httpClient.get("/api/profile/me"),
    httpClient.get("/api/roadmaps"),
  ]);
  assert.equal(refreshes, 1);
  httpClient.defaults.adapter = (config) => fail(config, 403);
  await assert.rejects(
    httpClient.get("/api/admin/members"),
    (error) => error.status === 403
  );
  assert.equal(refreshes, 1);
  assert.equal(useSessionStore.getState().status, "authenticated");
  httpClient.defaults.adapter = (config) => fail(config, 401);
  await assert.rejects(
    httpClient.get("/api/profile/me"),
    (error) => error.status === 401
  );
  assert.equal(refreshes, 2);
  assert.equal(useSessionStore.getState().status, "unauthenticated");
});
