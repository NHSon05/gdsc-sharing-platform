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
    const body = await request.json();

    const res = await fetch(`${BACKEND_INTERNAL_URL}/api/auth/login`, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify(body),
    });

    const data = await res.json();

    if (!res.ok) {
      return NextResponse.json(data, { status: res.status });
    }

    const cookieStore = await cookies();
    const isProd = process.env.NODE_ENV === "production";

    if (data.accessToken) {
      cookieStore.set(AUTH_COOKIE_NAMES.ACCESS_TOKEN, data.accessToken, {
        httpOnly: true,
        secure: isProd,
        sameSite: "lax",
        path: "/",
        maxAge: data.expiresIn || 15 * 60,
      });
    }

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
    console.error("[Route /api/auth/login] Error:", error);
    return NextResponse.json(
      { title: "Internal Server Error", detail: "Login request failed" },
      { status: 500 }
    );
  }
}
