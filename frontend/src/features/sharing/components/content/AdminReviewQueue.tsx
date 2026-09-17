"use client";

import React, { useState } from "react";
import Link from "next/link";
import { useTranslation } from "@/core/i18n/i18n.context";
import { useAdminContentsQuery } from "../../hooks/use-contents-query";
import { useAdminContentByIdQuery } from "../../hooks/use-content-detail-query";
import {
  useApproveContentMutation,
  useRejectContentMutation,
  useReturnToDraftMutation,
  useArchiveContentMutation,
} from "../../hooks/use-content-mutations";
import { MarkdownViewer } from "../common/MarkdownViewer";
import { ResourceList } from "../common/ResourceList";
import { StatusBadge } from "../common/StatusBadge";
import { UserAvatar } from "@/components/ui/user-avatar";
import type {
  ContentSummary,
  SharingContentStatus,
} from "../../types/sharing.types";
import {
  CheckCircle2,
  XCircle,
  Eye,
  AlertCircle,
  Loader2,
  Search,
  RotateCcw,
  Archive,
  ExternalLink,
  ChevronLeft,
  ChevronRight,
  FileText,
  Clock,
  Inbox,
} from "lucide-react";
import { Button } from "@/components/ui/button";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogFooter,
} from "@/components/ui/dialog";

type StatusFilterType = "ALL" | SharingContentStatus;

interface AdminReviewQueueProps {
  initialStatus?: StatusFilterType;
}

