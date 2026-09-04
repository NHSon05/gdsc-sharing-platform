import { NextResponse } from "next/server";
import { cookies } from "next/headers";
import { AUTH_COOKIE_NAMES } from "@/core/session/session.cookies";

const BACKEND_INTERNAL_URL =
  process.env.INTERNAL_API_URL ||
  process.env.BACKEND_API_URL ||
  process.env.NEXT_PUBLIC_API_URL ||
  "http://localhost:5184";

export async function POST(request: Request) {
  try {
    const cookieStore = await cookies();
    const accessToken = cookieStore.get(AUTH_COOKIE_NAMES.ACCESS_TOKEN)?.value;
    const refreshToken = cookieStore.get(AUTH_COOKIE_NAMES.REFRESH_TOKEN)?.value;

    let body = {};
    try {
      body = await request.json();
    } catch {
      // Empty body is okay
    }

    // Forward logout request to backend
    await fetch(`${BACKEND_INTERNAL_URL}/api/auth/logout`, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
        ...(accessToken ? { Authorization: `Bearer ${accessToken}` } : {}),
      },
      body: JSON.stringify({
        refreshToken: refreshToken || (body as { refreshToken?: string })?.refreshToken,
      }),
    }).catch(() => {
      // Ignore backend logout errors to always ensure local session is cleared
    });

    // Clear HttpOnly cookies
    cookieStore.delete(AUTH_COOKIE_NAMES.ACCESS_TOKEN);
    cookieStore.delete(AUTH_COOKIE_NAMES.REFRESH_TOKEN);

    return new NextResponse(null, { status: 204 });
  } catch (error) {
    console.error("[Route /api/auth/logout] Error:", error);
    return new NextResponse(null, { status: 204 });
  }
}
