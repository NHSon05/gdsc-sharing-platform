import { NextRequest, NextResponse } from "next/server";
import { AUTH_COOKIE_NAMES } from "@/core/session/session.cookies";
import { parseUserProfile } from "@/core/session/user-profile";
import { backendFetch, noStore, unavailable } from "@/core/session/bff.server";

export async function GET(request: NextRequest) {
  const token = request.cookies.get(AUTH_COOKIE_NAMES.ACCESS_TOKEN)?.value;
  if (!token) return noStore(new NextResponse(null, { status: 401 }));
  try {
    const response = await backendFetch("/api/auth/me", {
      headers: { Authorization: `Bearer ${token}` },
    });
    if (!response.ok)
      return noStore(
        new NextResponse(null, {
          status: response.status >= 400 ? response.status : 502,
        })
      );
    return noStore(NextResponse.json(parseUserProfile(await response.json())));
  } catch {
    return unavailable();
  }
}
