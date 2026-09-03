"use client";

import React, { useState } from "react";
import Image from "next/image";
import Link from "next/link";
import { usePathname } from "next/navigation";
import logoSvg from "@/assets/images/logo.svg";
import { useTranslation } from "@/core/i18n/i18n.context";
import { useLogoutMutation } from "@/features/auth/hooks/use-logout-mutation";
import type { CurrentUserDto } from "@/features/auth/types/auth.types";
import { LogOut } from "lucide-react";
import {
  Sidebar,
  SidebarHeader,
  SidebarContent,
  SidebarNav,
  SidebarNavItem,
  SidebarAccordion,
  SidebarFooter,
  SidebarUserProfile,
} from "@/components/ui/side-bar";
import { getSidebarNavItems } from "./sidebar.config";

interface AppSidebarProps {
  user?: CurrentUserDto | null;
  collapsed: boolean;
  onToggle?: () => void;
  className?: string;
}

export function AppSidebar({ user, collapsed, className }: AppSidebarProps) {
  const { t } = useTranslation();
  const pathname = usePathname();
  const logoutMutation = useLogoutMutation();

  const [categoriesOpen, setCategoriesOpen] = useState(true);
  const [moreFromUsOpen, setMoreFromUsOpen] = useState(true);

  const { mainNavItems, utilityNavItems, categoryItems, moreItems } =
    getSidebarNavItems(t);

  const displayName = user?.displayName || "User";
  const userRole = user?.roles?.[0] || t("sidebar.proMember");
  const avatarInitial = displayName.charAt(0).toUpperCase();

  const isLinkActive = (href: string, exact = false) => {
    if (href.startsWith("#")) return false;
    if (exact) return pathname === href;
    return pathname === href || pathname.startsWith(`${href}/`);
  };

  return (
    <Sidebar collapsed={collapsed} className={className}>
      {/* Top Content: Brand + Navigation + Accordions */}
      <SidebarContent>
        {/* Brand Header */}
        <SidebarHeader>
          {!collapsed ? (
            <Link href="/" className="flex items-center gap-2.5">
              <Image
                src={logoSvg}
                alt="GDSC Logo"
                width={28}
                height={14}
                className="h-5 w-auto object-contain"
                priority
              />
              <span className="text-sm font-bold tracking-tight text-neutral-900 dark:text-white">
                GDSC <span className="text-brand font-semibold">Platform</span>
              </span>
            </Link>
          ) : (
            <Link href="/" title="GDSC Platform">
              <Image
                src={logoSvg}
                alt="GDSC Logo"
                width={24}
                height={12}
                className="h-5 w-auto object-contain"
                priority
              />
            </Link>
          )}
        </SidebarHeader>

        {/* Primary Navigation */}
        <SidebarNav>
          {mainNavItems.map((item) => (
            <Link key={item.title} href={item.href}>
              <SidebarNavItem
                title={item.title}
                icon={item.icon}
                active={isLinkActive(item.href, item.exact)}
              />
            </Link>
          ))}
        </SidebarNav>

        {/* Utility Items: Search, History, Favorites */}
        <div className="mt-4 border-t border-neutral-200/60 pt-3 dark:border-zinc-800/60">
          <SidebarNav>
            {utilityNavItems.map((item) => (
              <Link key={item.title} href={item.href}>
                <SidebarNavItem
                  title={item.title}
                  icon={item.icon}
                  active={isLinkActive(item.href)}
                />
              </Link>
            ))}
          </SidebarNav>
        </div>

        {/* Accordion 1: Categories */}
        <SidebarAccordion
          title={t("sidebar.categories")}
          isOpen={categoriesOpen}
          onToggle={() => setCategoriesOpen(!categoriesOpen)}
        >
          {categoryItems.map((item) => (
            <Link key={item.title} href={item.href}>
              <SidebarNavItem
                title={item.title}
                icon={item.icon}
                active={isLinkActive(item.href)}
              />
            </Link>
          ))}
        </SidebarAccordion>

        {/* Accordion 2: More From Us */}
        <SidebarAccordion
          title={t("sidebar.moreFromUs")}
          isOpen={moreFromUsOpen}
          onToggle={() => setMoreFromUsOpen(!moreFromUsOpen)}
        >
          {moreItems.map((item) => (
            <Link key={item.title} href={item.href}>
              <SidebarNavItem
                title={item.title}
                icon={item.icon}
                active={isLinkActive(item.href)}
              />
            </Link>
          ))}
        </SidebarAccordion>
      </SidebarContent>

      {/* Bottom Footer: Upgrade Card & User Profile */}
      <SidebarFooter>
        {/* User Profile Pill */}
        <SidebarUserProfile
          displayName={displayName}
          role={userRole}
          avatarInitial={avatarInitial}
          actionSlot={
            <button
              type="button"
              onClick={() => logoutMutation.mutate()}
              disabled={logoutMutation.isPending}
              title={t("common.logout")}
              aria-label={t("common.logout")}
              className="flex size-7 cursor-pointer items-center justify-center rounded-lg text-neutral-400 transition-colors hover:bg-rose-50 hover:text-rose-600 dark:text-zinc-500 dark:hover:bg-rose-950/40 dark:hover:text-rose-400"
            >
              <LogOut className="size-3.5" />
            </button>
          }
        />
      </SidebarFooter>
    </Sidebar>
  );
}
