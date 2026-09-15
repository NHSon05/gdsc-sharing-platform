"use client";

import React from "react";
import { AlertCircle } from "lucide-react";
import { Button } from "@/components/ui/button";

export interface ProfilePageErrorProps {
  errorMessage?: string;
  onRetry: () => void;
  className?: string;
}

export function ProfilePageError({
  errorMessage = "Đã xảy ra lỗi trong quá trình truy vấn dữ liệu từ máy chủ.",
  onRetry,
  className = "",
}: ProfilePageErrorProps) {
  return (
    <div
      className={`mx-auto flex min-h-[60vh] max-w-5xl flex-col items-center justify-center p-6 text-center ${className}`}
    >
      <div className="mb-4 flex size-14 items-center justify-center rounded-2xl border border-rose-200 bg-rose-50 text-rose-600 dark:border-rose-900 dark:bg-rose-950/40 dark:text-rose-400">
        <AlertCircle className="size-7" />
      </div>

      <h2 className="text-lg font-bold text-neutral-900 dark:text-white">
        Không thể tải thông tin hồ sơ
      </h2>

      <p className="mt-1.5 max-w-md text-xs text-neutral-500 dark:text-zinc-400">
        {errorMessage}
      </p>

      <Button
        variant="brand"
        size="md"
        onClick={onRetry}
        className="mt-6 font-semibold"
      >
        Thử lại
      </Button>
    </div>
  );
}
