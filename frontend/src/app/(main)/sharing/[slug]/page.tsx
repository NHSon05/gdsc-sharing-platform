"use client";

import React from "react";
import { useParams, useRouter } from "next/navigation";
import Link from "next/link";
import { format } from "date-fns";
import {
  useContentBySlugQuery,
  MarkdownViewer,
  ResourceList,
  StatusBadge,
} from "@/features/sharing";
import { useTranslation } from "@/core/i18n/i18n.context";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { UserAvatar } from "@/components/ui/user-avatar";
import {
  ArrowLeft,
  Calendar,
  BookOpen,
  FileText,
  Loader2,
  CalendarDays,
} from "lucide-react";

export default function ContentDetailPage() {
  const { t } = useTranslation();
  const router = useRouter();
  const params = useParams();
  const slug = (params?.slug as string) || "";

  const { data: detail, isLoading, isError } = useContentBySlugQuery(slug);

  if (isLoading) {
    return (
      <div className="flex min-h-[70vh] items-center justify-center">
        <Loader2 className="size-8 animate-spin text-neutral-400" />
      </div>
    );
  }

  if (isError || !detail) {
    return (
      <div className="mx-auto max-w-4xl px-4 py-16 text-center">
        <div className="mx-auto flex size-14 items-center justify-center rounded-2xl bg-rose-50 text-rose-500 dark:bg-rose-950/40 dark:text-rose-400">
          <BookOpen className="size-7" />
        </div>
        <h2 className="mt-4 text-xl font-bold text-neutral-900 dark:text-white">
          Content Not Found
        </h2>
        <p className="mt-1 text-sm text-neutral-500 dark:text-zinc-400">
          The requested article could not be found or you do not have permission
          to view it.
        </p>
        <Button
          variant="outline"
          className="mt-6"
          onClick={() => router.push("/sharing")}
        >
          <ArrowLeft className="mr-2 size-4" />
          <span>Back to Sharing</span>
        </Button>
      </div>
    );
  }

  const { content, bodyMarkdown, authors, tags, resources, schedules } = detail;
  const primaryAuthor = authors.find((a) => a.role === "Owner") || authors[0];

  return (
    <article className="mx-auto max-w-4xl space-y-8 px-4 py-8 sm:px-6 lg:px-8">
      {/* Top Back Navigation */}
      <div>
        <Link
          href="/sharing"
          className="inline-flex items-center gap-1.5 text-xs font-semibold text-neutral-500 transition-colors hover:text-neutral-900 dark:text-zinc-400 dark:hover:text-white"
        >
          <ArrowLeft className="size-3.5" />
          <span>Back to all content</span>
        </Link>
      </div>

      {/* Header Info */}
      <header className="space-y-4">
        {/* Tags and Status */}
        <div className="flex flex-wrap items-center gap-2">
          {tags.map((tag) => (
            <Badge
              key={tag.id}
              variant="secondary"
              className="bg-neutral-100 text-xs font-medium text-neutral-700 dark:bg-zinc-800 dark:text-zinc-300"
              style={
                tag.color
                  ? { borderLeftColor: tag.color, borderLeftWidth: "3px" }
                  : undefined
              }
            >
              {tag.name}
            </Badge>
          ))}
          {content.status !== "Published" && (
            <StatusBadge status={content.status} />
          )}
        </div>

        {/* Title */}
        <h1 className="text-3xl font-extrabold tracking-tight text-neutral-900 sm:text-4xl dark:text-white">
          {content.title}
        </h1>

        {/* Summary */}
        <p className="text-base leading-relaxed text-neutral-600 dark:text-zinc-300">
          {content.summary}
        </p>

        {/* Authors and Metadata */}
        <div className="flex flex-wrap items-center justify-between gap-4 border-y border-neutral-100 py-4 dark:border-zinc-800">
          <div className="flex flex-wrap items-center gap-4">
            {/* Primary Author */}
            {primaryAuthor && (
              <div className="flex items-center gap-2.5">
                <UserAvatar
                  name={primaryAuthor.fullName}
                  size="sm"
                  isAdmin={
                    primaryAuthor.fullName === "System Administrator" ||
                    primaryAuthor.fullName.toLowerCase().includes("admin")
                  }
                  showAdminBadge={
                    primaryAuthor.fullName === "System Administrator" ||
                    primaryAuthor.fullName.toLowerCase().includes("admin")
                  }
                />
                <div>
                  <p className="text-xs font-bold text-neutral-900 dark:text-white">
                    {primaryAuthor.fullName}
                  </p>
                  <p className="text-[11px] text-neutral-400 dark:text-zinc-500">
                    {primaryAuthor.fullName === "System Administrator" ||
                    primaryAuthor.fullName.toLowerCase().includes("admin")
                      ? "Admin"
                      : primaryAuthor.role}
                  </p>
                </div>
              </div>
            )}

            {/* Other Authors / Contributors */}
            {authors.length > 1 && (
              <div className="text-xs text-neutral-500 dark:text-zinc-400">
                + {authors.length - 1} contributor(s)
              </div>
            )}
          </div>

          {/* Date */}
          {content.publishedAtUtc && (
            <div className="flex items-center gap-1.5 text-xs text-neutral-500 dark:text-zinc-400">
              <Calendar className="size-4 text-neutral-400" />
              <span>
                Published on{" "}
                {format(new Date(content.publishedAtUtc), "MMMM d, yyyy")}
              </span>
            </div>
          )}
        </div>
      </header>

      {/* Cover Image */}
      {content.coverImageUrl && (
        <div className="relative aspect-16/9 w-full overflow-hidden rounded-3xl border border-neutral-200 bg-neutral-100 dark:border-zinc-800 dark:bg-zinc-800">
          {/* eslint-disable-next-line @next/next/no-img-element */}
          <img
            src={content.coverImageUrl}
            alt={content.title}
            className="size-full object-cover"
          />
        </div>
      )}

      {/* Markdown Content Reader */}
      <div className="rounded-3xl border border-neutral-200 bg-white p-6 shadow-2xs sm:p-10 dark:border-zinc-800 dark:bg-zinc-900/60">
        <MarkdownViewer content={bodyMarkdown} />
      </div>

      {/* Attachments and Resources Section */}
      {resources && resources.length > 0 && (
        <section className="space-y-4 rounded-3xl border border-neutral-200 bg-white p-6 shadow-2xs sm:p-8 dark:border-zinc-800 dark:bg-zinc-900/60">
          <div className="flex items-center gap-2">
            <FileText className="size-5 text-neutral-500 dark:text-zinc-400" />
            <h3 className="text-lg font-bold text-neutral-900 dark:text-white">
              {t("sharing.resources")}
            </h3>
          </div>
          <ResourceList resources={resources} />
        </section>
      )}

      {/* Related Sharing Schedules Section */}
      {schedules && schedules.length > 0 && (
        <section className="space-y-4 rounded-3xl border border-neutral-200 bg-white p-6 shadow-2xs sm:p-8 dark:border-zinc-800 dark:bg-zinc-900/60">
          <div className="flex items-center gap-2">
            <CalendarDays className="size-5 text-blue-500" />
            <h3 className="text-lg font-bold text-neutral-900 dark:text-white">
              Related Sharing Sessions
            </h3>
          </div>
          <div className="space-y-2">
            {schedules.map((s) => (
              <Link
                key={s.id}
                href={`/schedule?id=${s.id}`}
                className="flex items-center justify-between rounded-xl border border-neutral-100 bg-neutral-50/60 p-3 text-sm font-semibold text-neutral-800 transition-colors hover:border-neutral-300 dark:border-zinc-800 dark:bg-zinc-900 dark:text-zinc-200"
              >
                <span>{s.title}</span>
                <span className="text-xs text-neutral-400">
                  {format(new Date(s.startsAtUtc), "MMM d, yyyy")}
                </span>
              </Link>
            ))}
          </div>
        </section>
      )}
    </article>
  );
}
