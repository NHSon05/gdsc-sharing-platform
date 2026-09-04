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
    let refreshToken = cookieStore.get(AUTH_COOKIE_NAMES.REFRESH_TOKEN)?.value;

    // Check JSON request body if not found in cookies
    if (!refreshToken) {
      try {
        const body = await request.json();
        refreshToken = body?.refreshToken;
      } catch {
        // Body was empty or not JSON
      }
    }

    if (!refreshToken) {
      return NextResponse.json(
        { title: "Unauthorized", detail: "No refresh token available" },
        { status: 401 }
      );
    }

    const res = await fetch(`${BACKEND_INTERNAL_URL}/api/auth/refresh`, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify({ refreshToken }),
    });

    if (!res.ok) {
      // If refresh failed on backend, clear invalid cookies
      cookieStore.delete(AUTH_COOKIE_NAMES.ACCESS_TOKEN);
      cookieStore.delete(AUTH_COOKIE_NAMES.REFRESH_TOKEN);
      const errorData = await res.json().catch(() => ({
        title: "Unauthorized",
        detail: "Failed to refresh token",
      }));
      return NextResponse.json(errorData, { status: res.status });
    }

    const data = (await res.json()) as {
      accessToken: string;
      refreshToken?: string;
      tokenType?: string;
      expiresIn?: number;
    };

    const isProd = process.env.NODE_ENV === "production";

    // Update HttpOnly accessToken cookie
    cookieStore.set(AUTH_COOKIE_NAMES.ACCESS_TOKEN, data.accessToken, {
      httpOnly: true,
      secure: isProd,
      sameSite: "lax",
      path: "/",
      maxAge: data.expiresIn || 15 * 60,
    });

    // Update HttpOnly refreshToken cookie if rotated
    if (data.refreshToken) {
      cookieStore.set(AUTH_COOKIE_NAMES.REFRESH_TOKEN, data.refreshToken, {
        httpOnly: true,
        secure: isProd,
        sameSite: "lax",
        path: "/",
        maxAge: 7 * 24 * 60 * 60,
      });
    }

    return NextResponse.json(data);
  } catch (error) {
    console.error("[Route /api/auth/refresh] Error:", error);
    return NextResponse.json(
      { title: "Internal Server Error", detail: "Failed to refresh token" },
      { status: 500 }
    );
  }
}
