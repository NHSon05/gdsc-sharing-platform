import React from "react";
import { redirect } from "next/navigation";
import { getCurrentUserServerSide } from "@/features/auth/api/auth.server";
import { AuthenticatedLayout } from "@/components/layout/AuthenticatedLayout";

export default async function AdminLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  const user = await getCurrentUserServerSide();
  if (user && !user.roles.includes("Admin")) redirect("/403");
  // Expired access cookie is restored by /me -> BFF refresh on the client.
  // All backend admin endpoints still enforce their own authorization policies.
  return (
    <AuthenticatedLayout user={user} requiredRole="Admin">
      {children}
    </AuthenticatedLayout>
  );
}
