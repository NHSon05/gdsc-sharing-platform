import React from "react";
import { getCurrentUserServerSide } from "@/features/auth/api/auth.server";
import { AuthenticatedLayout } from "@/components/layout/AuthenticatedLayout";

export default async function MainLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  const user = await getCurrentUserServerSide();
  return <AuthenticatedLayout user={user}>{children}</AuthenticatedLayout>;
}
