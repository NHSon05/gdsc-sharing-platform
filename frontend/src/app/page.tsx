import { cookies } from "next/headers";
import { AUTH_COOKIE_NAMES } from "@/core/session/session.cookies";
import { LandingPageView } from "@/components/home/LandingPageView";
import { AuthenticatedHomeView } from "@/components/home/AuthenticatedHomeView";

export default async function Home() {
  const cookieStore = await cookies();
  const accessToken = cookieStore.get(AUTH_COOKIE_NAMES.ACCESS_TOKEN)?.value;

  if (accessToken) {
    return <AuthenticatedHomeView />;
  }

  return <LandingPageView />;
}
