import React from "react";
import { redirect } from "next/navigation";
import { getCurrentUserServerSide } from "@/features/auth/api/auth.server";
import { AuthenticatedLayout } from "@/components/layout/AuthenticatedLayout";

export default async function AdminLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  // 1. Fetch user directly on Server
  const user = await getCurrentUserServerSide();

  // 2. If unauthenticated -> redirect to login with returnUrl
  if (!user) {
    redirect("/login?returnUrl=/admin");
  }

  // 3. Authorization Check: Must possess Admin role
  const isAdmin = user.roles?.includes("Admin");
  if (!isAdmin) {
    redirect("/403");
  }

  return <AuthenticatedLayout user={user}>{children}</AuthenticatedLayout>;
}
