"use client";

import React, { useState } from "react";
import Link from "next/link";
import { formatDistanceToNow, parseISO } from "date-fns";
import { vi, enUS } from "date-fns/locale";
import { Card } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { StatusBadge } from "../common/StatusBadge";
import { useTranslation } from "@/core/i18n/i18n.context";
import type { ContentSummary } from "../../types/sharing.types";
import {
  Clock,
  Share2,
  Check,
  ArrowRight,
  Sparkles,
  BookOpen,
} from "lucide-react";

interface FeedPostCardProps {
  content: ContentSummary;
  className?: string;
}

export function FeedPostCard({ content, className }: FeedPostCardProps) {
  const { t, locale } = useTranslation();
  const [copied, setCopied] = useState(false);

  const primaryAuthor =
    content.authors.find((a) => a.role === "Owner") ?? content.authors[0];

  const authorInitials = primaryAuthor?.fullName
    ? primaryAuthor.fullName
        .split(" ")
        .map((n) => n[0])
        .join("")
        .slice(0, 2)
        .toUpperCase()
    : "GD";

  let timeAgo = "";
  if (content.publishedAtUtc) {
    try {
      timeAgo = formatDistanceToNow(parseISO(content.publishedAtUtc), {
        addSuffix: true,
        locale: locale === "vi" ? vi : enUS,
      });
    } catch {
      timeAgo = "";
    }
  }

  const handleCopyLink = async (e: React.MouseEvent) => {
    e.preventDefault();
    e.stopPropagation();
    try {
      const url = `${window.location.origin}/sharing/${content.slug}`;
      await navigator.clipboard.writeText(url);
      setCopied(true);
      setTimeout(() => setCopied(false), 2000);
    } catch {
      // ignore
    }
  };

  // Estimate reading time from summary
  const words = content.summary.split(/\s+/).length + 200;
  const readingTime = Math.max(1, Math.ceil(words / 150));

  return (
    <Card
      variant="default"
      className={`group hover:border-brand/40 dark:hover:border-brand/40 relative overflow-hidden rounded-2xl border border-neutral-200/80 bg-white p-6 shadow-2xs transition-all duration-300 hover:shadow-lg dark:border-zinc-800 dark:bg-zinc-900/90 ${className ?? ""}`}
    >
      {/* Top Author & Meta Row */}
      <div className="flex items-center justify-between gap-4 pb-4">
        <div className="flex items-center gap-3">
          <div className="bg-brand/10 text-brand ring-brand/20 flex size-11 items-center justify-center rounded-full text-sm font-bold ring-2">
            {authorInitials}
          </div>

          <div className="min-w-0">
            <div className="flex items-center gap-2">
              <span className="truncate text-sm font-bold text-neutral-900 dark:text-zinc-100">
                {primaryAuthor?.fullName || "GDSC Contributor"}
              </span>
              <span className="border-brand-border/40 bg-brand-muted text-brand inline-flex items-center gap-1 rounded-md border px-2 py-0.5 text-[10px] font-semibold">
                <Sparkles className="size-2.5" />
                {primaryAuthor?.role || "Member"}
              </span>
            </div>
            <div className="flex items-center gap-2 text-xs text-neutral-500 dark:text-zinc-400">
              {timeAgo && <span>{timeAgo}</span>}
              <span>•</span>
              <span className="flex items-center gap-1">
                <Clock className="size-3 text-neutral-400" />
                {readingTime} {t("sharing.readingTime")}
              </span>
            </div>
          </div>
        </div>

        <div className="flex items-center gap-2">
          {content.status !== "Published" && (
            <StatusBadge status={content.status} />
          )}

          <Button
            variant="ghost"
            size="icon"
            onClick={handleCopyLink}
            aria-label="Copy link"
            className="hover:text-brand size-8 rounded-full text-neutral-400 hover:bg-neutral-100 dark:hover:bg-zinc-800"
          >
            {copied ? (
              <Check className="size-4 text-emerald-600" />
            ) : (
              <Share2 className="size-4" />
            )}
          </Button>
        </div>
      </div>

      {/* Main Post Content */}
      <div className="space-y-3">
        {/* Title */}
        <Link href={`/sharing/${content.slug}`} className="block">
          <h3 className="group-hover:text-brand dark:group-hover:text-brand text-lg font-bold tracking-tight text-neutral-900 transition-colors sm:text-xl dark:text-zinc-100">
            {content.title}
          </h3>
        </Link>

        {/* Summary */}
        <p className="line-clamp-3 text-sm leading-relaxed text-neutral-600 dark:text-zinc-300">
          {content.summary}
        </p>

        {/* Optional Cover Image */}
        {content.coverImageUrl && (
          <Link
            href={`/sharing/${content.slug}`}
            className="block overflow-hidden rounded-xl border border-neutral-100 dark:border-zinc-800"
          >
            <div className="relative aspect-[21/9] w-full overflow-hidden bg-neutral-100 sm:aspect-[2/1] dark:bg-zinc-800">
              {/* eslint-disable-next-line @next/next/no-img-element */}
              <img
                src={content.coverImageUrl}
                alt={content.title}
                className="size-full object-cover transition-transform duration-500 group-hover:scale-103"
                loading="lazy"
              />
            </div>
          </Link>
        )}
      </div>

      {/* Tags & Action Footer */}
      <div className="mt-5 flex flex-wrap items-center justify-between gap-3 border-t border-neutral-100 pt-4 dark:border-zinc-800/80">
        {/* Tags */}
        <div className="flex flex-wrap items-center gap-1.5">
          {content.tags && content.tags.length > 0 ? (
            content.tags.map((tag) => (
              <Badge
                key={tag.id}
                variant="outline"
                className="hover:border-brand/40 hover:bg-brand-muted hover:text-brand border-neutral-200 bg-neutral-50/80 text-xs font-medium text-neutral-600 transition-colors dark:border-zinc-800 dark:bg-zinc-800/60 dark:text-zinc-300"
                style={
                  tag.color
                    ? { borderLeftColor: tag.color, borderLeftWidth: "3px" }
                    : undefined
                }
              >
                {tag.name}
              </Badge>
            ))
          ) : (
            <span className="text-[11px] text-neutral-400 dark:text-zinc-500">
              #GDSCSharing
            </span>
          )}
        </div>

        {/* Read More CTA */}
        <Link href={`/sharing/${content.slug}`}>
          <Button
            variant="brand"
            size="sm"
            className="font-semibold shadow-xs"
            rightIcon={<ArrowRight className="size-3.5" />}
          >
            <BookOpen className="size-3.5" />
            <span>{t("sharing.readMore")}</span>
          </Button>
        </Link>
      </div>
    </Card>
  );
}
