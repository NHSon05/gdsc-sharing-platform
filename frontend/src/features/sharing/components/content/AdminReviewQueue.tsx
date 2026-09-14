"use client";

import React, { useState } from "react";
import { useTranslation } from "@/core/i18n/i18n.context";
import { useAdminContentsQuery } from "../../hooks/use-contents-query";
import { useAdminContentByIdQuery } from "../../hooks/use-content-detail-query";
import {
  useApproveContentMutation,
  useRejectContentMutation,
} from "../../hooks/use-content-mutations";
import { MarkdownViewer } from "../common/MarkdownViewer";
import { ResourceList } from "../common/ResourceList";
import { StatusBadge } from "../common/StatusBadge";
import type { ContentSummary } from "../../types/sharing.types";
import {
  CheckCircle2,
  XCircle,
  Eye,
  AlertCircle,
  Loader2,
  User,
} from "lucide-react";
import { Button } from "@/components/ui/button";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogFooter,
} from "@/components/ui/dialog";

export function AdminReviewQueue() {
  const { t } = useTranslation();

  // Fetch PendingReview contents
  const {
    data: queueData,
    isLoading,
    refetch,
  } = useAdminContentsQuery({
    status: "PendingReview",
    page: 1,
    pageSize: 20,
    sort: "oldest", // FIFO priority
  });

  const approveMutation = useApproveContentMutation();
  const rejectMutation = useRejectContentMutation();

  // Preview Drawer/Dialog State
  const [selectedContentId, setSelectedContentId] = useState<string | null>(
    null
  );
  const [rejectDialogState, setRejectDialogState] = useState<{
    isOpen: boolean;
    item: ContentSummary | null;
    reason: string;
  }>({ isOpen: false, item: null, reason: "" });

  const [feedbackError, setFeedbackError] = useState<string | null>(null);

  // Full detail of selected content
  const { data: detailData, isLoading: isLoadingDetail } =
    useAdminContentByIdQuery(
      selectedContentId || "",
      Boolean(selectedContentId)
    );

  const items = queueData?.items || [];

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
        setFeedbackError(apiErr?.message || "Failed to approve content.");
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
        setFeedbackError(apiErr?.message || "Failed to reject content.");
      }
    }
  };

  return (
    <div className="space-y-6">
      {/* Feedback banner */}
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

      {/* Main Review Table / List */}
      {isLoading ? (
        <div className="flex min-h-60 items-center justify-center">
          <Loader2 className="size-8 animate-spin text-neutral-400" />
        </div>
      ) : items.length === 0 ? (
        <div className="flex min-h-72 flex-col items-center justify-center rounded-2xl border border-dashed border-neutral-200 bg-neutral-50/50 p-8 text-center dark:border-zinc-800 dark:bg-zinc-900/30">
          <div className="flex size-12 items-center justify-center rounded-full bg-emerald-100 text-emerald-600 dark:bg-emerald-950/40 dark:text-emerald-400">
            <CheckCircle2 className="size-6" />
          </div>
          <h3 className="mt-3 text-base font-bold text-neutral-900 dark:text-white">
            {t("sharing.noPendingReviews")}
          </h3>
          <p className="mt-1 text-xs text-neutral-500 dark:text-zinc-400">
            All submitted content articles have been processed.
          </p>
        </div>
      ) : (
        <div className="space-y-3">
          {items.map((item) => {
            const author = item.authors[0]?.fullName || "Unknown Author";

            return (
              <div
                key={item.id}
                className="flex flex-col gap-4 rounded-2xl border border-neutral-200 bg-white p-5 shadow-2xs transition-all hover:border-neutral-300 sm:flex-row sm:items-center sm:justify-between dark:border-zinc-800 dark:bg-zinc-900 dark:hover:border-zinc-700"
              >
                <div className="min-w-0 flex-1 space-y-1">
                  <div className="flex flex-wrap items-center gap-2">
                    <StatusBadge status="PendingReview" />
                    <h3 className="truncate text-base font-bold text-neutral-900 dark:text-white">
                      {item.title}
                    </h3>
                  </div>
                  <p className="line-clamp-2 text-xs text-neutral-500 dark:text-zinc-400">
                    {item.summary}
                  </p>
                  <div className="flex flex-wrap items-center gap-4 pt-1 text-xs text-neutral-400 dark:text-zinc-500">
                    <span className="flex items-center gap-1 font-medium text-neutral-600 dark:text-zinc-300">
                      <User className="size-3" />
                      {author}
                    </span>
                    <span>Slug: {item.slug}</span>
                  </div>
                </div>

                <div className="flex shrink-0 flex-wrap items-center gap-2">
                  {/* View Details / Preview */}
                  <Button
                    variant="outline"
                    size="sm"
                    onClick={() => setSelectedContentId(item.id)}
                  >
                    <Eye className="mr-1.5 size-3.5" />
                    <span>Preview</span>
                  </Button>

                  {/* Reject */}
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
                  >
                    <XCircle className="mr-1.5 size-3.5" />
                    <span>{t("sharing.reject")}</span>
                  </Button>

                  {/* Approve */}
                  <Button
                    size="sm"
                    disabled={approveMutation.isPending}
                    onClick={() => handleApprove(item)}
                    className="bg-emerald-600 text-white hover:bg-emerald-700 dark:bg-emerald-600 dark:hover:bg-emerald-500"
                  >
                    <CheckCircle2 className="mr-1.5 size-3.5" />
                    <span>{t("sharing.approve")}</span>
                  </Button>
                </div>
              </div>
            );
          })}
        </div>
      )}

      {/* Preview Dialog */}
      <Dialog
        open={Boolean(selectedContentId)}
        onOpenChange={(open) => !open && setSelectedContentId(null)}
      >
        <DialogContent className="max-h-[90vh] max-w-4xl overflow-y-auto p-6">
          <DialogHeader>
            <DialogTitle className="text-xl font-bold text-neutral-900 dark:text-white">
              Preview Article
            </DialogTitle>
          </DialogHeader>

          {isLoadingDetail ? (
            <div className="flex min-h-60 items-center justify-center">
              <Loader2 className="size-8 animate-spin text-neutral-400" />
            </div>
          ) : detailData ? (
            <div className="space-y-6 pt-2">
              <div className="border-b border-neutral-100 pb-4 dark:border-zinc-800">
                <h2 className="text-2xl font-bold text-neutral-900 dark:text-white">
                  {detailData.content.title}
                </h2>
                <p className="mt-2 text-sm text-neutral-600 dark:text-zinc-300">
                  {detailData.content.summary}
                </p>
                <div className="mt-3 flex flex-wrap items-center gap-3 text-xs text-neutral-400 dark:text-zinc-500">
                  <span>
                    Author: {detailData.authors[0]?.fullName || "Unknown"}
                  </span>
                  <span>Slug: {detailData.content.slug}</span>
                </div>
              </div>

              {/* Render Markdown */}
              <div>
                <h4 className="mb-2 text-xs font-bold tracking-wider text-neutral-400 uppercase dark:text-zinc-500">
                  Body Content
                </h4>
                <div className="rounded-xl border border-neutral-200 bg-neutral-50/40 p-4 dark:border-zinc-800 dark:bg-zinc-900/40">
                  <MarkdownViewer content={detailData.bodyMarkdown} />
                </div>
              </div>

              {/* Attached Resources */}
              {detailData.resources && detailData.resources.length > 0 && (
                <div>
                  <h4 className="mb-2 text-xs font-bold tracking-wider text-neutral-400 uppercase dark:text-zinc-500">
                    Attachments & Resources
                  </h4>
                  <ResourceList resources={detailData.resources} />
                </div>
              )}

              <DialogFooter className="flex justify-end gap-2 border-t border-neutral-100 pt-4 dark:border-zinc-800">
                <Button
                  variant="outline"
                  onClick={() => setSelectedContentId(null)}
                >
                  Close
                </Button>
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
              </DialogFooter>
            </div>
          ) : null}
        </DialogContent>
      </Dialog>

      {/* Reject Reason Dialog (Mandatory) */}
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
              <span>Reject Submission</span>
            </DialogTitle>
          </DialogHeader>
          <div className="space-y-3 pt-2">
            <p className="text-xs text-neutral-500 dark:text-zinc-400">
              Please specify the reason for rejection or improvements required.
              This will be visible to the author in their Rejected tab.
            </p>
            <textarea
              value={rejectDialogState.reason}
              onChange={(e) =>
                setRejectDialogState((prev) => ({
                  ...prev,
                  reason: e.target.value,
                }))
              }
              placeholder={t("sharing.reviewNotePlaceholder")}
              rows={4}
              className="w-full rounded-xl border border-neutral-200 bg-white p-3 text-sm text-neutral-900 placeholder:text-neutral-400 focus:border-neutral-400 focus:outline-hidden dark:border-zinc-800 dark:bg-zinc-900 dark:text-white dark:placeholder:text-zinc-500"
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
              Cancel
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
              Confirm Reject
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}
