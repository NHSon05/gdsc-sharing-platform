import { NextRequest, NextResponse } from "next/server";
import { AUTH_COOKIE_NAMES } from "@/core/session/session.cookies";
import {
  clearSession,
  trustedMutation,
  unavailable,
  writeSession,
} from "@/core/session/bff.server";
import { refreshSession } from "@/core/session/refresh.server";

export async function POST(request: NextRequest) {
  if (!trustedMutation(request)) return new NextResponse(null, { status: 403 });
  const token = request.cookies.get(AUTH_COOKIE_NAMES.REFRESH_TOKEN)?.value;
  const rejected = () => clearSession(new NextResponse(null, { status: 401 }));
  if (!token) return rejected();
  try {
    const pair = await refreshSession(token);
    return pair
      ? writeSession(new NextResponse(null, { status: 204 }), pair)
      : rejected();
  } catch {
    return unavailable();
  } // Do not erase a session on transient outages.
}
