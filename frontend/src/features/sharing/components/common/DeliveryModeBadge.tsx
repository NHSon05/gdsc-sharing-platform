"use client";

import React from "react";
import { Badge } from "@/components/ui/badge";
import { useTranslation } from "@/core/i18n/i18n.context";
import type { DeliveryMode } from "../../types/sharing.types";
import { MapPin, Video, Network } from "lucide-react";

interface DeliveryModeBadgeProps {
  mode: DeliveryMode;
  className?: string;
}

export function DeliveryModeBadge({ mode, className }: DeliveryModeBadgeProps) {
  const { t } = useTranslation();

  switch (mode) {
    case "Offline":
      return (
        <Badge
          variant="outline"
          className={`inline-flex items-center gap-1 border-neutral-200 bg-neutral-100 font-medium text-neutral-700 dark:border-zinc-700 dark:bg-zinc-800 dark:text-zinc-300 ${className ?? ""}`}
        >
          <MapPin className="size-3 text-neutral-500" />
          <span>{t("sharing.deliveryOffline")}</span>
        </Badge>
      );
    case "Online":
      return (
        <Badge
          variant="outline"
          className={`inline-flex items-center gap-1 border-sky-200 bg-sky-50 font-medium text-sky-700 dark:border-sky-800/60 dark:bg-sky-950/40 dark:text-sky-300 ${className ?? ""}`}
        >
          <Video className="size-3 text-sky-500" />
          <span>{t("sharing.deliveryOnline")}</span>
        </Badge>
      );
    case "Hybrid":
      return (
        <Badge
          variant="outline"
          className={`inline-flex items-center gap-1 border-violet-200 bg-violet-50 font-medium text-violet-700 dark:border-violet-800/60 dark:bg-violet-950/40 dark:text-violet-300 ${className ?? ""}`}
        >
          <Network className="size-3 text-violet-500" />
          <span>{t("sharing.deliveryHybrid")}</span>
        </Badge>
      );
    default:
      return <Badge variant="outline">{mode}</Badge>;
  }
}
