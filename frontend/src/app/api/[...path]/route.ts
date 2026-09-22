import { NextRequest, NextResponse } from "next/server";
import { AUTH_COOKIE_NAMES } from "@/core/session/session.cookies";
import { proxyPath } from "@/core/http/proxy-path";
import {
  backendFetch,
  noStore,
  trustedMutation,
  unavailable,
} from "@/core/session/bff.server";

export const runtime = "nodejs";
async function proxy(request: NextRequest) {
  const path = proxyPath(request.nextUrl.pathname);
  // Only known business routes are forwarded, including after percent-decoding.
  if (!path) return noStore(new NextResponse(null, { status: 404 }));
  if (
    !["GET", "HEAD", "OPTIONS"].includes(request.method) &&
    !trustedMutation(request)
  )
    return new NextResponse(null, { status: 403 });
  const headers = new Headers();
  for (const name of [
    "content-type",
    "accept",
    "if-match",
    "if-none-match",
    "range",
  ]) {
    const value = request.headers.get(name);
    if (value) headers.set(name, value);
  }
  const token = request.cookies.get(AUTH_COOKIE_NAMES.ACCESS_TOKEN)?.value;
  if (token) headers.set("Authorization", `Bearer ${token}`);
  try {
    const response = await backendFetch(path + request.nextUrl.search, {
      method: request.method,
      headers,
      body: ["GET", "HEAD"].includes(request.method)
        ? undefined
        : await request.arrayBuffer(),
    });
    if (
      response.status >= 300 &&
      response.status < 400 &&
      response.status !== 304
    )
      return unavailable();
    const outgoing = new Headers();
    for (const name of [
      "content-type",
      "etag",
      "retry-after",
      "content-disposition",
      "content-range",
      "accept-ranges",
    ]) {
      const value = response.headers.get(name);
      if (value) outgoing.set(name, value);
    }
    return noStore(
      new NextResponse(response.body, {
        status: response.status,
        headers: outgoing,
      })
    );
  } catch {
    return unavailable();
  }
}
export {
  proxy as GET,
  proxy as POST,
  proxy as PUT,
  proxy as PATCH,
  proxy as DELETE,
  proxy as HEAD,
  proxy as OPTIONS,
};
