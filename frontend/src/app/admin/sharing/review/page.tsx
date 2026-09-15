"use client";

import React from "react";
import Link from "next/link";
import { AdminReviewQueue } from "@/features/sharing";
import { ShieldCheck, ArrowLeft } from "lucide-react";

export default function AdminReviewPage() {
  return (
    <div className="mx-auto max-w-7xl space-y-6 px-4 py-8 sm:px-6 lg:px-8">
      {/* Top Header */}
      <div className="flex flex-col gap-2 border-b border-neutral-200 pb-4 sm:flex-row sm:items-center sm:justify-between dark:border-zinc-800">
        <div>
          <div className="flex items-center gap-2">
            <div className="flex size-8 items-center justify-center rounded-lg bg-blue-100 text-blue-600 dark:bg-blue-950/60 dark:text-blue-400">
              <ShieldCheck className="size-4.5" />
            </div>
            <h1 className="text-2xl font-bold tracking-tight text-neutral-900 dark:text-white">
              Hàng chờ phê duyệt & Quản lý bài đăng
            </h1>
          </div>
          <p className="mt-1 text-xs text-neutral-500 dark:text-zinc-400">
            Kiểm duyệt bài viết thành viên gửi lên, duyệt hoặc từ chối kèm góp ý
            trước khi xuất bản.
          </p>
        </div>

        <div>
          <Link
            href="/admin"
            className="inline-flex items-center gap-1 text-xs font-semibold text-neutral-500 hover:text-neutral-900 dark:text-zinc-400 dark:hover:text-white"
          >
            <ArrowLeft className="size-3.5" />
            <span>Quay lại Admin Dashboard</span>
          </Link>
        </div>
      </div>

      {/* Review Queue Component */}
      <AdminReviewQueue initialStatus="PendingReview" />
    </div>
  );
}
