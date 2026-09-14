"use client";

import React from "react";
import Link from "next/link";
import {
  Users,
  FileCheck,
  Calendar,
  ArrowRight,
  ShieldCheck,
} from "lucide-react";

export default function AdminDashboardPage() {
  const cards = [
    {
      title: "Content Review Queue",
      description:
        "Review, approve, or reject articles submitted by GDSC club members.",
      href: "/admin/sharing/review",
      icon: FileCheck,
      color:
        "from-blue-500/10 to-indigo-500/10 border-blue-200 dark:border-blue-900/50",
      iconColor: "text-blue-600 dark:text-blue-400",
    },
    {
      title: "Sharing Schedules",
      description:
        "Plan, publish, update status, and manage attendees for sharing sessions.",
      href: "/admin/sharing/schedules",
      icon: Calendar,
      color:
        "from-violet-500/10 to-purple-500/10 border-violet-200 dark:border-violet-900/50",
      iconColor: "text-violet-600 dark:text-violet-400",
    },
    {
      title: "Member Management",
      description:
        "Manage generations, departments, club roles, and member accounts.",
      href: "/admin/users",
      icon: Users,
      color:
        "from-emerald-500/10 to-teal-500/10 border-emerald-200 dark:border-emerald-900/50",
      iconColor: "text-emerald-600 dark:text-emerald-400",
    },
  ];

  return (
    <div className="mx-auto max-w-7xl space-y-8 px-4 py-8 sm:px-6 lg:px-8">
      <div className="space-y-2 border-b border-neutral-200 pb-5 dark:border-zinc-800">
        <div className="inline-flex items-center gap-1.5 rounded-full border border-blue-200 bg-blue-50 px-3 py-1 text-xs font-semibold text-blue-700 dark:border-blue-900/60 dark:bg-blue-950/50 dark:text-blue-300">
          <ShieldCheck className="size-3.5" />
          <span>Administration Center</span>
        </div>
        <h1 className="text-3xl font-extrabold tracking-tight text-neutral-900 dark:text-white">
          Admin Portal
        </h1>
        <p className="text-sm text-neutral-500 dark:text-zinc-400">
          Centralized management for club content, schedules, and memberships.
        </p>
      </div>

      <div className="grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-3">
        {cards.map((card) => {
          const Icon = card.icon;
          return (
            <Link
              key={card.href}
              href={card.href}
              className={`group flex flex-col justify-between rounded-2xl border bg-linear-to-br p-6 shadow-2xs transition-all duration-200 hover:-translate-y-1 hover:shadow-md ${card.color} bg-white dark:bg-zinc-900`}
            >
              <div className="space-y-3">
                <div
                  className={`flex size-10 items-center justify-center rounded-xl bg-white shadow-2xs dark:bg-zinc-800 ${card.iconColor}`}
                >
                  <Icon className="size-5" />
                </div>
                <h3 className="text-lg font-bold text-neutral-900 transition-colors group-hover:text-blue-600 dark:text-white dark:group-hover:text-blue-400">
                  {card.title}
                </h3>
                <p className="text-xs leading-relaxed text-neutral-600 dark:text-zinc-400">
                  {card.description}
                </p>
              </div>

              <div className="mt-6 flex items-center gap-1.5 border-t border-neutral-100 pt-3 text-xs font-bold text-neutral-700 group-hover:text-blue-600 dark:border-zinc-800/80 dark:text-zinc-300 dark:group-hover:text-blue-400">
                <span>Access Management</span>
                <ArrowRight className="size-3.5 transition-transform group-hover:translate-x-1" />
              </div>
            </Link>
          );
        })}
      </div>
    </div>
  );
}
