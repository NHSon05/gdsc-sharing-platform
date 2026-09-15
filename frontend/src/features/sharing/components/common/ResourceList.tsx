"use client";

import React, { useState } from "react";
import type { ResourceResponse } from "../../types/sharing.types";
import { downloadSharingResourceFile } from "../../api/sharing-resource.api";
import { useTranslation } from "@/core/i18n/i18n.context";
import {
  ExternalLink,
  Download,
  FileText,
  Link as LinkIcon,
  Loader2,
  FileCode,
  Video,
  Presentation,
} from "lucide-react";
import { Badge } from "@/components/ui/badge";

interface ResourceListProps {
  resources: ResourceResponse[];
  className?: string;
}

function formatFileSize(bytes?: number | null): string {
  if (!bytes) return "";
  if (bytes < 1024) return `${bytes} B`;
  if (bytes < 1024 * 1024) return `${Math.round(bytes / 1024)} KB`;
  return `${(bytes / (1024 * 1024)).toFixed(1)} MB`;
}

function getResourceIcon(type: string) {
  switch (type) {
    case "Link":
      return <LinkIcon className="size-4 text-blue-500" />;
    case "Slide":
      return <Presentation className="size-4 text-amber-500" />;
    case "CodeSample":
      return <FileCode className="size-4 text-emerald-500" />;
    case "Recording":
      return <Video className="size-4 text-purple-500" />;
    default:
      return (
        <FileText className="size-4 text-neutral-500 dark:text-zinc-400" />
      );
  }
}

export function ResourceList({ resources, className }: ResourceListProps) {
  const { t } = useTranslation();
  const [downloadingId, setDownloadingId] = useState<string | null>(null);

  if (!resources || resources.length === 0) {
    return null;
  }

  const handleDownload = async (resource: ResourceResponse) => {
    try {
      setDownloadingId(resource.id);
      await downloadSharingResourceFile(
        resource.id,
        resource.originalFileName || resource.title
      );
    } catch (err) {
      console.error("Failed to download resource file", err);
    } finally {
      setDownloadingId(null);
    }
  };

  return (
    <div className={`space-y-2 ${className ?? ""}`}>
      {resources.map((item) => {
        const isFile = Boolean(item.originalFileName);

        return (
          <div
            key={item.id}
            className="flex items-center justify-between gap-3 rounded-xl border border-neutral-200 bg-white p-3 shadow-2xs transition-all hover:border-neutral-300 dark:border-zinc-800 dark:bg-zinc-900/60 dark:hover:border-zinc-700"
          >
            <div className="flex min-w-0 items-start gap-3">
              <div className="mt-0.5 flex size-8 shrink-0 items-center justify-center rounded-lg bg-neutral-100 dark:bg-zinc-800">
                {getResourceIcon(item.resourceType)}
              </div>
              <div className="min-w-0">
                <div className="flex items-center gap-2">
                  <h4 className="truncate text-sm font-semibold text-neutral-900 dark:text-white">
                    {item.title}
                  </h4>
                  <Badge
                    variant="secondary"
                    className="px-1.5 py-0 text-[10px]"
                  >
                    {item.resourceType}
                  </Badge>
                </div>
                {item.description && (
                  <p className="mt-0.5 truncate text-xs text-neutral-500 dark:text-zinc-400">
                    {item.description}
                  </p>
                )}
                {item.fileSize && (
                  <span className="text-[11px] text-neutral-400 dark:text-zinc-500">
                    {formatFileSize(item.fileSize)}
                  </span>
                )}
              </div>
            </div>

            {/* Action buttons */}
            <div className="shrink-0">
              {isFile ? (
                <button
                  type="button"
                  disabled={downloadingId === item.id}
                  onClick={() => handleDownload(item)}
                  className="inline-flex cursor-pointer items-center gap-1.5 rounded-lg border border-neutral-200 bg-white px-3 py-1.5 text-xs font-semibold text-neutral-700 shadow-2xs hover:bg-neutral-50 disabled:opacity-50 dark:border-zinc-700 dark:bg-zinc-800 dark:text-zinc-200 dark:hover:bg-zinc-700"
                >
                  {downloadingId === item.id ? (
                    <Loader2 className="size-3.5 animate-spin" />
                  ) : (
                    <Download className="size-3.5" />
                  )}
                  <span>{t("sharing.download")}</span>
                </button>
              ) : item.externalUrl ? (
                <a
                  href={item.externalUrl}
                  target="_blank"
                  rel="noopener noreferrer"
                  className="inline-flex items-center gap-1.5 rounded-lg border border-neutral-200 bg-white px-3 py-1.5 text-xs font-semibold text-neutral-700 shadow-2xs hover:bg-neutral-50 dark:border-zinc-700 dark:bg-zinc-800 dark:text-zinc-200 dark:hover:bg-zinc-700"
                >
                  <ExternalLink className="size-3.5" />
                  <span>{t("roadmaps.openLink")}</span>
                </a>
              ) : null}
            </div>
          </div>
        );
      })}
    </div>
  );
}
