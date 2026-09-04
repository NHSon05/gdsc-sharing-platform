"use client";

import React, { useState } from "react";
import Image from "next/image";
import {
  Camera,
  Mail,
  Shield,
  Sparkles,
  Layers,
  AlertCircle,
  CheckCircle2,
} from "lucide-react";
import type { UserProfileDto } from "../types/profile.types";
import { Progress } from "@/components/ui/progress";
import { AvatarUploader } from "./avatar-uploader";
import { EmailChangeDialog } from "./email-change-dialog";
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

  // Find active generation and primary department
  const activeMembership = profile.memberships?.find((m) => m.isActive);
  const primaryDept =
    activeMembership?.departments?.find((d) => d.isPrimary) ||
    activeMembership?.departments?.[0];

  const completion = profile.profileCompletionPercentage ?? 100;
  const missingFields = profile.missingProfileFields || [];

  return (
    <div
      className={cn(
        "relative flex flex-col gap-6 rounded-3xl border border-neutral-200/80 bg-white p-6 shadow-sm transition-all sm:p-8 dark:border-zinc-800/80 dark:bg-[#0C0C0E]",
        className
      )}
    >
      {/* Top Banner Content: Avatar + User Info */}
      <div className="flex flex-col gap-6 md:flex-row md:items-center md:justify-between">
        <div className="flex flex-col gap-5 sm:flex-row sm:items-center">
          {/* Avatar with Camera Overlay */}
          <div className="group relative size-24 shrink-0 overflow-hidden rounded-3xl border-4 border-white shadow-md ring-2 ring-neutral-200 dark:border-zinc-900 dark:ring-zinc-800">
            {profile.avatarUrl ? (
              <Image
                src={profile.avatarUrl}
                alt={profile.displayName}
                fill
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

          {/* User Meta Information */}
          <div className="space-y-2">
            <div className="flex flex-wrap items-center gap-2">
              <h1 className="text-xl font-bold tracking-tight text-neutral-900 sm:text-2xl dark:text-white">
                {profile.displayName}
              </h1>

              {/* System Roles Badge */}
              <span className="bg-brand/10 text-brand border-brand/30 dark:bg-brand/15 dark:text-brand-hover inline-flex items-center gap-1 rounded-full border px-2.5 py-0.5 text-xs font-semibold">
                <Shield className="size-3.5" />
                <span>{profile.systemRoles || "Member"}</span>
              </span>
            </div>

            {/* Email and Change Trigger */}
            <div className="flex flex-wrap items-center gap-2 text-xs text-neutral-500 dark:text-zinc-400">
              <div className="flex items-center gap-1.5">
                <Mail className="size-3.5" />
                <span>{profile.email}</span>
              </div>
              <span>•</span>
              <button
                type="button"
                onClick={() => setEmailOpen(true)}
                className="text-brand hover:text-brand-hover cursor-pointer font-semibold underline-offset-2 hover:underline"
              >
                {t("profile.emailChangeTitle")}
              </button>
            </div>

            {/* Active Gen & Primary Department Chips */}
            <div className="flex flex-wrap items-center gap-2 pt-1">
              {activeMembership && (
                <span className="inline-flex items-center gap-1.5 rounded-full border border-neutral-200 bg-neutral-100 px-3 py-0.5 text-xs font-medium text-neutral-700 dark:border-zinc-700 dark:bg-zinc-800 dark:text-zinc-300">
                  <Layers className="size-3 text-neutral-400" />
                  <span>
                    {activeMembership.generation.name ||
                      `Gen ${activeMembership.generation.number}`}
                  </span>
                </span>
              )}

              {primaryDept && (
                <span className="bg-brand-muted text-brand inline-flex items-center gap-1.5 rounded-full px-3 py-0.5 text-xs font-semibold">
                  <Sparkles className="size-3" />
                  <span>{primaryDept.department.name}</span>
                </span>
              )}
            </div>
          </div>
        </div>

        {/* Profile Completion Card */}
        <div className="flex w-full flex-col gap-2 rounded-2xl border border-neutral-200/80 bg-neutral-50/70 p-4 sm:max-w-xs dark:border-zinc-800 dark:bg-zinc-900/60">
          <div className="flex items-center justify-between text-xs">
            <span className="flex items-center gap-1.5 font-semibold text-neutral-700 dark:text-zinc-300">
              {completion === 100 ? (
                <CheckCircle2 className="size-3.5 text-emerald-500" />
              ) : (
                <AlertCircle className="size-3.5 text-amber-500" />
              )}
              {t("profile.completionTitle")}
            </span>
            <span className="text-brand font-bold">{completion}%</span>
          </div>

          <Progress value={completion} className="h-2" />

          {missingFields.length > 0 && (
            <div className="text-[11px] leading-tight text-neutral-400 dark:text-zinc-500">
              <span className="font-medium text-neutral-500 dark:text-zinc-400">
                {t("profile.missingFields")}{" "}
              </span>
              <span className="italic">{missingFields.join(", ")}</span>
            </div>
          )}
        </div>
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
