"use client";

import React, { Suspense } from "react";
import { useSearchParams, useRouter, usePathname } from "next/navigation";
import {
  Users,
  FileCheck,
  Calendar,
  ShieldCheck,
  Loader2,
} from "lucide-react";
import { Tabs, TabsList, TabsTrigger, TabsContent } from "@/components/ui/tabs";
import { AdminUsersView } from "@/features/member-management";
import { AdminReviewQueue, AdminSchedulesView } from "@/features/sharing";

type AdminTab = "members" | "contents" | "schedules";

function AdminDashboardContent() {
  const searchParams = useSearchParams();
  const router = useRouter();
  const pathname = usePathname();

  const tabParam = searchParams.get("tab") as AdminTab | null;
  const currentTab: AdminTab =
    tabParam && ["members", "contents", "schedules"].includes(tabParam)
      ? tabParam
      : "members";

  const handleTabChange = (value: string) => {
    const params = new URLSearchParams(searchParams.toString());
    params.set("tab", value);
    router.replace(`${pathname}?${params.toString()}`);
  };

  return (
    <div className="mx-auto max-w-7xl space-y-8 px-4 py-8 sm:px-6 lg:px-8">
      {/* Top Banner Header */}
      <div className="flex flex-col gap-3 border-b border-neutral-200 pb-5 sm:flex-row sm:items-end sm:justify-between dark:border-zinc-800">
        <div className="space-y-1.5">
          <div className="inline-flex items-center gap-1.5 rounded-full border border-blue-200 bg-blue-50 px-3 py-1 text-xs font-semibold text-blue-700 dark:border-blue-900/60 dark:bg-blue-950/50 dark:text-blue-300">
            <ShieldCheck className="size-3.5" />
            <span>Administration Center</span>
          </div>
          <h1 className="text-3xl font-extrabold tracking-tight text-neutral-900 dark:text-white">
            Admin Portal
          </h1>
          <p className="text-sm text-neutral-500 dark:text-zinc-400">
            Quản lý tập trung thành viên, bài viết chia sẻ và lịch sinh hoạt câu lạc bộ.
          </p>
        </div>
      </div>

      {/* Main Views Container (Replaced 3 cards with direct interactive views) */}
      <Tabs value={currentTab} onValueChange={handleTabChange} className="space-y-6">
        <div className="border-b border-neutral-200/80 pb-1 dark:border-zinc-800/80">
          <TabsList className="h-11 p-1">
            <TabsTrigger
              value="members"
              icon={<Users className="size-4" />}
              className="gap-2 px-4 py-2 text-xs font-semibold"
            >
              <span>Quản lý Thành viên</span>
            </TabsTrigger>

            <TabsTrigger
              value="contents"
              icon={<FileCheck className="size-4" />}
              className="gap-2 px-4 py-2 text-xs font-semibold"
            >
              <span>Quản lý Bài đăng</span>
            </TabsTrigger>

            <TabsTrigger
              value="schedules"
              icon={<Calendar className="size-4" />}
              className="gap-2 px-4 py-2 text-xs font-semibold"
            >
              <span>Lịch chia sẻ</span>
            </TabsTrigger>
          </TabsList>
        </div>

        {/* VIEW 1: Quản lý thành viên (Member Management) */}
        <TabsContent value="members" className="outline-hidden">
          <div className="rounded-3xl border border-neutral-200/70 bg-neutral-50/30 p-4 sm:p-6 dark:border-zinc-800/70 dark:bg-zinc-950/30">
            <AdminUsersView showHeader={false} />
          </div>
        </TabsContent>

        {/* VIEW 2: Quản lý bài đăng (Content Management) */}
        <TabsContent value="contents" className="outline-hidden">
          <div className="rounded-3xl border border-neutral-200/70 bg-neutral-50/30 p-4 sm:p-6 dark:border-zinc-800/70 dark:bg-zinc-950/30">
            <AdminReviewQueue initialStatus="ALL" />
          </div>
        </TabsContent>

        {/* VIEW 3: Lịch chia sẻ (Sharing Schedules) */}
        <TabsContent value="schedules" className="outline-hidden">
          <div className="rounded-3xl border border-neutral-200/70 bg-neutral-50/30 p-4 sm:p-6 dark:border-zinc-800/70 dark:bg-zinc-950/30">
            <AdminSchedulesView showHeader={false} />
          </div>
        </TabsContent>
      </Tabs>
    </div>
  );
}

export default function AdminDashboardPage() {
  return (
    <Suspense
      fallback={
        <div className="flex min-h-96 items-center justify-center">
          <Loader2 className="size-8 animate-spin text-neutral-400" />
        </div>
      }
    >
      <AdminDashboardContent />
    </Suspense>
  );
}