export function AdminReviewQueue({
  initialStatus = "PendingReview",
}: AdminReviewQueueProps) {
  const { t } = useTranslation();

  // Filters and Pagination
  const [activeStatus, setActiveStatus] =
    useState<StatusFilterType>(initialStatus);
  const [searchTerm, setSearchTerm] = useState("");
  const [sortOrder, setSortOrder] = useState<"newest" | "oldest" | "title">(
    "newest"
  );
  const [page, setPage] = useState(1);
  const pageSize = 15;

  // Query for active status view
  const {
    data: contentPage,
    isLoading,
    refetch,
  } = useAdminContentsQuery({
    status: activeStatus === "ALL" ? undefined : activeStatus,
    search: searchTerm.trim() || undefined,
    sort: activeStatus === "PendingReview" ? "oldest" : sortOrder,
    page,
    pageSize,
  });

  // Queries for quick metric counters
  const { data: allMetric } = useAdminContentsQuery({ pageSize: 1 });
  const { data: pendingMetric } = useAdminContentsQuery({
    status: "PendingReview",
    pageSize: 1,
  });
  const { data: publishedMetric } = useAdminContentsQuery({
    status: "Published",
    pageSize: 1,
  });
  const { data: rejectedMetric } = useAdminContentsQuery({
    status: "Rejected",
    pageSize: 1,
  });

  // Mutations
  const approveMutation = useApproveContentMutation();
  const rejectMutation = useRejectContentMutation();
  const returnToDraftMutation = useReturnToDraftMutation();
  const archiveMutation = useArchiveContentMutation();

  // Preview Dialog State
  const [selectedContentId, setSelectedContentId] = useState<string | null>(
    null
  );

  // Reject Modal State
  const [rejectDialogState, setRejectDialogState] = useState<{
    isOpen: boolean;
    item: ContentSummary | null;
    reason: string;
  }>({ isOpen: false, item: null, reason: "" });

  const [feedbackError, setFeedbackError] = useState<string | null>(null);

  // Full detail of selected content in modal
  const { data: detailData, isLoading: isLoadingDetail } =
    useAdminContentByIdQuery(
      selectedContentId || "",
      Boolean(selectedContentId)
    );

  const items = contentPage?.items || [];
  const totalCount = contentPage?.totalCount || 0;
  const totalPages = Math.max(1, Math.ceil(totalCount / pageSize));

  // Handlers
  const handleApprove = async (item: ContentSummary) => {
    setFeedbackError(null);
    try {
      await approveMutation.mutateAsync({
        id: item.id,
        version: item.version,
      });
      if (selectedContentId === item.id) {
        setSelectedContentId(null);
      }
    } catch (err: unknown) {
      const apiErr = err as { status?: number; message?: string } | undefined;
      if (apiErr?.status === 412) {
        setFeedbackError(t("sharing.concurrencyError"));
      } else {
        setFeedbackError(apiErr?.message || "Không thể phê duyệt bài viết.");
      }
    }
  };

  const handleConfirmReject = async () => {
    const { item, reason } = rejectDialogState;
    if (!item || !reason.trim()) return;

    setFeedbackError(null);
    try {
      await rejectMutation.mutateAsync({
        id: item.id,
        request: { reviewNote: reason.trim() },
        version: item.version,
      });
      setRejectDialogState({ isOpen: false, item: null, reason: "" });
      if (selectedContentId === item.id) {
        setSelectedContentId(null);
      }
    } catch (err: unknown) {
      const apiErr = err as { status?: number; message?: string } | undefined;
      if (apiErr?.status === 412) {
        setFeedbackError(t("sharing.concurrencyError"));
      } else {
        setFeedbackError(apiErr?.message || "Không thể từ chối bài viết.");
      }
    }
  };

  const handleReturnToDraft = async (item: ContentSummary) => {
    if (
      !window.confirm(
        "Chuyển bài viết này về Bản nháp để tác giả có thể sửa đổi?"
      )
    ) {
      return;
    }
    setFeedbackError(null);
    try {
      await returnToDraftMutation.mutateAsync({
        id: item.id,
        version: item.version,
      });
      if (selectedContentId === item.id) {
        setSelectedContentId(null);
      }
    } catch (err: unknown) {
      const apiErr = err as { status?: number; message?: string } | undefined;
      setFeedbackError(
        apiErr?.message || "Không thể chuyển bài viết về bản nháp."
      );
    }
  };

  const handleArchive = async (item: ContentSummary) => {
    if (
      !window.confirm(
        "Lưu trữ bài viết này? Bài viết sẽ không còn hiển thị công khai trên Bảng tin."
      )
    ) {
      return;
    }
    setFeedbackError(null);
    try {
      await archiveMutation.mutateAsync({
        id: item.id,
        version: item.version,
      });
      if (selectedContentId === item.id) {
        setSelectedContentId(null);
      }
    } catch (err: unknown) {
      const apiErr = err as { status?: number; message?: string } | undefined;
      setFeedbackError(apiErr?.message || "Không thể lưu trữ bài viết.");
    }
  };

  const tabs: { id: StatusFilterType; label: string; count?: number }[] = [
    { id: "ALL", label: "Tất cả", count: allMetric?.totalCount },
    {
      id: "PendingReview",
      label: "Chờ phê duyệt",
      count: pendingMetric?.totalCount,
    },
    {
      id: "Published",
      label: "Đã xuất bản",
      count: publishedMetric?.totalCount,
    },
    { id: "Rejected", label: "Bị từ chối", count: rejectedMetric?.totalCount },
    { id: "Draft", label: "Bản nháp" },
    { id: "Archived", label: "Đã lưu trữ" },
  ];

  return (
    <div className="space-y-6">
      {/* 1. Quick Stats Metric Cards */}
      <div className="grid grid-cols-2 gap-4 sm:grid-cols-4">
        <div className="rounded-2xl border border-neutral-200/80 bg-white p-4 shadow-2xs dark:border-zinc-800/80 dark:bg-zinc-900/60">
          <div className="flex items-center justify-between text-xs font-semibold text-neutral-500 dark:text-zinc-400">
            <span>Tổng bài viết</span>
            <FileText className="size-4 text-blue-500" />
          </div>
          <div className="mt-2 text-2xl font-black text-neutral-900 dark:text-white">
            {allMetric?.totalCount ?? 0}
          </div>
        </div>

        <div
          onClick={() => {
            setActiveStatus("PendingReview");
            setPage(1);
          }}
          className={`cursor-pointer rounded-2xl border p-4 shadow-2xs transition-all ${
            activeStatus === "PendingReview"
              ? "border-amber-400 bg-amber-50/70 dark:border-amber-700 dark:bg-amber-950/30"
              : "border-neutral-200/80 bg-white hover:border-amber-300 dark:border-zinc-800/80 dark:bg-zinc-900/60"
          }`}
        >
          <div className="flex items-center justify-between text-xs font-semibold text-amber-700 dark:text-amber-400">
            <span>Chờ phê duyệt</span>
            <Clock className="size-4" />
          </div>
          <div className="mt-2 text-2xl font-black text-amber-800 dark:text-amber-300">
            {pendingMetric?.totalCount ?? 0}
          </div>
        </div>

        <div
          onClick={() => {
            setActiveStatus("Published");
            setPage(1);
          }}
          className={`cursor-pointer rounded-2xl border p-4 shadow-2xs transition-all ${
            activeStatus === "Published"
              ? "border-emerald-400 bg-emerald-50/70 dark:border-emerald-700 dark:bg-emerald-950/30"
              : "border-neutral-200/80 bg-white hover:border-emerald-300 dark:border-zinc-800/80 dark:bg-zinc-900/60"
          }`}
        >
          <div className="flex items-center justify-between text-xs font-semibold text-emerald-700 dark:text-emerald-400">
            <span>Đã xuất bản</span>
            <CheckCircle2 className="size-4" />
          </div>
          <div className="mt-2 text-2xl font-black text-emerald-800 dark:text-emerald-300">
            {publishedMetric?.totalCount ?? 0}
          </div>
        </div>

        <div
          onClick={() => {
            setActiveStatus("Rejected");
            setPage(1);
          }}
          className={`cursor-pointer rounded-2xl border p-4 shadow-2xs transition-all ${
            activeStatus === "Rejected"
              ? "border-rose-400 bg-rose-50/70 dark:border-rose-700 dark:bg-rose-950/30"
              : "border-neutral-200/80 bg-white hover:border-rose-300 dark:border-zinc-800/80 dark:bg-zinc-900/60"
          }`}
        >
          <div className="flex items-center justify-between text-xs font-semibold text-rose-700 dark:text-rose-400">
            <span>Bị từ chối</span>
            <XCircle className="size-4" />
          </div>
          <div className="mt-2 text-2xl font-black text-rose-800 dark:text-rose-300">
            {rejectedMetric?.totalCount ?? 0}
          </div>
        </div>
      </div>

      {/* 2. Feedback Error Banner */}
      {feedbackError && (
        <div className="flex items-center justify-between rounded-xl border border-rose-200 bg-rose-50 p-3 text-xs font-semibold text-rose-700 dark:border-rose-900/60 dark:bg-rose-950/40 dark:text-rose-300">
          <div className="flex items-center gap-2">
            <AlertCircle className="size-4 shrink-0" />
            <span>{feedbackError}</span>
          </div>
          <button
            type="button"
            onClick={() => refetch()}
            className="cursor-pointer underline underline-offset-2 hover:text-rose-900"
          >
            {t("sharing.refresh")}
          </button>
        </div>
      )}

      {/* 3. Status Tabs & Filters Bar */}
      <div className="space-y-4">
        {/* Status Tabs */}
        <div className="flex flex-wrap items-center gap-1.5 border-b border-neutral-200/80 pb-3 dark:border-zinc-800/80">
          {tabs.map((tab) => {
            const isActive = activeStatus === tab.id;
            return (
              <button
                key={tab.id}
                type="button"
                onClick={() => {
                  setActiveStatus(tab.id);
                  setPage(1);
                }}
                className={`flex cursor-pointer items-center gap-2 rounded-xl px-3.5 py-2 text-xs font-semibold transition-all ${
                  isActive
                    ? "bg-brand text-white shadow-xs"
                    : "text-neutral-600 hover:bg-neutral-100 dark:text-zinc-400 dark:hover:bg-zinc-800/60 dark:hover:text-white"
                }`}
              >
                <span>{tab.label}</span>
                {typeof tab.count === "number" && (
                  <span
                    className={`py-0.2 rounded-full px-1.5 text-[10px] font-bold ${
                      isActive
                        ? "bg-white/20 text-white"
                        : "bg-neutral-200/80 text-neutral-700 dark:bg-zinc-800 dark:text-zinc-300"
                    }`}
                  >
                    {tab.count}
                  </span>
                )}
              </button>
            );
          })}
        </div>

        {/* Search & Sort Controls */}
        <div className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
          <div className="relative max-w-sm flex-1">
            <Search className="absolute top-1/2 left-3 size-4 -translate-y-1/2 text-neutral-400 dark:text-zinc-500" />
            <input
              type="text"
              value={searchTerm}
              onChange={(e) => {
                setSearchTerm(e.target.value);
                setPage(1);
              }}
              placeholder="Tìm theo tiêu đề hoặc tóm tắt..."
              className="focus:border-brand w-full rounded-xl border border-neutral-200/90 bg-white py-2 pr-4 pl-9 text-xs text-neutral-900 placeholder:text-neutral-400 focus:outline-hidden dark:border-zinc-800 dark:bg-zinc-900/90 dark:text-zinc-100"
            />
          </div>

          <div className="flex items-center gap-2">
            <select
              value={sortOrder}
              onChange={(e) => {
                setSortOrder(e.target.value as "newest" | "oldest" | "title");
                setPage(1);
              }}
              className="rounded-xl border border-neutral-200/90 bg-white px-3 py-2 text-xs font-medium text-neutral-700 dark:border-zinc-800 dark:bg-zinc-900 dark:text-zinc-300"
            >
              <option value="newest">Mới nhất trước</option>
              <option value="oldest">Cũ nhất trước</option>
              <option value="title">Theo bảng chữ cái (A-Z)</option>
            </select>
          </div>
        </div>
      </div>

      {/* 4. Main Contents Table / List */}
      <div className="overflow-hidden rounded-3xl border border-neutral-200/80 bg-white shadow-xs dark:border-zinc-800/80 dark:bg-[#0C0C0E]">
        <div className="overflow-x-auto">
          <table className="w-full text-left text-xs">
            <thead className="border-b border-neutral-100 bg-neutral-50/70 text-[11px] font-semibold text-neutral-500 uppercase dark:border-zinc-800/80 dark:bg-zinc-900/40 dark:text-zinc-400">
              <tr>
                <th className="px-6 py-3.5">Bài viết</th>
                <th className="px-6 py-3.5">Tác giả</th>
                <th className="px-6 py-3.5">Thẻ</th>
                <th className="px-6 py-3.5">Trạng thái</th>
                <th className="px-6 py-3.5">Thời gian</th>
                <th className="px-6 py-3.5 text-right">Thao tác</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-neutral-100 font-medium dark:divide-zinc-800/80">
              {isLoading ? (
                <tr>
                  <td colSpan={6} className="py-20 text-center">
                    <Loader2 className="text-brand mx-auto size-8 animate-spin" />
                    <span className="mt-2 block text-xs text-neutral-500">
                      Đang tải danh sách bài viết...
                    </span>
                  </td>
                </tr>
              ) : items.length === 0 ? (
                <tr>
                  <td colSpan={6} className="py-20 text-center">
                    <Inbox className="mx-auto size-12 text-neutral-300 dark:text-zinc-600" />
                    <h3 className="mt-3 text-base font-bold text-neutral-900 dark:text-white">
                      Không có bài viết nào
                    </h3>
                    <p className="mt-1 text-xs text-neutral-500 dark:text-zinc-500">
                      {activeStatus === "PendingReview"
                        ? t("sharing.noPendingReviews")
                        : "Không tìm thấy bài viết nào trong danh mục hoặc từ khóa này."}
                    </p>
                  </td>
                </tr>
              ) : (
                items.map((item) => {
                  const author = item.authors[0]?.fullName || "Unknown Author";

                  return (
                    <tr
                      key={item.id}
                      className="transition-colors hover:bg-neutral-50/50 dark:hover:bg-zinc-900/30"
                    >
                      {/* Title & summary column */}
                      <td className="max-w-xs px-6 py-4 sm:max-w-sm">
                        <div className="flex items-start gap-3">
                          {item.coverImageUrl ? (
                            <img
                              src={item.coverImageUrl}
                              alt={item.title}
                              className="size-10 shrink-0 rounded-xl object-cover shadow-2xs"
                            />
                          ) : (
                            <div className="flex size-10 shrink-0 items-center justify-center rounded-xl bg-blue-50 text-blue-600 dark:bg-blue-950/50 dark:text-blue-400">
                              <FileText className="size-5" />
                            </div>
                          )}
                          <div className="min-w-0 flex-1 space-y-0.5">
                            <span className="line-clamp-1 font-bold text-neutral-900 dark:text-white">
                              {item.title}
                            </span>
                            <p className="line-clamp-1 text-[11px] text-neutral-500 dark:text-zinc-400">
                              {item.summary}
                            </p>
                            <div className="flex items-center gap-1.5 text-[10px] text-neutral-400 dark:text-zinc-500">
                              <span>Slug: {item.slug}</span>
                              {item.status === "Published" && (
                                <Link
                                  href={`/sharing/${item.slug}`}
                                  target="_blank"
                                  className="text-brand inline-flex items-center gap-0.5 hover:underline"
                                >
                                  <span>Xem bài</span>
                                  <ExternalLink className="size-2.5" />
                                </Link>
                              )}
                            </div>
                          </div>
                        </div>
                      </td>

                      {/* Author column */}
                      <td className="px-6 py-4">
                        <div className="flex items-center gap-2">
                          <UserAvatar
                            name={author}
                            size="xs"
                            isAdmin={
                              author === "System Administrator" ||
                              author.toLowerCase().includes("admin")
                            }
                            showAdminBadge={
                              author === "System Administrator" ||
                              author.toLowerCase().includes("admin")
                            }
                          />
                          <span className="font-semibold text-neutral-900 dark:text-zinc-200">
                            {author}
                          </span>
                        </div>
                      </td>

                      {/* Tags column */}
                      <td className="px-6 py-4">
                        <div className="flex max-w-[180px] flex-wrap items-center gap-1">
                          {item.tags.length > 0 ? (
                            item.tags.map((tag) => (
                              <span
                                key={tag.id}
                                className="rounded-md border border-neutral-200 bg-neutral-50 px-1.5 py-0.5 text-[10px] font-medium text-neutral-600 dark:border-zinc-800 dark:bg-zinc-900 dark:text-zinc-400"
                              >
                                #{tag.name}
                              </span>
                            ))
                          ) : (
                            <span className="text-[11px] text-neutral-400 italic">
                              Không có thẻ
                            </span>
                          )}
                        </div>
                      </td>

                      {/* Status column */}
                      <td className="px-6 py-4">
                        <StatusBadge status={item.status} />
                      </td>

                      {/* Date column */}
                      <td className="px-6 py-4 text-[11px] text-neutral-500 dark:text-zinc-400">
                        {item.publishedAtUtc ? (
                          <span>
                            {new Date(item.publishedAtUtc).toLocaleDateString(
                              "vi-VN"
                            )}
                          </span>
                        ) : (
                          <span className="italic">Chưa xuất bản</span>
                        )}
                      </td>

                      {/* Actions column */}
                      <td className="px-6 py-4 text-right">
                        <div className="flex items-center justify-end gap-1.5">
                          {/* Preview Button */}
                          <Button
                            variant="outline"
                            size="sm"
                            onClick={() => setSelectedContentId(item.id)}
                            className="h-8 text-xs"
                          >
                            <Eye className="mr-1 size-3.5" />
                            <span>Chi tiết</span>
                          </Button>

                          {/* Quick Actions for PendingReview */}
                          {item.status === "PendingReview" && (
                            <>
                              <Button
                                variant="destructive"
                                size="sm"
                                onClick={() =>
                                  setRejectDialogState({
                                    isOpen: true,
                                    item,
                                    reason: "",
                                  })
                                }
                                className="h-8 text-xs"
                              >
                                <XCircle className="mr-1 size-3.5" />
                                <span>{t("sharing.reject")}</span>
                              </Button>
                              <Button
                                size="sm"
                                disabled={approveMutation.isPending}
                                onClick={() => handleApprove(item)}
                                className="h-8 bg-emerald-600 text-xs text-white hover:bg-emerald-700 dark:bg-emerald-600 dark:hover:bg-emerald-500"
                              >
                                <CheckCircle2 className="mr-1 size-3.5" />
                                <span>{t("sharing.approve")}</span>
                              </Button>
                            </>
                          )}

                          {/* Quick Action for Published: Archive & Return to Draft */}
                          {item.status === "Published" && (
                            <>
                              <button
                                type="button"
                                onClick={() => handleReturnToDraft(item)}
                                title="Chuyển về Bản nháp"
                                className="flex size-8 cursor-pointer items-center justify-center rounded-lg border border-neutral-200 bg-white text-neutral-600 hover:bg-neutral-100 dark:border-zinc-800 dark:bg-zinc-900 dark:text-zinc-400"
                              >
                                <RotateCcw className="size-3.5" />
                              </button>
                              <button
                                type="button"
                                onClick={() => handleArchive(item)}
                                title="Lưu trữ bài viết"
                                className="flex size-8 cursor-pointer items-center justify-center rounded-lg border border-neutral-200 bg-white text-neutral-600 hover:bg-neutral-100 dark:border-zinc-800 dark:bg-zinc-900 dark:text-zinc-400"
                              >
                                <Archive className="size-3.5" />
                              </button>
                            </>
                          )}

                          {/* Quick Action for Rejected & Archived: Return to Draft */}
                          {(item.status === "Rejected" ||
                            item.status === "Archived") && (
                            <button
                              type="button"
                              onClick={() => handleReturnToDraft(item)}
                              title="Chuyển về Bản nháp"
                              className="flex size-8 cursor-pointer items-center justify-center rounded-lg border border-neutral-200 bg-white text-neutral-600 hover:bg-neutral-100 dark:border-zinc-800 dark:bg-zinc-900 dark:text-zinc-400"
                            >
                              <RotateCcw className="size-3.5" />
                            </button>
                          )}
                        </div>
                      </td>
                    </tr>
                  );
                })
              )}
            </tbody>
          </table>
        </div>

        {/* Pagination Footer */}
        {totalCount > 0 && (
          <div className="flex flex-col items-center justify-between gap-3 border-t border-neutral-100 bg-neutral-50/50 px-6 py-3.5 sm:flex-row dark:border-zinc-800/80 dark:bg-zinc-900/30">
            <span className="text-xs text-neutral-500 dark:text-zinc-400">
              Hiển thị{" "}
              <span className="font-semibold text-neutral-900 dark:text-white">
                {(page - 1) * pageSize + 1} -{" "}
                {Math.min(page * pageSize, totalCount)}
              </span>{" "}
              trên tổng số{" "}
              <span className="font-semibold text-neutral-900 dark:text-white">
                {totalCount}
              </span>{" "}
              bài viết
            </span>

            <div className="flex items-center gap-2">
              <Button
                variant="outline"
                size="sm"
                disabled={page <= 1}
                onClick={() => setPage((p) => Math.max(1, p - 1))}
                leftIcon={<ChevronLeft className="size-3.5" />}
                className="h-8 text-xs"
              >
                Trước
              </Button>
              <span className="px-2 text-xs font-semibold text-neutral-700 dark:text-zinc-300">
                Trang {page} / {totalPages}
              </span>
              <Button
                variant="outline"
                size="sm"
                disabled={page >= totalPages}
                onClick={() => setPage((p) => p + 1)}
                rightIcon={<ChevronRight className="size-3.5" />}
                className="h-8 text-xs"
              >
                Sau
              </Button>
            </div>
          </div>
        )}
      </div>

      {/* 5. Preview & Inspect Modal */}
      <Dialog
        open={Boolean(selectedContentId)}
        onOpenChange={(open) => !open && setSelectedContentId(null)}
      >
        <DialogContent className="max-h-[90vh] max-w-4xl overflow-y-auto p-6">
          <DialogHeader>
            <DialogTitle className="text-xl font-bold text-neutral-900 dark:text-white">
              Chi tiết bài viết
            </DialogTitle>
          </DialogHeader>

          {isLoadingDetail ? (
            <div className="flex min-h-60 items-center justify-center">
              <Loader2 className="size-8 animate-spin text-neutral-400" />
            </div>
          ) : detailData ? (
            <div className="space-y-6 pt-2">
              {/* Header Details */}
              <div className="space-y-2 border-b border-neutral-100 pb-4 dark:border-zinc-800">
                <div className="flex flex-wrap items-center gap-2">
                  <StatusBadge status={detailData.content.status} />
                  <h2 className="text-2xl font-bold text-neutral-900 dark:text-white">
                    {detailData.content.title}
                  </h2>
                </div>
                <p className="text-sm text-neutral-600 dark:text-zinc-300">
                  {detailData.content.summary}
                </p>
                <div className="flex flex-wrap items-center gap-4 text-xs text-neutral-400 dark:text-zinc-500">
                  <span>
                    Tác giả:{" "}
                    <strong className="text-neutral-700 dark:text-zinc-300">
                      {detailData.authors[0]?.fullName || "Unknown"}
                    </strong>
                  </span>
                  <span>Slug: {detailData.content.slug}</span>
                  {detailData.content.publishedAtUtc && (
                    <span>
                      Xuất bản lúc:{" "}
                      {new Date(
                        detailData.content.publishedAtUtc
                      ).toLocaleString("vi-VN")}
                    </span>
                  )}
                </div>
              </div>

              {/* Reject Note Banner if Rejected */}
              {detailData.content.status === "Rejected" &&
                detailData.reviewNote && (
                  <div className="rounded-xl border border-rose-200 bg-rose-50 p-4 text-xs dark:border-rose-900/60 dark:bg-rose-950/30">
                    <h5 className="font-bold text-rose-800 dark:text-rose-300">
                      Lý do từ chối từ Quản trị viên:
                    </h5>
                    <p className="mt-1 text-rose-700 dark:text-rose-400">
                      {detailData.reviewNote}
                    </p>
                  </div>
                )}

              {/* Cover Image */}
              {detailData.content.coverImageUrl && (
                <div className="overflow-hidden rounded-2xl border border-neutral-200 dark:border-zinc-800">
                  <img
                    src={detailData.content.coverImageUrl}
                    alt={detailData.content.title}
                    className="max-h-72 w-full object-cover"
                  />
                </div>
              )}

              {/* Body Content Markdown */}
              <div>
                <h4 className="mb-2 text-xs font-bold tracking-wider text-neutral-400 uppercase dark:text-zinc-500">
                  Nội dung bài viết
                </h4>
                <div className="rounded-xl border border-neutral-200 bg-neutral-50/40 p-5 dark:border-zinc-800 dark:bg-zinc-900/40">
                  <MarkdownViewer content={detailData.bodyMarkdown} />
                </div>
              </div>

              {/* Attached Resources */}
              {detailData.resources && detailData.resources.length > 0 && (
                <div>
                  <h4 className="mb-2 text-xs font-bold tracking-wider text-neutral-400 uppercase dark:text-zinc-500">
                    Tài liệu & Tệp đính kèm ({detailData.resources.length})
                  </h4>
                  <ResourceList resources={detailData.resources} />
                </div>
              )}

              {/* Modal Footer Actions */}
              <DialogFooter className="flex flex-wrap items-center justify-between gap-2 border-t border-neutral-100 pt-4 dark:border-zinc-800">
                <div>
                  {detailData.content.status === "Published" && (
                    <Link
                      href={`/sharing/${detailData.content.slug}`}
                      target="_blank"
                      className="text-brand inline-flex items-center gap-1 text-xs font-semibold hover:underline"
                    >
                      <span>Mở trang bài viết công khai</span>
                      <ExternalLink className="size-3" />
                    </Link>
                  )}
                </div>

                <div className="flex items-center gap-2">
                  <Button
                    variant="outline"
                    onClick={() => setSelectedContentId(null)}
                  >
                    Đóng
                  </Button>

                  {/* Actions for PendingReview */}
                  {detailData.content.status === "PendingReview" && (
                    <>
                      <Button
                        variant="destructive"
                        onClick={() => {
                          setRejectDialogState({
                            isOpen: true,
                            item: detailData.content,
                            reason: "",
                          });
                        }}
                      >
                        <XCircle className="mr-1.5 size-3.5" />
                        <span>{t("sharing.reject")}</span>
                      </Button>
                      <Button
                        onClick={() => handleApprove(detailData.content)}
                        className="bg-emerald-600 text-white hover:bg-emerald-700 dark:bg-emerald-600 dark:hover:bg-emerald-500"
                      >
                        <CheckCircle2 className="mr-1.5 size-3.5" />
                        <span>{t("sharing.approve")}</span>
                      </Button>
                    </>
                  )}

                  {/* Actions for Published */}
                  {detailData.content.status === "Published" && (
                    <>
                      <Button
                        variant="outline"
                        onClick={() => handleReturnToDraft(detailData.content)}
                      >
                        <RotateCcw className="mr-1.5 size-3.5" />
                        <span>Trả về Bản nháp</span>
                      </Button>
                      <Button
                        variant="subtle"
                        onClick={() => handleArchive(detailData.content)}
                      >
                        <Archive className="mr-1.5 size-3.5" />
                        <span>Lưu trữ</span>
                      </Button>
                    </>
                  )}

                  {/* Actions for Rejected / Archived */}
                  {(detailData.content.status === "Rejected" ||
                    detailData.content.status === "Archived") && (
                    <Button
                      variant="outline"
                      onClick={() => handleReturnToDraft(detailData.content)}
                    >
                      <RotateCcw className="mr-1.5 size-3.5" />
                      <span>Trả về Bản nháp</span>
                    </Button>
                  )}
                </div>
              </DialogFooter>
            </div>
          ) : null}
        </DialogContent>
      </Dialog>

      {/* 6. Mandatory Reject Reason Dialog */}
      <Dialog
        open={rejectDialogState.isOpen}
        onOpenChange={(open) =>
          !open &&
          setRejectDialogState({ isOpen: false, item: null, reason: "" })
        }
      >
        <DialogContent className="max-w-md p-6">
          <DialogHeader>
            <DialogTitle className="flex items-center gap-2 text-base font-bold text-neutral-900 dark:text-white">
              <XCircle className="size-4 text-rose-500" />
              <span>Từ chối phê duyệt bài viết</span>
            </DialogTitle>
          </DialogHeader>
          <div className="space-y-3 pt-2">
            <p className="text-xs text-neutral-500 dark:text-zinc-400">
              Vui lòng nhập lý do từ chối hoặc góp ý chỉnh sửa. Tác giả sẽ đọc
              được nội dung này để hoàn thiện bài viết.
            </p>
            <textarea
              value={rejectDialogState.reason}
              onChange={(e) =>
                setRejectDialogState((prev) => ({
                  ...prev,
                  reason: e.target.value,
                }))
              }
              placeholder="Nhập lý do từ chối hoặc yêu cầu sửa đổi..."
              rows={4}
              className="w-full rounded-xl border border-neutral-200 bg-white p-3 text-xs text-neutral-900 placeholder:text-neutral-400 focus:border-neutral-400 focus:outline-hidden dark:border-zinc-800 dark:bg-zinc-900 dark:text-white dark:placeholder:text-zinc-500"
              required
            />
          </div>
          <DialogFooter className="mt-4 flex justify-end gap-2">
            <Button
              variant="outline"
              onClick={() =>
                setRejectDialogState({
                  isOpen: false,
                  item: null,
                  reason: "",
                })
              }
            >
              Hủy
            </Button>
            <Button
              variant="destructive"
              disabled={
                !rejectDialogState.reason.trim() || rejectMutation.isPending
              }
              onClick={handleConfirmReject}
            >
              {rejectMutation.isPending && (
                <Loader2 className="mr-1.5 size-3.5 animate-spin" />
              )}
              Xác nhận từ chối
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}
