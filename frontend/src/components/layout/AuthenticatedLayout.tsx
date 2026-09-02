"use client";

import React, { useState } from "react";
import { AppSidebar } from "./AppSidebar";
import { ThemeToggle } from "@/components/ThemeToggle";
import { LanguageToggle } from "@/components/LanguageToggle";
import { useCurrentUserQuery } from "@/features/auth/hooks/use-current-user-query";
import type { CurrentUserDto } from "@/features/auth/types/auth.types";
import { Menu, X, PanelLeftOpen, PanelLeftClose } from "lucide-react";
import Link from "next/link";
import Image from "next/image";
import logoSvg from "@/assets/images/logo.svg";

interface AuthenticatedLayoutProps {
  children: React.ReactNode;
  user?: CurrentUserDto | null;
}

export function AuthenticatedLayout({
  children,
  user: initialUser,
}: AuthenticatedLayoutProps) {
  const { data: queriedUser } = useCurrentUserQuery();
  const user = initialUser !== undefined ? initialUser : queriedUser;

  const [collapsed, setCollapsed] = useState(false);
  const [mobileOpen, setMobileOpen] = useState(false);

  const displayName = user?.displayName || "Hoang Thuan";
  const avatarInitial = displayName.charAt(0).toUpperCase();

  return (
    <div className="relative flex min-h-dvh w-full bg-[#F4F4F6] font-sans text-neutral-900 transition-colors duration-300 dark:bg-[#09090B] dark:text-zinc-100">
      {/* Desktop Left Sidebar (Sticky) */}
      <div className="hidden shrink-0 md:flex">
        <AppSidebar
          user={user}
          collapsed={collapsed}
          onToggle={() => setCollapsed(!collapsed)}
          className="sticky top-0 h-dvh"
        />
      </div>

      {/* Mobile Slide-Over Drawer with Backdrop */}
      {mobileOpen && (
        <div className="fixed inset-0 z-50 flex md:hidden">
          {/* Backdrop Blur */}
          <div
            onClick={() => setMobileOpen(false)}
            className="animate-in fade-in fixed inset-0 bg-neutral-950/40 backdrop-blur-xs transition-opacity"
          />

          {/* Drawer Content */}
          <div className="animate-in slide-in-from-left relative z-10 flex h-full w-72 flex-col bg-white shadow-2xl duration-300 dark:bg-[#0C0C0E]">
            <button
              type="button"
              onClick={() => setMobileOpen(false)}
              className="absolute top-4 right-4 flex size-8 items-center justify-center rounded-lg text-neutral-500 hover:bg-neutral-100 hover:text-neutral-900 dark:text-zinc-400 dark:hover:bg-zinc-800"
              aria-label="Close sidebar"
            >
              <X className="size-5" />
            </button>
            <AppSidebar
              user={user}
              collapsed={false}
              onToggle={() => setMobileOpen(false)}
              className="h-full border-r-0"
            />
          </div>
        </div>
      )}

      {/* Main Content Area */}
      <div className="flex min-w-0 flex-1 flex-col">
        {/* Top Floating App Bar */}
        <header className="sticky top-0 z-30 flex h-16 w-full items-center justify-between border-b border-neutral-200/70 bg-white/80 px-4 backdrop-blur-xl transition-colors sm:px-6 lg:px-8 dark:border-zinc-800/80 dark:bg-[#09090B]/80">
          {/* Left Controls (Mobile Hamburger & Desktop Toggle) */}
          <div className="flex items-center gap-3">
            {/* Mobile Hamburger */}
            <button
              type="button"
              onClick={() => setMobileOpen(true)}
              className="flex size-9 cursor-pointer items-center justify-center rounded-xl border border-neutral-200/80 bg-white text-neutral-700 shadow-2xs md:hidden dark:border-zinc-800 dark:bg-zinc-900 dark:text-zinc-200"
              aria-label="Open sidebar"
            >
              <Menu className="size-5" />
            </button>

            {/* Mobile Logo Brand */}
            <Link href="/" className="flex items-center gap-2 md:hidden">
              <Image
                src={logoSvg}
                alt="GDSC Logo"
                width={26}
                height={13}
                className="h-4.5 w-auto object-contain"
                priority
              />
              <span className="text-sm font-bold tracking-tight text-neutral-900 dark:text-white">
                GDSC <span className="text-brand font-semibold">Platform</span>
              </span>
            </Link>

            {/* Desktop Collapse Icon in Header for quick access */}
            <button
              type="button"
              onClick={() => setCollapsed(!collapsed)}
              className="hidden size-8 cursor-pointer items-center justify-center rounded-lg text-neutral-500 transition-colors hover:bg-neutral-200/60 hover:text-neutral-900 md:flex dark:text-zinc-400 dark:hover:bg-zinc-800/60 dark:hover:text-white"
              title={collapsed ? "Expand sidebar" : "Collapse sidebar"}
            >
              {collapsed ? (
                <PanelLeftOpen className="size-4.5" />
              ) : (
                <PanelLeftClose className="size-4.5" />
              )}
            </button>
          </div>

          {/* Right Controls: Language, Theme, User Avatar */}
          <div className="flex items-center gap-3">
            <LanguageToggle />
            <ThemeToggle />

            <Link
              href="/profile"
              className="group hover:border-brand/60 flex items-center gap-2 rounded-full border border-neutral-200/80 bg-white/70 py-1 pr-3 pl-1.5 shadow-2xs backdrop-blur-md transition-all dark:border-zinc-800 dark:bg-zinc-900/70"
            >
              <div className="bg-brand flex size-7 items-center justify-center rounded-full text-xs font-bold text-white shadow-xs">
                {avatarInitial}
              </div>
              <span className="hidden max-w-30 truncate text-xs font-semibold text-neutral-800 sm:inline-block dark:text-zinc-200">
                {displayName}
              </span>
            </Link>
          </div>
        </header>

        {/* Inner Page View Content */}
        <main className="w-full flex-1">{children}</main>
      </div>
    </div>
  );
}
