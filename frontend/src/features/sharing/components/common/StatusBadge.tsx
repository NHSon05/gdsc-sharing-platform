"use client";

import React from "react";
import { Badge } from "@/components/ui/badge";
import { useTranslation } from "@/core/i18n/i18n.context";
import type {
  SharingContentStatus,
  SharingScheduleStatus,
} from "../../types/sharing.types";
import {
  FileEdit,
  Clock,
  CheckCircle2,
  XCircle,
  Archive,
  Calendar,
  PlayCircle,
} from "lucide-react";

interface StatusBadgeProps {
  status: SharingContentStatus | SharingScheduleStatus;
  className?: string;
}

export function StatusBadge({ status, className }: StatusBadgeProps) {
  const { t } = useTranslation();

  switch (status) {
    // Content statuses
    case "Draft":
      return (
        <Badge
          variant="secondary"
          className={`inline-flex items-center gap-1 font-medium ${className ?? ""}`}
        >
          <FileEdit className="size-3" />
          <span>{t("sharing.statusDraft")}</span>
        </Badge>
      );
    case "PendingReview":
      return (
        <Badge
          variant="outline"
          className={`inline-flex items-center gap-1 border-amber-200 bg-amber-50 font-medium text-amber-700 dark:border-amber-800/60 dark:bg-amber-950/40 dark:text-amber-300 ${className ?? ""}`}
        >
          <Clock className="size-3" />
          <span>{t("sharing.statusPendingReview")}</span>
        </Badge>
      );
    case "Published":
      return (
        <Badge
          variant="outline"
          className={`inline-flex items-center gap-1 border-emerald-200 bg-emerald-50 font-medium text-emerald-700 dark:border-emerald-800/60 dark:bg-emerald-950/40 dark:text-emerald-300 ${className ?? ""}`}
        >
          <CheckCircle2 className="size-3" />
          <span>{t("sharing.statusPublished")}</span>
        </Badge>
      );
    case "Rejected":
      return (
        <Badge
          variant="destructive"
          className={`inline-flex items-center gap-1 font-medium ${className ?? ""}`}
        >
          <XCircle className="size-3" />
          <span>{t("sharing.statusRejected")}</span>
        </Badge>
      );
    case "Archived":
      return (
        <Badge
          variant="secondary"
          className={`inline-flex items-center gap-1 font-medium ${className ?? ""}`}
        >
          <Archive className="size-3" />
          <span>{t("sharing.statusArchived")}</span>
        </Badge>
      );

    // Schedule statuses
    case "Scheduled":
      return (
        <Badge
          variant="outline"
          className={`inline-flex items-center gap-1 border-blue-200 bg-blue-50 font-medium text-blue-700 dark:border-blue-800/60 dark:bg-blue-950/40 dark:text-blue-300 ${className ?? ""}`}
        >
          <Calendar className="size-3" />
          <span>{t("sharing.statusScheduled")}</span>
        </Badge>
      );
    case "InProgress":
      return (
        <Badge
          variant="outline"
          className={`inline-flex animate-pulse items-center gap-1 border-amber-200 bg-amber-50 font-medium text-amber-700 dark:border-amber-800/60 dark:bg-amber-950/40 dark:text-amber-300 ${className ?? ""}`}
        >
          <PlayCircle className="size-3" />
          <span>{t("sharing.statusInProgress")}</span>
        </Badge>
      );
    case "Completed":
      return (
        <Badge
          variant="outline"
          className={`inline-flex items-center gap-1 border-emerald-200 bg-emerald-50 font-medium text-emerald-700 dark:border-emerald-800/60 dark:bg-emerald-950/40 dark:text-emerald-300 ${className ?? ""}`}
        >
          <CheckCircle2 className="size-3" />
          <span>{t("sharing.statusCompleted")}</span>
        </Badge>
      );
    case "Cancelled":
      return (
        <Badge
          variant="destructive"
          className={`inline-flex items-center gap-1 font-medium ${className ?? ""}`}
        >
          <XCircle className="size-3" />
          <span>{t("sharing.statusCancelled")}</span>
        </Badge>
      );
    default:
      return <Badge variant="secondary">{status}</Badge>;
  }
}
