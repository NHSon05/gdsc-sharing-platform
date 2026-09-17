"use client";

import React from "react";
import { BookOpen, Plus } from "lucide-react";
import { Button } from "@/components/ui/button";

export interface ProfileBlogEmptyStateProps {
  searchTerm?: string;
  activeFilter?: string;
  onCreateNew?: () => void;
  className?: string;
}

export function ProfileBlogEmptyState({
  searchTerm,
  activeFilter = "ALL",
  onCreateNew,
  className = "",
}: ProfileBlogEmptyStateProps) {
  const isFiltered = Boolean(searchTerm) || activeFilter !== "ALL";

  return (
    <div
      className={`flex min-h-72 flex-col items-center justify-center rounded-3xl border border-dashed border-neutral-200 bg-white p-8 text-center dark:border-zinc-800 dark:bg-[#0C0C0E] ${className}`}
    >
      <div className="flex size-14 items-center justify-center rounded-2xl bg-neutral-100 text-neutral-400 dark:bg-zinc-800 dark:text-zinc-500">
        <BookOpen className="size-7" />
      </div>

      <h3 className="mt-3 text-sm font-bold text-neutral-900 dark:text-zinc-200">
        {searchTerm
          ? "Không tìm thấy bài viết nào phù hợp"
          : activeFilter === "ALL"
          ? "Bạn chưa có bài viết chia sẻ nào"
          : "Không có bài viết nào trong trạng thái này"}
      </h3>

      <p className="mt-1 max-w-sm text-xs text-neutral-500 dark:text-zinc-400">
        {searchTerm
          ? "Thử thay đổi từ khóa tìm kiếm hoặc chọn bộ lọc trạng thái khác."
          : "Hãy chia sẻ kinh nghiệm, kiến thức công nghệ hoặc bài hướng dẫn với các thành viên trong câu lạc bộ."}
      </p>

      {!isFiltered && onCreateNew && (
        <Button
          variant="brand"
          size="sm"
          onClick={onCreateNew}
          className="mt-4 font-semibold shadow-2xs"
        >
          <Plus className="mr-1.5 size-3.5" />
          <span>Viết bài chia sẻ đầu tiên</span>
        </Button>
      )}
    </div>
  );
}
