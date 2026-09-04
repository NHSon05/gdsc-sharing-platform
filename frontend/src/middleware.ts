import { NextResponse } from "next/server";
import type { NextRequest } from "next/server";
import { AUTH_COOKIE_NAMES } from "@/core/session/session.cookies";

// Define protected route prefixes
const PROTECTED_ROUTES = [
  "/admin",
  "/dashboard",
  "/roadmaps",
  "/schedule",
  "/profile",
];

// Define auth routes (where logged-in users shouldn't re-enter)
const AUTH_ROUTES = ["/login", "/register"];

export function middleware(request: NextRequest) {
  const { pathname, search } = request.nextUrl;
  const accessToken = request.cookies.get(
    AUTH_COOKIE_NAMES.ACCESS_TOKEN
  )?.value;
  const refreshToken = request.cookies.get(
    AUTH_COOKIE_NAMES.REFRESH_TOKEN
  )?.value;

  const isProtectedRoute = PROTECTED_ROUTES.some(
    (route) => pathname === route || pathname.startsWith(`${route}/`)
  );
  const isAuthRoute = AUTH_ROUTES.some(
    (route) => pathname === route || pathname.startsWith(`${route}/`)
  );

  // 1. If accessing protected route without access token AND without refresh token -> Redirect to /login
  if (isProtectedRoute && !accessToken && !refreshToken) {
    const returnUrl = encodeURIComponent(`${pathname}${search}`);
    const loginUrl = new URL(`/login?returnUrl=${returnUrl}`, request.url);
    return NextResponse.redirect(loginUrl);
  }

  // 2. If accessing login/register while already having an active session -> Redirect to home
  if (isAuthRoute && (accessToken || refreshToken)) {
    return NextResponse.redirect(new URL("/", request.url));
  }

  if (pathname.startsWith("/admin")) {
    if (!accessToken && !refreshToken) {
      return NextResponse.redirect(
        new URL("/login?returnUrl=" + pathname, request.url)
      );
    }

    const userRole = request.cookies.get("userRole")?.value;
    if (userRole !== "Admin") {
      return NextResponse.redirect(new URL("403", request.url));
    }
  }

  return NextResponse.next();
}

export const config = {
  matcher: [
    /*
     * Match all request paths except:
     * - _next/static (static files)
     * - _next/image (image optimization files)
     * - favicon.ico (favicon file)
     * - public assets (/images, /fonts, etc.)
     */
    "/((?!_next/static|_next/image|favicon.ico|.*\\.(?:svg|png|jpg|jpeg|gif|webp|woff2|woff|ttf)$).*)",
  ],
};
