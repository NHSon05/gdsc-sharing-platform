import React from "react";
import { cookies } from "next/headers";
import { redirect } from "next/navigation";
import { AUTH_COOKIE_NAMES } from "@/core/session/session.cookies";
import {
  getCurrentUserServerSide,
  refreshTokensServerSide,
} from "@/features/auth/api/auth.server";
import { AuthenticatedLayout } from "@/components/layout/AuthenticatedLayout";

export default async function AdminLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  const cookieStore = await cookies();
  let accessToken = cookieStore.get(AUTH_COOKIE_NAMES.ACCESS_TOKEN)?.value;
  let refreshToken = cookieStore.get(AUTH_COOKIE_NAMES.REFRESH_TOKEN)?.value;

  // 1. Fetch user directly on Server
  let user = await getCurrentUserServerSide(accessToken);

  // If accessToken expired, attempt token refresh on server using refreshToken
  if (!user && refreshToken) {
    const refreshed = await refreshTokensServerSide(refreshToken);
    if (refreshed) {
      accessToken = refreshed.accessToken;
      refreshToken = refreshed.refreshToken;
      user = await getCurrentUserServerSide(accessToken);
    }
  }

  // 2. If unauthenticated -> redirect to login with returnUrl
  if (!user) {
    redirect("/login?returnUrl=/admin");
  }

  // 3. Authorization Check: Must possess Admin role
  const isAdmin = user.roles?.includes("Admin");
  if (!isAdmin) {
    redirect("/403");
  }

  return (
    <AuthenticatedLayout
      user={user}
      accessToken={accessToken}
      refreshToken={refreshToken}
    >
      {children}
    </AuthenticatedLayout>
  );
}
