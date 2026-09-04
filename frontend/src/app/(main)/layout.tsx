import React from "react";
import { cookies } from "next/headers";
import { AUTH_COOKIE_NAMES } from "@/core/session/session.cookies";
import {
  getCurrentUserServerSide,
  refreshTokensServerSide,
} from "@/features/auth/api/auth.server";
import { AuthenticatedLayout } from "@/components/layout/AuthenticatedLayout";

export default async function MainLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  const cookieStore = await cookies();
  let accessToken = cookieStore.get(AUTH_COOKIE_NAMES.ACCESS_TOKEN)?.value;
  let refreshToken = cookieStore.get(AUTH_COOKIE_NAMES.REFRESH_TOKEN)?.value;
  let user = await getCurrentUserServerSide(accessToken);

  // If accessToken is missing or expired, but refreshToken is present in cookie:
  // Automatically call the refresh token API on the server!
  if (!user && refreshToken) {
    const refreshed = await refreshTokensServerSide(refreshToken);
    if (refreshed) {
      accessToken = refreshed.accessToken;
      refreshToken = refreshed.refreshToken;
      user = await getCurrentUserServerSide(accessToken);
    }
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
