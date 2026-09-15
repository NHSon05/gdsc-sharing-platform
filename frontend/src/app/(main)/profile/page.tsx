"use client";

import React, { useState, Suspense } from "react";
import { useSearchParams, useRouter, usePathname } from "next/navigation";
import { BookOpen, User, Layers } from "lucide-react";
import {
  ProfileHeroCard,
  ProfileSummaryCard,
  ProfileAskMeAboutCard,
  ProfileAdditionalDetailsCard,
  ProfileBlogSection,
  MembershipHistory,
  ProfileEditDialog,
  ProfilePageSkeleton,
  ProfilePageError,
  useProfileQuery,
} from "@/features/profile";
import { Tabs, TabsList, TabsTrigger, TabsContent } from "@/components/ui/tabs";
import { useTranslation } from "@/core/i18n/i18n.context";

type ProfileTab = "posts" | "overview" | "groups";

function ProfilePageContent() {
  const { t } = useTranslation();
  const searchParams = useSearchParams();
  const router = useRouter();
  const pathname = usePathname();

  const [editDialogOpen, setEditDialogOpen] = useState(false);

  const tabParam = searchParams.get("tab") as ProfileTab | null;
  // Default to "posts" (Blog) as requested: user enters profile, this section appears first
  const currentTab: ProfileTab =
    tabParam && ["posts", "overview", "groups"].includes(tabParam)
      ? tabParam
      : "posts";

  const {
    data: profile,
    isLoading,
    isError,
    error,
    refetch,
  } = useProfileQuery();

  const handleTabChange = (value: string) => {
    const params = new URLSearchParams(searchParams.toString());
    params.set("tab", value);
    router.replace(`${pathname}?${params.toString()}`);
  };

  if (isLoading) {
    return <ProfilePageSkeleton />;
  }

  if (isError || !profile) {
    return (
      <ProfilePageError
        errorMessage={error?.message}
        onRetry={() => refetch()}
      />
    );
  }

  return (
    <div className="animate-in fade-in mx-auto max-w-7xl space-y-6 px-4 py-8 font-sans duration-300 sm:px-6 lg:px-8">
      {/* 2-Column Responsive Layout matching reference design */}
      <div className="grid grid-cols-1 gap-6 lg:grid-cols-12">
        {/* Left / Main Column (approx 68% width on desktop) */}
        <div className="space-y-6 lg:col-span-8">
          {/* Top Hero Card (Cover Banner + Avatar + Bio Meta + Social Icons + Edit Button) */}
          <ProfileHeroCard
            profile={profile}
            onEditProfile={() => setEditDialogOpen(true)}
          />

          {/* Navigation Tabs Bar matching reference (Overview, Groups, Posts, etc.) */}
          <Tabs value={currentTab} onValueChange={handleTabChange}>
            <TabsList className="mb-6 rounded-2xl bg-neutral-100/90 p-1.5 dark:bg-zinc-900/90">
              <TabsTrigger
                value="posts"
                icon={<BookOpen className="size-4" />}
                className="rounded-xl px-4 text-xs font-semibold"
              >
                {t("profile.tabBlog") || "Bài viết & Blog (Posts)"}
              </TabsTrigger>
              <TabsTrigger
                value="overview"
                icon={<User className="size-4" />}
                className="rounded-xl px-4 text-xs font-semibold"
              >
                {t("profile.tabPersonalInfo") || "Tổng quan (Overview)"}
              </TabsTrigger>
              <TabsTrigger
                value="groups"
                icon={<Layers className="size-4" />}
                className="rounded-xl px-4 text-xs font-semibold"
              >
                {t("profile.tabClubHistory") || "Nhiệm kỳ CLB (Groups)"}
              </TabsTrigger>
            </TabsList>

            {/* Tab 1: Posts (Blog) - Default */}
            <TabsContent
              value="posts"
              className="space-y-6 focus:outline-hidden"
            >
              <ProfileBlogSection />
            </TabsContent>

            {/* Tab 2: Overview (Summary & Ask Me About cards matching reference image) */}
            <TabsContent
              value="overview"
              className="space-y-6 focus:outline-hidden"
            >
              {/* Summary Card */}
              <ProfileSummaryCard
                bio={profile.bio}
                onEdit={() => setEditDialogOpen(true)}
              />

              {/* Ask Me About Card */}
              <ProfileAskMeAboutCard onEdit={() => setEditDialogOpen(true)} />
            </TabsContent>

            {/* Tab 3: Groups / Membership History */}
            <TabsContent
              value="groups"
              className="space-y-6 focus:outline-hidden"
            >
              <MembershipHistory memberships={profile.memberships} />
            </TabsContent>
          </Tabs>
        </div>

        {/* Right Sidebar Column (approx 32% width on desktop) */}
        <div className="space-y-6 lg:col-span-4">
          {/* Additional Details Card */}
          <ProfileAdditionalDetailsCard
            profile={profile}
            onEdit={() => setEditDialogOpen(true)}
          />
        </div>
      </div>

      {/* Global Profile Edit Dialog */}
      <ProfileEditDialog
        profile={profile}
        open={editDialogOpen}
        onOpenChange={setEditDialogOpen}
      />
    </div>
  );
}

export default function ProfilePage() {
  return (
    <Suspense fallback={<ProfilePageSkeleton />}>
      <ProfilePageContent />
    </Suspense>
  );
}
