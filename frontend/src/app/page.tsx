import { cookies } from "next/headers";
import { AUTH_COOKIE_NAMES } from "@/core/session/session.cookies";
import {
  getCurrentUserServerSide,
  refreshTokensServerSide,
} from "@/features/auth/api/auth.server";
import { LandingPageView } from "@/components/home/LandingPageView";
import { AuthenticatedHomeView } from "@/components/home/AuthenticatedHomeView";

export default async function Home() {
  const cookieStore = await cookies();
  let accessToken = cookieStore.get(AUTH_COOKIE_NAMES.ACCESS_TOKEN)?.value;
  let refreshToken = cookieStore.get(AUTH_COOKIE_NAMES.REFRESH_TOKEN)?.value;

  if (accessToken || refreshToken) {
    let user = await getCurrentUserServerSide(accessToken);

    if (!user && refreshToken) {
      const refreshed = await refreshTokensServerSide(refreshToken);
      if (refreshed) {
        accessToken = refreshed.accessToken;
        refreshToken = refreshed.refreshToken;
        user = await getCurrentUserServerSide(accessToken);
      }
    }

    if (user) {
      return (
        <AuthenticatedHomeView
          user={user}
          accessToken={accessToken}
          refreshToken={refreshToken}
        />
      );
    }
  }

  return <LandingPageView />;
}
