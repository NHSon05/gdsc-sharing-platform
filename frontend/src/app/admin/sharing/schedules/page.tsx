"use client";

import React from "react";
import Link from "next/link";
import { ArrowLeft } from "lucide-react";
import { AdminSchedulesView } from "@/features/sharing";

export default function AdminSchedulesPage() {
  return (
    <div className="mx-auto max-w-7xl space-y-6 px-4 py-8 sm:px-6 lg:px-8">
      <div>
        <Link
          href="/admin?tab=schedules"
          className="inline-flex items-center gap-1.5 text-xs font-semibold text-neutral-500 hover:text-neutral-900 dark:text-zinc-400 dark:hover:text-white"
        >
          <ArrowLeft className="size-3.5" />
          <span>Quay lại Admin Portal</span>
        </Link>
      </div>

      <AdminSchedulesView showHeader={true} />
    </div>
  );
}
