"use client";

import React from "react";
import Link from "next/link";
import { format } from "date-fns";
import type { ContentSummary } from "../../types/sharing.types";
import { StatusBadge } from "../common/StatusBadge";
import { Badge } from "@/components/ui/badge";
import { UserAvatar } from "@/components/ui/user-avatar";
import { BookOpen, Calendar } from "lucide-react";

interface ContentCardProps {
  content: ContentSummary;
  href?: string;
  showStatus?: boolean;
  className?: string;
}

export function ContentCard({
  content,
  href = `/sharing/${content.slug}`,
  showStatus = false,
  className,
}: ContentCardProps) {
  const primaryAuthor =
    content.authors.find((a) => a.role === "Owner") || content.authors[0];

  return (
    <div
      className={`group hover:border-brand/40 dark:hover:border-brand/40 flex flex-col overflow-hidden rounded-2xl border border-neutral-200 bg-white shadow-2xs transition-all duration-200 hover:-translate-y-1 hover:shadow-md dark:border-zinc-800 dark:bg-zinc-900/80 ${
        className ?? ""
      }`}
    >
      {/* Thumbnail or Fallback Header */}
      <Link
        href={href}
        className="relative aspect-16/9 w-full overflow-hidden bg-neutral-100 dark:bg-zinc-800"
      >
        {content.coverImageUrl ? (
          // eslint-disable-next-line @next/next/no-img-element
          <img
            src={content.coverImageUrl}
            alt={content.title}
            className="size-full object-cover transition-transform duration-300 group-hover:scale-105"
            loading="lazy"
          />
        ) : (
          <div className="from-brand/10 dark:from-brand/20 flex size-full items-center justify-center bg-linear-to-br via-sky-500/10 to-indigo-500/10 dark:via-sky-950/40 dark:to-indigo-950/40">
            <BookOpen className="group-hover:text-brand dark:group-hover:text-brand size-10 text-neutral-400 transition-all duration-300 group-hover:scale-110 dark:text-zinc-600" />
          </div>
        )}

        {/* Status Badge overlay if requested */}
        {showStatus && (
          <div className="absolute top-3 right-3">
            <StatusBadge status={content.status} />
          </div>
        )}
      </Link>

      {/* Card Content */}
      <div className="flex flex-1 flex-col p-5">
        {/* Tags */}
        {content.tags && content.tags.length > 0 && (
          <div className="mb-2.5 flex flex-wrap gap-1.5">
            {content.tags.slice(0, 3).map((tag) => (
              <Badge
                key={tag.id}
                variant="secondary"
                className="bg-neutral-100 text-[11px] font-medium text-neutral-700 dark:bg-zinc-800 dark:text-zinc-300"
                style={
                  tag.color
                    ? { borderLeftColor: tag.color, borderLeftWidth: "3px" }
                    : undefined
                }
              >
                {tag.name}
              </Badge>
            ))}
            {content.tags.length > 3 && (
              <span className="self-center text-[10px] text-neutral-400 dark:text-zinc-500">
                +{content.tags.length - 3}
              </span>
            )}
          </div>
        )}

        {/* Title */}
        <Link href={href} className="group-hover:text-brand transition-colors">
          <h3 className="group-hover:text-brand dark:group-hover:text-brand line-clamp-2 text-base font-bold text-neutral-900 transition-colors dark:text-white">
            {content.title}
          </h3>
        </Link>

        {/* Summary */}
        <p className="mt-2 line-clamp-2 text-xs leading-relaxed text-neutral-500 dark:text-zinc-400">
          {content.summary}
        </p>

        <div className="mt-auto pt-4">
          <div className="flex items-center justify-between border-t border-neutral-100 pt-3 text-xs text-neutral-500 dark:border-zinc-800/80 dark:text-zinc-400">
            {/* Author */}
            <div className="flex min-w-0 items-center gap-1.5">
              <UserAvatar
                name={primaryAuthor?.fullName}
                size="xs"
                isAdmin={
                  primaryAuthor?.fullName === "System Administrator" ||
                  primaryAuthor?.fullName?.toLowerCase().includes("admin")
                }
                showAdminBadge={false}
              />
              <span className="truncate font-medium text-neutral-700 dark:text-zinc-300">
                {primaryAuthor ? primaryAuthor.fullName : "Unknown"}
              </span>
            </div>

            {/* Published Date */}
            {content.publishedAtUtc && (
              <div className="flex shrink-0 items-center gap-1 text-[11px]">
                <Calendar className="size-3 text-neutral-400" />
                <span>
                  {format(new Date(content.publishedAtUtc), "MMM d, yyyy")}
                </span>
              </div>
            )}
          </div>
        </div>
      </div>
    </div>
  );
}
