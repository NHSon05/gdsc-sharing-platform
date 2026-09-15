import { NextResponse } from "next/server";
import type { NextRequest } from "next/server";
import {
  AUTH_COOKIE_NAMES,
  AUTH_COOKIE_MAX_AGE,
} from "@/core/session/session.cookies";

const BACKEND_INTERNAL_URL =
  process.env.INTERNAL_API_URL ||
  process.env.BACKEND_API_URL ||
  process.env.NEXT_PUBLIC_API_URL ||
  "http://localhost:5184";

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

function parseRolesFromJwt(token?: string): string[] {
  if (!token) return [];
  try {
    const parts = token.split(".");
    if (parts.length < 2) return [];
    const base64Url = parts[1];
    const base64 = base64Url.replace(/-/g, "+").replace(/_/g, "/");
    const jsonPayload = decodeURIComponent(
      atob(base64)
        .split("")
        .map((c) => "%" + ("00" + c.charCodeAt(0).toString(16)).slice(-2))
        .join("")
    );
    const payload = JSON.parse(jsonPayload);
    const roleClaim =
      payload["role"] ||
      payload["roles"] ||
      payload["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"];
    if (Array.isArray(roleClaim)) return roleClaim;
    if (typeof roleClaim === "string") return [roleClaim];
    return [];
  } catch {
    return [];
  }
}

export async function middleware(request: NextRequest) {
  const { pathname, search } = request.nextUrl;
  let accessToken = request.cookies.get(AUTH_COOKIE_NAMES.ACCESS_TOKEN)?.value;
  const refreshToken = request.cookies.get(
    AUTH_COOKIE_NAMES.REFRESH_TOKEN
  )?.value;

  const isProtectedRoute = PROTECTED_ROUTES.some(
    (route) => pathname === route || pathname.startsWith(`${route}/`)
  );
  const isAuthRoute = AUTH_ROUTES.some(
    (route) => pathname === route || pathname.startsWith(`${route}/`)
  );

  let refreshedResponse: NextResponse | null = null;

  // If accessToken is missing from cookies, but refreshToken is still present:
  // Call the backend refresh API to restore the accessToken from refreshToken.
  if (!accessToken && refreshToken) {
    try {
      const res = await fetch(`${BACKEND_INTERNAL_URL}/api/auth/refresh`, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify({ refreshToken }),
      });

      if (res.ok) {
        const data = (await res.json()) as {
          accessToken: string;
          refreshToken?: string;
          expiresIn?: number;
        };

        accessToken = data.accessToken;
        request.cookies.set(AUTH_COOKIE_NAMES.ACCESS_TOKEN, data.accessToken);

        const isProd = process.env.NODE_ENV === "production";
        refreshedResponse = NextResponse.next({ request });

        refreshedResponse.cookies.set(
          AUTH_COOKIE_NAMES.ACCESS_TOKEN,
          data.accessToken,
          {
            path: "/",
            maxAge: data.expiresIn || AUTH_COOKIE_MAX_AGE.ACCESS_TOKEN,
            sameSite: "lax",
            secure: isProd,
          }
        );

        if (data.refreshToken) {
          request.cookies.set(
            AUTH_COOKIE_NAMES.REFRESH_TOKEN,
            data.refreshToken
          );
          refreshedResponse.cookies.set(
            AUTH_COOKIE_NAMES.REFRESH_TOKEN,
            data.refreshToken,
            {
              path: "/",
              maxAge: AUTH_COOKIE_MAX_AGE.REFRESH_TOKEN,
              sameSite: "lax",
              secure: isProd,
            }
          );
        }
      } else if (isProtectedRoute) {
        // Refresh failed and route is protected -> redirect to login and clear cookies
        const returnUrl = encodeURIComponent(`${pathname}${search}`);
        const loginUrl = new URL(`/login?returnUrl=${returnUrl}`, request.url);
        const redirectRes = NextResponse.redirect(loginUrl);
        redirectRes.cookies.delete(AUTH_COOKIE_NAMES.ACCESS_TOKEN);
        redirectRes.cookies.delete(AUTH_COOKIE_NAMES.REFRESH_TOKEN);
        return redirectRes;
      }
    } catch (err) {
      console.error("[Middleware] Automatic token refresh error:", err);
    }
  }

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
        new URL(`/login?returnUrl=${encodeURIComponent(pathname)}`, request.url)
      );
    }

    const cookieRole = request.cookies.get("userRole")?.value;
    const tokenRoles = parseRolesFromJwt(accessToken);
    const hasAdminRole = cookieRole === "Admin" || tokenRoles.includes("Admin");

    // If we have token/cookie info and user is NOT Admin -> 403
    if ((cookieRole || tokenRoles.length > 0) && !hasAdminRole) {
      return NextResponse.redirect(new URL("/403", request.url));
    }
  }

  return refreshedResponse || NextResponse.next();
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
