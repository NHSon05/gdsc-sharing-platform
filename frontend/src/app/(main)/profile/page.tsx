"use client";

import React, { useState } from "react";
import { User, Layers, Loader2, AlertCircle } from "lucide-react";
import {
  ProfileHeader,
  ProfileForm,
  MembershipHistory,
  useProfileQuery,
} from "@/features/profile";
import { Tabs, TabsList, TabsTrigger, TabsContent } from "@/components/ui/tabs";
import { Button } from "@/components/ui/button";
import { useTranslation } from "@/core/i18n/i18n.context";

export default function ProfilePage() {
  const { t } = useTranslation();
  const [activeTab, setActiveTab] = useState("personal");

  const {
    data: profile,
    isLoading,
    isError,
    error,
    refetch,
  } = useProfileQuery();

  if (isLoading) {
    return (
      <div className="mx-auto flex min-h-[60vh] max-w-5xl flex-col items-center justify-center p-6 text-center">
        <Loader2 className="text-brand size-10 animate-spin" />
        <p className="mt-4 text-xs font-semibold text-neutral-500 dark:text-zinc-400">
          Đang tải hồ sơ thành viên...
        </p>
      </div>
    );
  }

  if (isError || !profile) {
    return (
      <div className="mx-auto flex min-h-[60vh] max-w-5xl flex-col items-center justify-center p-6 text-center">
        <div className="mb-4 flex size-14 items-center justify-center rounded-2xl border border-rose-200 bg-rose-50 text-rose-600 dark:border-rose-900 dark:bg-rose-950/40 dark:text-rose-400">
          <AlertCircle className="size-7" />
        </div>
        <h2 className="text-lg font-bold text-neutral-900 dark:text-white">
          Không thể tải thông tin hồ sơ
        </h2>
        <p className="mt-1.5 max-w-md text-xs text-neutral-500 dark:text-zinc-400">
          {error?.message ||
            "Đã xảy ra lỗi trong quá trình truy vấn dữ liệu từ máy chủ."}
        </p>
        <Button
          variant="brand"
          size="md"
          onClick={() => refetch()}
          className="mt-6 font-semibold"
        >
          Thử lại
        </Button>
      </div>
    );
  }

  return (
    <div className="animate-in fade-in mx-auto max-w-5xl space-y-8 px-4 py-8 font-sans duration-300 sm:px-6 lg:px-8">
      {/* Profile Header Banner */}
      <ProfileHeader profile={profile} />

      {/* Tabs Navigation: Personal Information vs Club Membership History */}
      <Tabs value={activeTab} onValueChange={setActiveTab}>
        <TabsList className="mb-6">
          <TabsTrigger value="personal" icon={<User className="size-4" />}>
            {t("profile.tabPersonalInfo")}
          </TabsTrigger>
          <TabsTrigger value="memberships" icon={<Layers className="size-4" />}>
            {t("profile.tabClubHistory")}
          </TabsTrigger>
        </TabsList>

        {/* Tab 1: Personal Info Form */}
        <TabsContent value="personal">
          <ProfileForm profile={profile} />
        </TabsContent>

        {/* Tab 2: Multi-Gen Club Membership History */}
        <TabsContent value="memberships">
          <MembershipHistory memberships={profile.memberships} />
        </TabsContent>
      </Tabs>
    </div>
  );
}
