"use client";

import React from "react";
import { Pencil } from "lucide-react";

export interface ProfileSummaryCardProps {
  bio?: string | null;
  onEdit: () => void;
  className?: string;
}

export function ProfileSummaryCard({
  bio,
  onEdit,
  className = "",
}: ProfileSummaryCardProps) {
  return (
    <div
      className={`rounded-3xl border border-neutral-200/80 bg-white p-6 shadow-xs transition-all sm:p-7 dark:border-zinc-800/80 dark:bg-[#0C0C0E] ${className}`}
    >
      <div className="flex items-center justify-between pb-3">
        <h2 className="text-base font-bold tracking-tight text-neutral-900 dark:text-white">
          Summary
        </h2>
        <button
          type="button"
          onClick={onEdit}
          title="Chỉnh sửa tóm tắt tiểu sử"
          className="flex size-7 cursor-pointer items-center justify-center rounded-lg text-emerald-600 transition-colors hover:bg-emerald-50 dark:text-emerald-400 dark:hover:bg-emerald-950/40"
        >
          <Pencil className="size-3.5" />
        </button>
      </div>

      <div className="pt-2 text-xs leading-relaxed text-neutral-600 dark:text-zinc-400">
        {bio ? (
          <div className="space-y-3 whitespace-pre-line">{bio}</div>
        ) : (
          <p className="italic text-neutral-400 dark:text-zinc-500">
            Chưa có thông tin giới thiệu. Nhấn vào biểu tượng cây bút để cập nhật tóm tắt về bản thân, chuyên môn và định hướng phát triển.
          </p>
        )}
      </div>
    </div>
  );
}
