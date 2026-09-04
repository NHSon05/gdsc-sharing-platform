"use client";

import React, { useState } from "react";
import {
  User,
  Phone,
  GraduationCap,
  // Github,
  Check,
  Loader2,
  AlertCircle,
} from "lucide-react";
import type { UserProfileDto } from "../types/profile.types";
import { TextField } from "@/components/ui/input";
import { Button } from "@/components/ui/button";
import { useUpdateProfileMutation } from "../hooks/use-update-profile-mutation";
import { useTranslation } from "@/core/i18n/i18n.context";
import { cn } from "@/lib/utils";

export interface ProfileFormProps {
  profile: UserProfileDto;
  className?: string;
}

export function ProfileForm({ profile, className }: ProfileFormProps) {
  const { t } = useTranslation();

  const [displayName, setDisplayName] = useState(profile.displayName || "");
  const [studentCode, setStudentCode] = useState(profile.studentCode || "");
  const [phoneNumber, setPhoneNumber] = useState(profile.phoneNumber || "");
  const [githubUrl, setGithubUrl] = useState(profile.githubUrl || "");
  const [bio, setBio] = useState(profile.bio || "");

  const [savedSuccess, setSavedSuccess] = useState(false);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);

  const updateMutation = useUpdateProfileMutation();

  const isDirty =
    displayName !== (profile.displayName || "") ||
    studentCode !== (profile.studentCode || "") ||
    phoneNumber !== (profile.phoneNumber || "") ||
    githubUrl !== (profile.githubUrl || "") ||
    bio !== (profile.bio || "");

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    setErrorMessage(null);
    setSavedSuccess(false);

    if (!displayName.trim()) {
      setErrorMessage("Họ và tên không được để trống.");
      return;
    }

    updateMutation.mutate(
      {
        displayName: displayName.trim(),
        studentCode: studentCode.trim() || null,
        phoneNumber: phoneNumber.trim() || null,
        githubUrl: githubUrl.trim() || null,
        bio: bio.trim() || null,
      },
      {
        onSuccess: () => {
          setSavedSuccess(true);
          setTimeout(() => setSavedSuccess(false), 3500);
        },
        onError: (err) => {
          setErrorMessage(err.message || "Không thể cập nhật thông tin hồ sơ.");
        },
      }
    );
  };

  const isPending = updateMutation.isPending;

  return (
    <form
      key={profile.updatedAtUtc || profile.id}
      onSubmit={handleSubmit}
      className={cn(
        "rounded-3xl border border-neutral-200/80 bg-white p-6 shadow-sm transition-all sm:p-8 dark:border-zinc-800/80 dark:bg-[#0C0C0E]",
        className
      )}
    >
      <div className="mb-6 flex flex-col gap-1 border-b border-neutral-100 pb-5 text-left dark:border-zinc-800/80">
        <h2 className="text-lg font-bold tracking-tight text-neutral-900 dark:text-white">
          {t("profile.tabPersonalInfo")}
        </h2>
        <p className="text-xs text-neutral-500 dark:text-zinc-400">
          Cập nhật thông tin cá nhân hiển thị trên hồ sơ thành viên GDSC.
        </p>
      </div>

      {/* Success Alert */}
      {savedSuccess && (
        <div className="animate-in fade-in mb-5 flex items-center gap-2 rounded-2xl border border-emerald-200 bg-emerald-50 p-3.5 text-xs font-semibold text-emerald-800 dark:border-emerald-900/40 dark:bg-emerald-950/40 dark:text-emerald-300">
          <Check className="size-4 shrink-0 text-emerald-600 dark:text-emerald-400" />
          <span>{t("profile.savedSuccess")}</span>
        </div>
      )}

      {/* Error Alert */}
      {errorMessage && (
        <div className="animate-in fade-in mb-5 flex items-center gap-2 rounded-2xl border border-rose-200 bg-rose-50 p-3.5 text-xs text-rose-800 dark:border-rose-900/40 dark:bg-rose-950/40 dark:text-rose-300">
          <AlertCircle className="size-4 shrink-0 text-rose-600 dark:text-rose-400" />
          <span>{errorMessage}</span>
        </div>
      )}

      <div className="grid grid-cols-1 gap-5 sm:grid-cols-2">
        {/* Display Name */}
        <TextField
          id="displayName"
          label={t("profile.displayName")}
          value={displayName}
          onChange={(e) => setDisplayName(e.target.value)}
          placeholder={t("profile.displayNamePlaceholder")}
          required
          disabled={isPending}
          startIcon={<User className="size-4 text-neutral-400" />}
        />

        {/* Student Code */}
        <TextField
          id="studentCode"
          label={t("profile.studentCode")}
          value={studentCode}
          onChange={(e) => setStudentCode(e.target.value)}
          placeholder={t("profile.studentCodePlaceholder")}
          disabled={isPending}
          startIcon={<GraduationCap className="size-4 text-neutral-400" />}
        />

        {/* Phone Number */}
        <TextField
          id="phoneNumber"
          label={t("profile.phoneNumber")}
          value={phoneNumber}
          onChange={(e) => setPhoneNumber(e.target.value)}
          placeholder={t("profile.phoneNumberPlaceholder")}
          disabled={isPending}
          startIcon={<Phone className="size-4 text-neutral-400" />}
        />

        {/* GitHub URL */}
        <TextField
          id="githubUrl"
          label={t("profile.githubUrl")}
          value={githubUrl}
          onChange={(e) => setGithubUrl(e.target.value)}
          placeholder={t("profile.githubUrlPlaceholder")}
          disabled={isPending}
          // startIcon={<Github className="size-4 text-neutral-400" />}
        />

        {/* Bio Textarea (spanning 2 columns) */}
        <div className="sm:col-span-2">
          <label
            htmlFor="bio"
            className="mb-1.5 block text-xs font-semibold text-neutral-700 dark:text-zinc-300"
          >
            {t("profile.bio")}
          </label>
          <div className="relative">
            <textarea
              id="bio"
              rows={4}
              value={bio}
              onChange={(e) => setBio(e.target.value)}
              placeholder={t("profile.bioPlaceholder")}
              maxLength={500}
              disabled={isPending}
              className="focus:border-brand focus:ring-brand/20 w-full rounded-2xl border border-neutral-200/90 bg-white p-3.5 text-xs text-neutral-900 transition-all placeholder:text-neutral-400 focus:ring-2 focus:outline-hidden dark:border-zinc-800 dark:bg-zinc-900/90 dark:text-zinc-100 dark:placeholder:text-zinc-500"
            />
            <div className="absolute right-3 bottom-3 text-[10px] text-neutral-400 dark:text-zinc-500">
              {bio.length}/500
            </div>
          </div>
        </div>
      </div>

      {/* Save Button Bar */}
      <div className="mt-8 flex items-center justify-end gap-3 border-t border-neutral-100 pt-5 dark:border-zinc-800/80">
        <Button
          type="submit"
          variant="brand"
          size="md"
          disabled={!isDirty || isPending}
          leftIcon={
            isPending ? (
              <Loader2 className="size-4 animate-spin" />
            ) : (
              <Check className="size-4" />
            )
          }
          className="min-w-32 font-semibold shadow-md"
        >
          {isPending ? "Đang lưu..." : t("profile.saveChanges")}
        </Button>
      </div>
    </form>
  );
}
