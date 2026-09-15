"use client";

import React from "react";
import { Info, Pencil, Check, Plus } from "lucide-react";

export interface ProfileAskMeAboutCardProps {
  topics?: { label: string; count?: number; active?: boolean }[];
  onEdit?: () => void;
  className?: string;
}

const DEFAULT_TOPICS = [
  { label: "Web & Frontend Development", count: 12, active: true },
  { label: "Cloud & DevOps Architecture", count: 8, active: true },
  { label: "Google Developer Technologies", count: 15, active: true },
  { label: "AI & Machine Learning Applications", count: 5, active: false },
  { label: "GDSC Community & Event Organization", count: 20, active: true },
];

export function ProfileAskMeAboutCard({
  topics = DEFAULT_TOPICS,
  onEdit,
  className = "",
}: ProfileAskMeAboutCardProps) {
  return (
    <div
      className={`rounded-3xl border border-neutral-200/80 bg-white p-6 shadow-xs transition-all sm:p-7 dark:border-zinc-800/80 dark:bg-[#0C0C0E] ${className}`}
    >
      <div className="flex items-center justify-between pb-3">
        <div className="flex items-center gap-2">
          <h2 className="text-base font-bold tracking-tight text-neutral-900 dark:text-white">
            Ask Me About
          </h2>
          <span
            title="Lĩnh vực chuyên môn và chủ đề bạn sẵn sàng chia sẻ, giải đáp cho các thành viên"
            className="flex size-4 items-center justify-center rounded-full bg-neutral-100 text-neutral-400 dark:bg-zinc-800 dark:text-zinc-500"
          >
            <Info className="size-2.5" />
          </span>
        </div>

        {onEdit && (
          <button
            type="button"
            onClick={onEdit}
            title="Chỉnh sửa chủ đề"
            className="flex size-7 cursor-pointer items-center justify-center rounded-lg text-emerald-600 transition-colors hover:bg-emerald-50 dark:text-emerald-400 dark:hover:bg-emerald-950/40"
          >
            <Pencil className="size-3.5" />
          </button>
        )}
      </div>

      <div className="flex flex-wrap gap-2.5 pt-2">
        {topics.map((topic, idx) => (
          <div
            key={idx}
            className="flex items-center gap-2 rounded-xl border border-neutral-200/80 bg-neutral-50/60 px-3 py-1.5 text-xs font-medium text-neutral-700 transition-colors hover:border-neutral-300 dark:border-zinc-800 dark:bg-zinc-900/60 dark:text-zinc-300 dark:hover:border-zinc-700"
          >
            <span
              className={`flex size-4.5 items-center justify-center rounded-md text-[10px] ${
                topic.active
                  ? "bg-emerald-500 text-white"
                  : "bg-neutral-200 text-neutral-600 dark:bg-zinc-700 dark:text-zinc-300"
              }`}
            >
              {topic.active ? <Check className="size-3" /> : <Plus className="size-3" />}
            </span>
            <span>{topic.label}</span>
            {topic.count !== undefined && (
              <span className="text-[10px] text-neutral-400 dark:text-zinc-500">
                • {topic.count}
              </span>
            )}
          </div>
        ))}
      </div>
    </div>
  );
}
