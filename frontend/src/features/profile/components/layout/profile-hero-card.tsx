"use client";

import React, { useState } from "react";
import Image from "next/image";
import {
  Camera,
  Pencil,
  Mail,
  Phone,
  Share2,
  Bookmark,
  MapPin,
  Check,
} from "lucide-react";
import type { UserProfileDto } from "../../types/profile.types";
import { AvatarUploader } from "../avatar-uploader";
import { useTranslation } from "@/core/i18n/i18n.context";

export interface ProfileHeroCardProps {
  profile: UserProfileDto;
  onEditProfile: () => void;
  className?: string;
}

export function ProfileHeroCard({
  profile,
  onEditProfile,
  className = "",
}: ProfileHeroCardProps) {
  const { t } = useTranslation();
  const [avatarOpen, setAvatarOpen] = useState(false);
  const [copied, setCopied] = useState(false);
  const [bookmarked, setBookmarked] = useState(false);

  const initialLetter = profile.displayName.charAt(0).toUpperCase() || "U";
  const activeMembership = profile.memberships?.find((m) => m.isActive);
  const primaryDept =
    activeMembership?.departments?.find((d) => d.isPrimary) ||
    activeMembership?.departments?.[0];

  const roleTitle =
    primaryDept?.roles?.[0]?.name ||
    primaryDept?.department.name ||
    (profile.systemRoles === "Admin"
      ? "Quản trị viên (Admin)"
      : "Thành viên GDSC");

  const locationText = activeMembership
    ? `${activeMembership.generation.name || `Gen ${activeMembership.generation.number}`} • GDG On Campus`
    : "Thành viên GDG On Campus";

  const handleCopyLink = () => {
    if (typeof window !== "undefined") {
      navigator.clipboard.writeText(window.location.href);
      setCopied(true);
      setTimeout(() => setCopied(false), 2000);
    }
  };

  return (
    <div
      className={`relative overflow-hidden rounded-3xl border border-neutral-200/80 bg-white shadow-xs transition-all dark:border-zinc-800/80 dark:bg-[#0C0C0E] ${className}`}
    >
      {/* Cover Banner with Tech/Nature Illustrated Theme matching reference */}
      <div className="relative h-44 w-full overflow-hidden bg-linear-to-r from-emerald-500/20 via-teal-500/20 to-lime-500/20 sm:h-52 dark:from-emerald-950/40 dark:via-teal-950/40 dark:to-lime-950/30">
        {/* Subtle patterned SVG overlay */}
        <svg
          className="absolute inset-0 size-full opacity-35 dark:opacity-20"
          xmlns="http://www.w3.org/2000/svg"
          viewBox="0 0 800 400"
          preserveAspectRatio="none"
        >
          <defs>
            <linearGradient id="cover-grad" x1="0%" y1="0%" x2="100%" y2="100%">
              <stop offset="0%" stopColor="#10b981" stopOpacity="0.4" />
              <stop offset="50%" stopColor="#06b6d4" stopOpacity="0.3" />
              <stop offset="100%" stopColor="#84cc16" stopOpacity="0.35" />
            </linearGradient>
            <pattern
              id="grid-pattern"
              width="40"
              height="40"
              patternUnits="userSpaceOnUse"
            >
              <path
                d="M 40 0 L 0 0 0 40"
                fill="none"
                stroke="currentColor"
                strokeWidth="0.5"
                className="text-emerald-600/30"
              />
            </pattern>
          </defs>
          <rect width="800" height="400" fill="url(#cover-grad)" />
          <rect width="800" height="400" fill="url(#grid-pattern)" />
          {/* Isometric hill shapes resembling the reference cover */}
          <path
            d="M-50,300 Q150,150 400,280 T850,220 L850,400 L-50,400 Z"
            fill="#10b981"
            fillOpacity="0.25"
          />
          <path
            d="M100,340 Q350,190 600,310 T950,250 L950,400 L100,400 Z"
            fill="#059669"
            fillOpacity="0.2"
          />
        </svg>

        {/* GDG Tag Badge on Cover */}
        <div className="absolute top-4 right-4 rounded-full bg-white/70 px-3 py-1 text-[11px] font-semibold text-emerald-800 backdrop-blur-md dark:bg-zinc-900/70 dark:text-emerald-400">
          Google Developer Groups
        </div>
      </div>

      {/* Main Profile Info Section */}
      <div className="relative px-6 pt-2 pb-6 sm:px-8">
        {/* Avatar positioned overlapping cover banner */}
        <div className="flex flex-col gap-4 sm:flex-row sm:items-end sm:justify-between">
          <div className="group relative -mt-16 size-28 shrink-0 overflow-hidden rounded-full border-4 border-white bg-white shadow-lg ring-2 ring-neutral-200/80 sm:-mt-20 sm:size-32 dark:border-[#0C0C0E] dark:bg-[#0C0C0E] dark:ring-zinc-800">
            {profile.avatarUrl ? (
              <Image
                src={profile.avatarUrl}
                alt={profile.displayName}
                fill
                sizes="(max-width: 640px) 112px, 128px"
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

          {/* Edit Profile Action Button matching reference */}
          <div className="flex items-center gap-2 self-start sm:self-auto">
            <button
              type="button"
              onClick={onEditProfile}
              className="inline-flex cursor-pointer items-center gap-1.5 rounded-full border border-emerald-500/30 bg-emerald-50/80 px-4 py-1.5 text-xs font-semibold text-emerald-700 shadow-2xs transition-colors hover:bg-emerald-100/90 dark:border-emerald-500/30 dark:bg-emerald-950/40 dark:text-emerald-300 dark:hover:bg-emerald-900/50"
            >
              <Pencil className="size-3.5 text-emerald-600 dark:text-emerald-400" />
              <span>Edit Profile</span>
            </button>
          </div>
        </div>

        {/* User Identity Text */}
        <div className="mt-3 space-y-1">
          <h1 className="text-2xl font-bold tracking-tight text-neutral-900 sm:text-3xl dark:text-white">
            {profile.displayName}
          </h1>

          <p className="text-sm font-medium text-neutral-700 dark:text-zinc-300">
            {roleTitle}
          </p>

          <div className="flex items-center gap-1.5 text-sm text-neutral-900 dark:text-zinc-400">
            <MapPin className="size-4 shrink-0 text-neutral-900 dark:text-zinc-500" />
            <span>{locationText}</span>
          </div>
        </div>

        {/* Quick Social & Contact Action Icons bar matching reference */}
        <div className="mt-5 flex items-center justify-between border-t border-neutral-100 pt-4 dark:border-zinc-800">
          <div className="flex items-center gap-3 text-neutral-600 dark:text-zinc-400">
            {/* Email Icon */}
            <a
              href={`mailto:${profile.email}`}
              title={profile.email}
              className="flex size-8 items-center justify-center rounded-xl bg-neutral-100 text-neutral-700 transition-colors hover:bg-neutral-200 dark:bg-zinc-800 dark:text-zinc-300 dark:hover:bg-zinc-700"
            >
              <Mail className="size-4 text-rose-500" />
            </a>

            {/* GitHub Icon */}
            {profile.githubUrl ? (
              <a
                href={profile.githubUrl}
                target="_blank"
                rel="noopener noreferrer"
                title="GitHub Profile"
                className="flex size-8 items-center justify-center rounded-xl bg-neutral-100 text-neutral-700 transition-colors hover:bg-neutral-200 dark:bg-zinc-800 dark:text-zinc-300 dark:hover:bg-zinc-700"
              >
                <svg className="size-4 fill-current" viewBox="0 0 24 24">
                  <path d="M12 0C5.37 0 0 5.37 0 12c0 5.31 3.435 9.795 8.205 11.385.6.105.825-.255.825-.57 0-.285-.015-1.23-.015-2.235-3.015.555-3.795-.735-4.035-1.41-.135-.345-.72-1.41-1.23-1.695-.42-.225-1.02-.78-.015-.795.945-.015 1.62.87 1.845 1.23 1.08 1.815 2.805 1.305 3.495.99.105-.78.42-1.305.765-1.605-2.67-.3-5.46-1.335-5.46-5.925 0-1.305.465-2.385 1.23-3.225-.12-.3-.54-1.53.12-3.18 0 0 1.005-.315 3.3 1.23.96-.27 1.98-.405 3-.405s2.04.135 3 .405c2.295-1.56 3.3-1.23 3.3-1.23.66 1.65.24 2.88.12 3.18.765.84 1.23 1.905 1.23 3.225 0 4.605-2.805 5.625-5.475 5.925.435.375.81 1.095.81 2.22 0 1.605-.015 2.895-.015 3.3 0 .315.225.69.825.57A12.02 12.02 0 0024 12c0-6.63-5.37-12-12-12z" />
                </svg>
              </a>
            ) : null}

            {/* Phone Icon */}
            {profile.phoneNumber ? (
              <a
                href={`tel:${profile.phoneNumber}`}
                title={profile.phoneNumber}
                className="flex size-8 items-center justify-center rounded-xl bg-neutral-100 text-neutral-700 transition-colors hover:bg-neutral-200 dark:bg-zinc-800 dark:text-zinc-300 dark:hover:bg-zinc-700"
              >
                <Phone className="size-4 text-blue-500" />
              </a>
            ) : null}

            {/* Share / Copy Profile URL */}
            <button
              type="button"
              onClick={handleCopyLink}
              title="Sao chép liên kết hồ sơ"
              className="flex size-8 cursor-pointer items-center justify-center rounded-xl bg-neutral-100 text-neutral-700 transition-colors hover:bg-neutral-200 dark:bg-zinc-800 dark:text-zinc-300 dark:hover:bg-zinc-700"
            >
              {copied ? (
                <Check className="size-4 text-emerald-600 dark:text-emerald-400" />
              ) : (
                <Share2 className="size-4 text-indigo-500" />
              )}
            </button>
          </div>

          {/* Bookmark Button */}
          <button
            type="button"
            onClick={() => setBookmarked(!bookmarked)}
            aria-label="Lưu hồ sơ"
            className="flex size-8 cursor-pointer items-center justify-center rounded-xl text-neutral-400 transition-colors hover:bg-neutral-100 hover:text-neutral-700 dark:text-zinc-500 dark:hover:bg-zinc-800 dark:hover:text-zinc-200"
          >
            <Bookmark
              className={`size-4.5 ${
                bookmarked
                  ? "fill-emerald-600 text-emerald-600 dark:fill-emerald-400 dark:text-emerald-400"
                  : ""
              }`}
            />
          </button>
        </div>
      </div>

      {/* Avatar Uploader Dialog */}
      <AvatarUploader
        currentAvatarUrl={profile.avatarUrl}
        displayName={profile.displayName}
        open={avatarOpen}
        onOpenChange={setAvatarOpen}
      />
    </div>
  );
}
