"use client";

import React from "react";
import { cn } from "@/lib/utils";
import { Shield } from "lucide-react";

export interface UserAvatarProps {
  name?: string | null;
  avatarUrl?: string | null;
  avatarInitial?: string;
  size?: "xs" | "sm" | "md" | "lg" | "xl";
  isAdmin?: boolean;
  showAdminBadge?: boolean;
  className?: string;
  fallbackClassName?: string;
}

export function getAvatarInitials(name?: string | null): string {
  if (!name || !name.trim()) return "U";
  const parts = name.trim().split(/\s+/).filter(Boolean);
  if (parts.length >= 2) {
    return (parts[0][0] + parts[parts.length - 1][0]).toUpperCase();
  }
  const single = parts[0];
  return single.length >= 2
    ? single.slice(0, 2).toUpperCase()
    : single.toUpperCase();
}

const sizeClasses = {
  xs: "size-6 text-[10px]",
  sm: "size-8 text-xs",
  md: "size-10 text-sm",
  lg: "size-11 text-sm sm:size-12 sm:text-base",
  xl: "size-14 text-base sm:size-16 sm:text-lg",
};

const badgeSizeClasses = {
  xs: "size-2.5 p-0.5",
  sm: "size-3 p-0.5",
  md: "size-3.5 p-0.5",
  lg: "size-4 p-0.5",
  xl: "size-4.5 p-1",
};

export function UserAvatar({
  name,
  avatarUrl,
  avatarInitial,
  size = "md",
  isAdmin: propIsAdmin,
  showAdminBadge = false,
  className,
  fallbackClassName,
}: UserAvatarProps) {
  const [imageError, setImageError] = React.useState(false);

  const trimmedName = name?.trim() || "";
  const initials = avatarInitial?.trim() || getAvatarInitials(trimmedName);

  const isAdmin =
    propIsAdmin !== undefined
      ? propIsAdmin
      : trimmedName === "System Administrator" ||
        trimmedName.toLowerCase().includes("admin");

  return (
    <div
      className={cn(
        "relative inline-flex shrink-0 select-none items-center justify-center rounded-full font-bold transition-transform",
        sizeClasses[size],
        isAdmin
          ? "ring-2 ring-blue-500/30 dark:ring-blue-400/40"
          : "ring-2 ring-brand/20 dark:ring-brand/30",
        className
      )}
    >
      {avatarUrl && !imageError ? (
        // eslint-disable-next-line @next/next/no-img-element
        <img
          src={avatarUrl}
          alt={trimmedName || "Avatar"}
          onError={() => setImageError(true)}
          className="size-full rounded-full object-cover"
        />
      ) : (
        <div
          className={cn(
            "flex size-full items-center justify-center rounded-full tracking-wider",
            isAdmin
              ? "bg-blue-50 text-blue-700 dark:bg-blue-950/80 dark:text-blue-300"
              : "bg-brand/10 text-brand dark:bg-brand/20 dark:text-brand",
            fallbackClassName
          )}
        >
          {initials}
        </div>
      )}

      {showAdminBadge && isAdmin && (
        <span
          className={cn(
            "absolute -right-0.5 -bottom-0.5 flex items-center justify-center rounded-full bg-blue-600 text-white shadow-xs ring-2 ring-white dark:ring-zinc-900",
            badgeSizeClasses[size]
          )}
          title="Admin"
        >
          <Shield className="size-full" />
        </span>
      )}
    </div>
  );
}
