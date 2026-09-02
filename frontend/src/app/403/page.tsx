"use client";

import React from "react";
import Link from "next/link";
import { ShieldAlert, ArrowLeft, Home } from "lucide-react";
import { Button } from "@/components/ui/button";

export default function ForbiddenPage() {
  return (
    <div className="flex min-h-dvh flex-col items-center justify-center bg-[#F4F4F6] px-4 text-center font-sans text-neutral-900 transition-colors dark:bg-[#09090B] dark:text-zinc-100">
      <div className="relative mx-auto flex max-w-md flex-col items-center rounded-3xl border border-neutral-200/80 bg-white p-8 shadow-xl dark:border-zinc-800/80 dark:bg-[#0C0C0E]">
        {/* Shield Icon with Brand Ring */}
        <div className="mb-6 flex size-16 items-center justify-center rounded-2xl border border-rose-200 bg-rose-50 text-rose-600 shadow-inner dark:border-rose-900/50 dark:bg-rose-950/40 dark:text-rose-400">
          <ShieldAlert className="size-8" />
        </div>

        <span className="text-xs font-bold tracking-widest text-rose-600 uppercase dark:text-rose-400">
          403 Access Denied
        </span>

        <h1 className="mt-2 text-2xl font-bold tracking-tight text-neutral-900 sm:text-3xl dark:text-white">
          Quyền truy cập bị từ chối
        </h1>

        <p className="mt-3 text-sm leading-relaxed text-neutral-500 dark:text-zinc-400">
          Bạn không có quyền quản trị viên (Admin) để truy cập vào khu vực này.
          Vui lòng liên hệ ban quản trị GDSC nếu bạn cho rằng đây là sự nhầm
          lẫn.
        </p>

        {/* Action Buttons */}
        <div className="mt-8 flex w-full flex-col gap-3 sm:flex-row">
          <Link href="/" className="w-full">
            <Button
              variant="brand"
              size="md"
              leftIcon={<Home className="size-4" />}
              className="w-full font-semibold shadow-md"
            >
              Về Trang chủ
            </Button>
          </Link>
          <button
            type="button"
            onClick={() => window.history.back()}
            className="inline-flex w-full cursor-pointer items-center justify-center gap-2 rounded-xl border border-neutral-200/80 bg-neutral-100 px-4 py-2.5 text-sm font-semibold text-neutral-800 transition-colors hover:bg-neutral-200/60 dark:border-zinc-800 dark:bg-zinc-800 dark:text-zinc-200 dark:hover:bg-zinc-700/60"
          >
            <ArrowLeft className="size-4" />
            <span>Quay lại</span>
          </button>
        </div>
      </div>
    </div>
  );
}
