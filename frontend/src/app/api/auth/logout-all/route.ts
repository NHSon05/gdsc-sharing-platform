import { NextRequest, NextResponse } from "next/server";
import { AUTH_COOKIE_NAMES } from "@/core/session/session.cookies";
import {
  backendFetch,
  clearSession,
  noStore,
  trustedMutation,
  unavailable,
} from "@/core/session/bff.server";

export async function POST(request: NextRequest) {
  if (!trustedMutation(request)) return new NextResponse(null, { status: 403 });
  const token = request.cookies.get(AUTH_COOKIE_NAMES.ACCESS_TOKEN)?.value;
  if (!token) return noStore(new NextResponse(null, { status: 401 }));
  try {
    const response = await backendFetch("/api/auth/logout-all", {
      method: "POST",
      headers: { Authorization: `Bearer ${token}` },
    });
    if (!response.ok)
      return noStore(
        new NextResponse(null, {
          status: response.status >= 400 ? response.status : 502,
        })
      );
    return clearSession(new NextResponse(null, { status: 204 }));
  } catch {
    return unavailable();
  }
}
