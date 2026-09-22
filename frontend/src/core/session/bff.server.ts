import "server-only";
import { NextResponse } from "next/server";
import { AUTH_COOKIE_NAMES, AUTH_COOKIE_MAX_AGE } from "./session.cookies";

export function configuredOrigin(name: string, fallback?: string): string {
  const value = process.env[name] || fallback;
  if (!value) throw new Error(`Missing server environment: ${name}`);
  const url = new URL(value);
  if (
    !["http:", "https:"].includes(url.protocol) ||
    url.username ||
    url.password ||
    url.pathname !== "/" ||
    url.search ||
    url.hash ||
    (process.env.NODE_ENV === "production" && url.protocol !== "https:")
  ) {
    throw new Error(`Invalid server origin: ${name}`);
  }
  return url.origin;
}

export const appOrigin = () =>
  configuredOrigin(
    "APP_ORIGIN",
    process.env.NODE_ENV === "production" ? undefined : "http://localhost:3000"
  );
export const backendOrigin = () =>
  configuredOrigin(
    "INTERNAL_API_URL",
    process.env.BACKEND_API_URL ||
      (process.env.NODE_ENV === "production"
        ? undefined
        : "http://localhost:5184")
  );

export function trustedMutation(request: Request): boolean {
  return (
    request.headers.get("origin") === appOrigin() &&
    request.headers.get("sec-fetch-site") !== "cross-site"
  );
}

export interface TokenPair {
  accessToken: string;
  refreshToken: string;
  expiresIn: number;
}

export function parseTokenPair(data: unknown): TokenPair {
  if (
    !data ||
    typeof data !== "object" ||
    !("accessToken" in data) ||
    typeof data.accessToken !== "string" ||
    !data.accessToken ||
    !("refreshToken" in data) ||
    typeof data.refreshToken !== "string" ||
    !data.refreshToken ||
    !("expiresIn" in data) ||
    typeof data.expiresIn !== "number" ||
    !Number.isFinite(data.expiresIn) ||
    data.expiresIn <= 0
  )
    throw new Error("Invalid session response");
  return {
    accessToken: data.accessToken,
    refreshToken: data.refreshToken,
    expiresIn: data.expiresIn,
  };
}

const cookieOptions = {
  httpOnly: true,
  secure: process.env.NODE_ENV === "production",
  sameSite: "lax" as const,
  path: "/",
};
export function noStore(response: NextResponse): NextResponse {
  response.headers.set("Cache-Control", "no-store");
  response.headers.set("Referrer-Policy", "no-referrer");
  return response;
}
export function writeSession(
  response: NextResponse,
  pair: TokenPair
): NextResponse {
  response.cookies.set(AUTH_COOKIE_NAMES.ACCESS_TOKEN, pair.accessToken, {
    ...cookieOptions,
    maxAge: pair.expiresIn,
  });
  response.cookies.set(AUTH_COOKIE_NAMES.REFRESH_TOKEN, pair.refreshToken, {
    ...cookieOptions,
    maxAge: AUTH_COOKIE_MAX_AGE.REFRESH_TOKEN,
  });
  response.cookies.set("userRole", "", { ...cookieOptions, maxAge: 0 });
  return noStore(response);
}
export function clearSession(response: NextResponse): NextResponse {
  for (const name of [...Object.values(AUTH_COOKIE_NAMES), "userRole"]) {
    response.cookies.set(name, "", { ...cookieOptions, maxAge: 0 });
  }
  return noStore(response);
}
export function backendFetch(
  path: string,
  init: RequestInit = {}
): Promise<Response> {
  const origin = backendOrigin();
  const url = new URL(path, origin);
  if (url.origin !== origin) throw new Error("Invalid backend path");
  const timeout = AbortSignal.timeout(15000);
  return fetch(url, {
    ...init,
    cache: "no-store",
    redirect: "manual",
    signal: init.signal ? AbortSignal.any([init.signal, timeout]) : timeout,
  });
}
export function unavailable(): NextResponse {
  return noStore(
    NextResponse.json(
      { title: "Authentication service unavailable" },
      { status: 502 }
    )
  );
}
export const googleCookieOptions = {
  ...cookieOptions,
  path: "/api/auth/google",
  maxAge: 300,
};
export function clearGoogleCookies(response: NextResponse): NextResponse {
  for (const name of ["google_verifier", "google_return_to"])
    response.cookies.set(name, "", { ...googleCookieOptions, maxAge: 0 });
  return noStore(response);
}
