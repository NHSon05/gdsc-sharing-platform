import { NextRequest, NextResponse } from "next/server";
import { safeReturnPath } from "@/core/session/safe-return-path";
import {
  appOrigin,
  backendFetch,
  clearGoogleCookies,
  clearSession,
  parseTokenPair,
  writeSession,
} from "@/core/session/bff.server";

export const runtime = "nodejs";

export async function GET(request: NextRequest) {
  const origin = appOrigin();
  const returnTo = safeReturnPath(
    request.cookies.get("google_return_to")?.value ?? null
  );
  const failureUrl = new URL("/login", origin);
  failureUrl.searchParams.set("error", "external_login_failed");
  if (returnTo !== "/") failureUrl.searchParams.set("returnUrl", returnTo);

  const failed = () =>
    clearGoogleCookies(clearSession(NextResponse.redirect(failureUrl)));

  const code = request.nextUrl.searchParams.get("code");
  const verifier = request.cookies.get("google_verifier")?.value;
  if (
    request.nextUrl.searchParams.has("error") ||
    !code ||
    !verifier ||
    !/^[A-Za-z0-9_-]{43}$/.test(code) ||
    !/^[A-Za-z0-9_-]{43}$/.test(verifier)
  ) {
    return failed();
  }

  try {
    const result = await backendFetch("/api/auth/google/exchange", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ code, verifier }),
    });
    if (!result.ok) return failed();

    const pair = parseTokenPair(await result.json());
    return clearGoogleCookies(
      writeSession(NextResponse.redirect(new URL(returnTo, origin)), pair)
    );
  } catch {
    return failed();
  }
}
