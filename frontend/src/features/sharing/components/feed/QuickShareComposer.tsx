"use client";

import React, { useState } from "react";
import { Card } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { UserAvatar } from "@/components/ui/user-avatar";
import { ContentFormDialog } from "../content/ContentFormDialog";
import { useTranslation } from "@/core/i18n/i18n.context";
import { useSessionStore } from "@/core/session/session.store";
import { selectCurrentUser } from "@/core/session/session.selectors";
import type { CurrentUserDto } from "@/features/auth/types/auth.types";
import {
  PenSquare,
  Sparkles,
  Tag as TagIcon,
  Image as ImageIcon,
  CheckCircle2,
} from "lucide-react";

interface QuickShareComposerProps {
  user?: CurrentUserDto | null;
  onPostCreated?: () => void;
  className?: string;
}

export function QuickShareComposer({
  user: propUser,
  onPostCreated,
  className,
}: QuickShareComposerProps) {
  const { t } = useTranslation();
  const storeUser = useSessionStore(selectCurrentUser);
  const user = propUser || storeUser;

  const [isDialogOpen, setIsDialogOpen] = useState(false);
  const [showSuccessToast, setShowSuccessToast] = useState(false);

  const displayName = user?.displayName || "Member";

  const handleSuccess = () => {
    setShowSuccessToast(true);
    onPostCreated?.();
    setTimeout(() => {
      setShowSuccessToast(false);
    }, 4000);
  };

  return (
    <>
      <Card
        variant="default"
        className={`group hover:border-brand/50 dark:hover:border-brand/40 relative overflow-hidden rounded-2xl border border-neutral-200/90 bg-white p-5 shadow-xs transition-all duration-200 hover:shadow-md dark:border-zinc-800 dark:bg-zinc-900/95 ${className ?? ""}`}
      >
        {/* Top Header Row */}
        <div className="flex items-center gap-3">
          {/* User Avatar with graceful fallback */}
          <UserAvatar
            name={displayName}
            avatarUrl={user?.avatarUrl}
            size="md"
            isAdmin={user?.roles?.includes("Admin")}
            showAdminBadge={user?.roles?.includes("Admin")}
          />

          {/* Quick Input Trigger with min-w-0 so text truncates cleanly */}
          <button
            type="button"
            onClick={() => setIsDialogOpen(true)}
            className="hover:border-brand/40 dark:hover:border-brand/40 flex h-11 min-w-0 flex-1 items-center rounded-full border border-neutral-200 bg-neutral-50/80 px-4 text-left text-sm text-neutral-500 transition-all hover:bg-neutral-100/80 hover:text-neutral-700 dark:border-zinc-700 dark:bg-zinc-800/60 dark:text-zinc-400 dark:hover:bg-zinc-800 dark:hover:text-zinc-200"
          >
            <span className="truncate">
              {displayName
                ? `${displayName}, ${t("sharing.composerPlaceholder").toLowerCase()}`
                : t("sharing.composerPlaceholder")}
            </span>
          </button>

          {/* Direct CTA with shrink-0 and whitespace-nowrap */}
          <Button
            variant="brand"
            size="md"
            onClick={() => setIsDialogOpen(true)}
            leftIcon={<PenSquare className="size-4" />}
            className="shrink-0 font-semibold whitespace-nowrap shadow-xs"
          >
            {t("sharing.sharePost")}
          </Button>
        </div>

        {/* Bottom Quick Action Pills */}
        <div className="mt-3 flex flex-wrap items-center justify-between gap-2 border-t border-neutral-100 pt-3 dark:border-zinc-800/80">
          <div className="flex items-center gap-1 sm:gap-2">
            <button
              type="button"
              onClick={() => setIsDialogOpen(true)}
              className="hover:bg-brand-muted hover:text-brand dark:hover:bg-brand-muted dark:hover:text-brand inline-flex items-center gap-1.5 rounded-lg px-2.5 py-1.5 text-xs font-medium text-neutral-600 transition-colors dark:text-zinc-400"
            >
              <PenSquare className="text-brand size-3.5" />
              <span>{t("sharing.createPost")}</span>
            </button>

            <button
              type="button"
              onClick={() => setIsDialogOpen(true)}
              className="inline-flex items-center gap-1.5 rounded-lg px-2.5 py-1.5 text-xs font-medium text-neutral-600 transition-colors hover:bg-emerald-50 hover:text-emerald-700 dark:text-zinc-400 dark:hover:bg-emerald-950/40 dark:hover:text-emerald-400"
            >
              <TagIcon className="size-3.5 text-emerald-600 dark:text-emerald-400" />
              <span>{t("sharing.quickTags")}</span>
            </button>

            <button
              type="button"
              onClick={() => setIsDialogOpen(true)}
              className="inline-flex items-center gap-1.5 rounded-lg px-2.5 py-1.5 text-xs font-medium text-neutral-600 transition-colors hover:bg-sky-50 hover:text-sky-700 dark:text-zinc-400 dark:hover:bg-sky-950/40 dark:hover:text-sky-400"
            >
              <ImageIcon className="size-3.5 text-sky-600 dark:text-sky-400" />
              <span>{t("sharing.coverImage")}</span>
            </button>
          </div>

          <span className="hidden items-center gap-1 text-[11px] font-medium text-neutral-400 md:inline-flex dark:text-zinc-500">
            <Sparkles className="text-brand size-3" />
            <span>GDSC Sharing Platform</span>
          </span>
        </div>

        {/* Success Alert Banner */}
        {showSuccessToast && (
          <div className="animate-in fade-in-50 slide-in-from-top-2 mt-3 flex items-center gap-2 rounded-xl border border-emerald-200 bg-emerald-50 px-3.5 py-2.5 text-xs font-semibold text-emerald-800 shadow-xs dark:border-emerald-800/60 dark:bg-emerald-950/50 dark:text-emerald-200">
            <CheckCircle2 className="size-4 shrink-0 text-emerald-600 dark:text-emerald-400" />
            <span>{t("sharing.postSharedSuccess")}</span>
          </div>
        )}
      </Card>

      {/* Creation Dialog */}
      <ContentFormDialog
        isOpen={isDialogOpen}
        onClose={() => setIsDialogOpen(false)}
        onSuccess={handleSuccess}
      />
    </>
  );
}
