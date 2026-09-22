import { NextRequest, NextResponse } from "next/server";
import { createHash, randomBytes } from "node:crypto";
import { safeReturnPath } from "@/core/session/safe-return-path";
import {
  configuredOrigin,
  googleCookieOptions,
  noStore,
  unavailable,
} from "@/core/session/bff.server";

export const runtime = "nodejs";

export async function GET(request: NextRequest) {
  try {
    // OAuth needs a top-level browser navigation so the verifier cookie is sent
    // back to the BFF callback after the backend/provider redirect chain.
    const verifier = randomBytes(32).toString("base64url");
    const backendUrl = new URL(
      "/api/auth/google/start",
      configuredOrigin("BACKEND_PUBLIC_ORIGIN")
    );
    backendUrl.searchParams.set(
      "challenge",
      createHash("sha256").update(verifier).digest("base64url")
    );

    const response = NextResponse.redirect(backendUrl);
    response.cookies.set("google_verifier", verifier, googleCookieOptions);
    response.cookies.set(
      "google_return_to",
      safeReturnPath(request.nextUrl.searchParams.get("returnUrl")),
      googleCookieOptions
    );
    return noStore(response);
  } catch {
    return unavailable();
  }
}
