import { cookies } from "next/headers";
import { AUTH_COOKIE_NAMES } from "@/core/session/session.cookies";
import { getCurrentUserServerSide } from "@/features/auth/api/auth.server";
import { LandingPageView } from "@/components/home/LandingPageView";
import { AuthenticatedHomeView } from "@/components/home/AuthenticatedHomeView";

export default async function Home() {
  const cookieStore = await cookies();
  if (
    cookieStore.has(AUTH_COOKIE_NAMES.ACCESS_TOKEN) ||
    cookieStore.has(AUTH_COOKIE_NAMES.REFRESH_TOKEN)
  ) {
    return <AuthenticatedHomeView user={await getCurrentUserServerSide()} />;
  }
  return <LandingPageView />;
}
