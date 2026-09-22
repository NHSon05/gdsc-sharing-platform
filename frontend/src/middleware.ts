import { NextResponse, type NextRequest } from "next/server";
import { AUTH_COOKIE_NAMES } from "@/core/session/session.cookies";

// UX hint only: cookie presence is not proof of identity or authorization.
export function middleware(request: NextRequest) {
  const hasSession = Object.values(AUTH_COOKIE_NAMES).some((name) =>
    request.cookies.has(name)
  );
  if (!hasSession) {
    const login = new URL("/login", request.url);
    login.searchParams.set(
      "returnUrl",
      request.nextUrl.pathname + request.nextUrl.search
    );
    return NextResponse.redirect(login);
  }
  return NextResponse.next();
}
export const config = {
  matcher: [
    "/admin/:path*",
    "/dashboard/:path*",
    "/roadmaps/:path*",
    "/schedule/:path*",
    "/profile/:path*",
    "/sharing/:path*",
  ],
};
