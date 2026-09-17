"use client";

import React from "react";
import {
  Pencil,
  Mail,
  GraduationCap,
  Phone,
  Calendar,
  Sparkles,
} from "lucide-react";
import type { UserProfileDto } from "../../types/profile.types";
import { Progress } from "@/components/ui/progress";

export interface ProfileAdditionalDetailsCardProps {
  profile: UserProfileDto;
  onEdit: () => void;
  className?: string;
}

export function ProfileAdditionalDetailsCard({
  profile,
  onEdit,
  className = "",
}: ProfileAdditionalDetailsCardProps) {
  const activeMembership = profile.memberships?.find((m) => m.isActive);
  const primaryDept =
    activeMembership?.departments?.find((d) => d.isPrimary) ||
    activeMembership?.departments?.[0];

  const completion = profile.profileCompletionPercentage ?? 100;

  return (
    <div
      className={`rounded-3xl border border-neutral-200/80 bg-white p-6 shadow-xs transition-all dark:border-zinc-800/80 dark:bg-[#0C0C0E] ${className}`}
    >
      <div className="flex items-center justify-between pb-4">
        <h2 className="text-base font-bold tracking-tight text-neutral-900 dark:text-white">
          Additional Details
        </h2>
        <button
          type="button"
          onClick={onEdit}
          title="Chỉnh sửa thông tin"
          className="flex size-7 cursor-pointer items-center justify-center rounded-lg text-emerald-600 transition-colors hover:bg-emerald-50 dark:text-emerald-400 dark:hover:bg-emerald-950/40"
        >
          <Pencil className="size-3.5" />
        </button>
      </div>

      <div className="space-y-6 pt-3 text-xs">
        {/* Email */}
        <div className="flex items-start gap-3">
          <Mail className="mt-0.5 size-6 shrink-0 text-red-500 dark:text-red-500" />
          <div className="min-w-0 space-y-0.5">
            <span className="text-xs font-bold tracking-wider text-neutral-900 uppercase dark:text-zinc-500">
              EMAIL
            </span>
            <p className="truncate font-medium text-neutral-900 dark:text-zinc-500">
              <a href={`mailto:${profile.email}`} className="hover:underline">
                {profile.email}
              </a>
            </p>
          </div>
        </div>

        {/* Student Code */}
        <div className="flex items-start gap-3">
          <GraduationCap className="mt-0.5 size-6 shrink-0 text-blue-500 dark:text-blue-500" />
          <div className="min-w-0 space-y-0.5">
            <span className="text-xs font-semibold tracking-wider text-neutral-900 uppercase dark:text-zinc-500">
              MÃ SỐ SINH VIÊN
            </span>
            <p className="truncate font-medium text-neutral-900 dark:text-zinc-200">
              {profile.studentCode || "Chưa cập nhật"}
            </p>
          </div>
        </div>

        {/* Phone */}
        <div className="flex items-start gap-3">
          <Phone className="mt-0.5 size-6 shrink-0 text-blue-500 dark:text-blue-500" />
          <div className="min-w-0 space-y-0.5">
            <span className="text-xs font-semibold tracking-wider text-neutral-900 uppercase dark:text-zinc-500">
              SỐ ĐIỆN THOẠI
            </span>
            <p className="truncate font-medium text-neutral-900 dark:text-zinc-200">
              {profile.phoneNumber || "Chưa cập nhật"}
            </p>
          </div>
        </div>

        {/* GitHub */}
        <div className="flex items-start gap-3">
          <div className="mt-0.5 size-6 shrink-0 text-neutral-900 dark:text-zinc-500">
            <svg className="size-6 fill-current" viewBox="0 0 24 24">
              <path d="M12 0C5.37 0 0 5.37 0 12c0 5.31 3.435 9.795 8.205 11.385.6.105.825-.255.825-.57 0-.285-.015-1.23-.015-2.235-3.015.555-3.795-.735-4.035-1.41-.135-.345-.72-1.41-1.23-1.695-.42-.225-1.02-.78-.015-.795.945-.015 1.62.87 1.845 1.23 1.08 1.815 2.805 1.305 3.495.99.105-.78.42-1.305.765-1.605-2.67-.3-5.46-1.335-5.46-5.925 0-1.305.465-2.385 1.23-3.225-.12-.3-.54-1.53.12-3.18 0 0 1.005-.315 3.3 1.23.96-.27 1.98-.405 3-.405s2.04.135 3 .405c2.295-1.56 3.3-1.23 3.3-1.23.66 1.65.24 2.88.12 3.18.765.84 1.23 1.905 1.23 3.225 0 4.605-2.805 5.625-5.475 5.925.435.375.81 1.095.81 2.22 0 1.605-.015 2.895-.015 3.3 0 .315.225.69.825.57A12.02 12.02 0 0024 12c0-6.63-5.37-12-12-12z" />
            </svg>
          </div>
          <div className="min-w-0 space-y-0.5">
            <span className="text-xs font-semibold tracking-wider text-neutral-900 uppercase dark:text-zinc-500">
              GITHUB
            </span>
            <p className="truncate font-medium text-neutral-900 dark:text-zinc-500">
              {profile.githubUrl ? (
                <a
                  href={profile.githubUrl}
                  target="_blank"
                  rel="noopener noreferrer"
                  className="hover:underline"
                >
                  {profile.githubUrl.replace(/^https?:\/\//, "")}
                </a>
              ) : (
                <span className="text-neutral-900 dark:text-zinc-500">
                  Chưa liên kết
                </span>
              )}
            </p>
          </div>
        </div>

        {/* Join Gen & Date */}
        <div className="flex items-start gap-3">
          <Calendar className="mt-0.5 size-6 shrink-0 text-blue-500 dark:text-blue-500" />
          <div className="min-w-0 space-y-0.5">
            <span className="text-xs font-semibold tracking-wider text-neutral-900 uppercase dark:text-zinc-500">
              NHIỆM KỲ
            </span>
            <p className="truncate font-medium text-neutral-900 dark:text-zinc-200">
              {activeMembership
                ? `${activeMembership.generation.name || `Gen ${activeMembership.generation.number}`} (Đang hoạt động)`
                : "Chưa phân nhiệm kỳ"}
            </p>
          </div>
        </div>

        {/* Primary Department */}
        <div className="flex items-start gap-3">
          <Sparkles className="mt-0.5 size-6 shrink-0 text-emerald-500 dark:text-emerald-500" />
          <div className="min-w-0 space-y-0.5">
            <span className="text-xs font-semibold tracking-wider text-neutral-900 uppercase dark:text-zinc-500">
              PHÒNG BAN
            </span>
            <p className="truncate font-medium text-neutral-900 dark:text-zinc-400">
              {primaryDept?.department.name || "Chưa phân ban"}
            </p>
          </div>
        </div>

        {/* Profile Completion */}
        <div className="mt-4 border-t border-neutral-100 pt-4 dark:border-zinc-800">
          <div className="mb-2 flex items-center justify-between">
            <span className="text-[11px] font-semibold text-neutral-600 dark:text-zinc-400">
              Độ hoàn thiện hồ sơ
            </span>
            <span className="text-xs font-bold text-emerald-600 dark:text-emerald-400">
              {completion}%
            </span>
          </div>
          <Progress value={completion} className="h-1.5" />
        </div>
      </div>
    </div>
  );
}
