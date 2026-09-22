"use client";

import React, { useState } from "react";
import Image from "next/image";
import { Camera } from "lucide-react";
import type { UserProfileDto } from "../types/profile.types";
import { AvatarUploader } from "./avatar-uploader";
import { EmailChangeDialog } from "./email-change-dialog";
import { ProfileUserInfo } from "./header/profile-user-info";
import { ProfileCompletionCard } from "./header/profile-completion-card";
import { useTranslation } from "@/core/i18n/i18n.context";
import { cn } from "@/lib/utils";

export interface ProfileHeaderProps {
  profile: UserProfileDto;
  className?: string;
}

export function ProfileHeader({ profile, className }: ProfileHeaderProps) {
  const { t } = useTranslation();
  const [avatarOpen, setAvatarOpen] = useState(false);
  const [emailOpen, setEmailOpen] = useState(false);

  const initialLetter = profile.displayName.charAt(0).toUpperCase() || "U";

  return (
    <div
      className={cn(
        "relative flex flex-col gap-6 rounded-3xl border border-neutral-200/80 bg-white p-6 shadow-sm transition-all sm:p-8 dark:border-zinc-800/80 dark:bg-[#0C0C0E]",
        className
      )}
    >
      {/* Top Banner Content: Avatar + User Info + Completion Card */}
      <div className="flex flex-col gap-6 md:flex-row md:items-center md:justify-between">
        <div className="flex flex-col gap-5 sm:flex-row sm:items-center">
          {/* Avatar with Camera Overlay */}
          <div className="group relative size-24 shrink-0 overflow-hidden rounded-3xl border-4 border-white shadow-md ring-2 ring-neutral-200 dark:border-zinc-900 dark:ring-zinc-800">
            {profile.avatarUrl ? (
              <Image
                src={profile.avatarUrl}
                unoptimized={profile.avatarUrl.startsWith("https://")}
                referrerPolicy="no-referrer"
                alt={profile.displayName}
                fill
                sizes="96px"
                className="object-cover transition-transform duration-300 group-hover:scale-105"
              />
            ) : (
              <div className="bg-brand text-brand-foreground flex size-full items-center justify-center text-3xl font-bold">
                {initialLetter}
              </div>
            )}

            {/* Hover Camera Overlay Button */}
            <button
              type="button"
              onClick={() => setAvatarOpen(true)}
              aria-label={t("profile.changeAvatar")}
              className="backdrop-blur-2xs absolute inset-0 flex cursor-pointer items-center justify-center bg-black/40 text-white opacity-0 transition-opacity duration-200 group-hover:opacity-100"
            >
              <Camera className="size-6" />
            </button>
          </div>

          {/* User Information Details */}
          <ProfileUserInfo
            profile={profile}
            onOpenEmailChange={() => setEmailOpen(true)}
          />
        </div>

        {/* Profile Completion Indicator */}
        <ProfileCompletionCard
          completionPercentage={profile.profileCompletionPercentage ?? 100}
          missingFields={profile.missingProfileFields || []}
        />
      </div>

      {/* Avatar Uploader Dialog */}
      <AvatarUploader
        currentAvatarUrl={profile.avatarUrl}
        displayName={profile.displayName}
        open={avatarOpen}
        onOpenChange={setAvatarOpen}
      />

      {/* Email Change Dialog */}
      <EmailChangeDialog
        currentEmail={profile.email}
        open={emailOpen}
        onOpenChange={setEmailOpen}
      />
    </div>
  );
}
