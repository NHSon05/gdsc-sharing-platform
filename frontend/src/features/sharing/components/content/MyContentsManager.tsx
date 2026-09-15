"use client";

import React, { useState } from "react";
import { useTranslation } from "@/core/i18n/i18n.context";
import { useMineContentsQuery } from "../../hooks/use-contents-query";
import { useMineContentByIdQuery } from "../../hooks/use-content-detail-query";
import {
  useSubmitContentMutation,
  useWithdrawContentMutation,
} from "../../hooks/use-content-mutations";
import { StatusBadge } from "../common/StatusBadge";
import { ContentFormDialog } from "./ContentFormDialog";
import type {
  ContentSummary,
  SharingContentStatus,
} from "../../types/sharing.types";
import {
  FileEdit,
  Clock,
  CheckCircle2,
  XCircle,
  Archive,
  Plus,
  Send,
  RotateCcw,
  Eye,
  AlertCircle,
  Loader2,
  Calendar,
  MessageSquare,
  type LucideIcon,
} from "lucide-react";
import { Button } from "@/components/ui/button";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogFooter,
} from "@/components/ui/dialog";
import { format } from "date-fns";
import type { TranslationKey } from "@/core/i18n/i18n.context";

const TABS: {
  status: SharingContentStatus;
  labelKey: TranslationKey;
  icon: LucideIcon;
}[] = [
  { status: "Draft", labelKey: "sharing.statusDraft", icon: FileEdit },
  {
    status: "PendingReview",
    labelKey: "sharing.statusPendingReview",
    icon: Clock,
  },
  {
    status: "Published",
    labelKey: "sharing.statusPublished",
    icon: CheckCircle2,
  },
  { status: "Rejected", labelKey: "sharing.statusRejected", icon: XCircle },
  { status: "Archived", labelKey: "sharing.statusArchived", icon: Archive },
];

export function MyContentsManager() {
  const { t } = useTranslation();
  const [activeTab, setActiveTab] = useState<SharingContentStatus>("Draft");
  const [page, setPage] = useState(1);

  // Queries
  const {
    data: contentsData,
    isLoading,
    refetch,
  } = useMineContentsQuery({
    status: activeTab,
    page,
    pageSize: 15,
  });

  // Mutations
  const submitMutation = useSubmitContentMutation();
  const withdrawMutation = useWithdrawContentMutation();

  // Dialog States
  const [isFormOpen, setIsFormOpen] = useState(false);
  const [editingContentId, setEditingContentId] = useState<string | null>(null);
  const [reviewNoteDialog, setReviewNoteDialog] = useState<{
    isOpen: boolean;
    note: string;
    title: string;
  }>({ isOpen: false, note: "", title: "" });

  const [feedbackError, setFeedbackError] = useState<string | null>(null);

  // Query editing full content if editing
  const { data: editingData, isLoading: isLoadingEditing } =
    useMineContentByIdQuery(editingContentId || "", Boolean(editingContentId));

  const items = contentsData?.items || [];

  const handleCreateNew = () => {
    setEditingContentId(null);
    setIsFormOpen(true);
  };

  const handleEdit = (item: ContentSummary) => {
    setEditingContentId(item.id);
    setIsFormOpen(true);
  };

  const handleSubmitReview = async (item: ContentSummary) => {
    setFeedbackError(null);
    try {
      await submitMutation.mutateAsync({
        id: item.id,
        version: item.version,
      });
    } catch (err: unknown) {
      const apiErr = err as { status?: number; message?: string } | undefined;
      if (apiErr?.status === 412) {
        setFeedbackError(t("sharing.concurrencyError"));
      } else {
        setFeedbackError(apiErr?.message || "Failed to submit content.");
      }
    }
  };

  const handleWithdraw = async (item: ContentSummary) => {
    setFeedbackError(null);
    try {
      await withdrawMutation.mutateAsync({
        id: item.id,
        version: item.version,
      });
    } catch (err: unknown) {
      const apiErr = err as { status?: number; message?: string } | undefined;
      if (apiErr?.status === 412) {
        setFeedbackError(t("sharing.concurrencyError"));
      } else {
        setFeedbackError(apiErr?.message || "Failed to withdraw submission.");
      }
    }
  };

  return (
    <div className="space-y-6">
      {/* Header Bar: Tabs & Create Button */}
      <div className="flex flex-col gap-4 border-b border-neutral-200 pb-4 sm:flex-row sm:items-center sm:justify-between dark:border-zinc-800">
        {/* Navigation Tabs */}
        <div className="flex flex-wrap items-center gap-1.5 rounded-xl border border-neutral-200/80 bg-white p-1 shadow-2xs dark:border-zinc-800 dark:bg-zinc-900">
          {TABS.map((tab) => {
            const Icon = tab.icon;
            const isActive = activeTab === tab.status;
            return (
              <button
                key={tab.status}
                type="button"
                onClick={() => {
                  setActiveTab(tab.status);
                  setPage(1);
                  setFeedbackError(null);
                }}
                className={`flex cursor-pointer items-center gap-1.5 rounded-lg px-3 py-1.5 text-xs font-semibold transition-all ${
                  isActive
                    ? "bg-brand text-white shadow-xs"
                    : "text-neutral-600 hover:text-neutral-900 dark:text-zinc-400 dark:hover:text-zinc-100"
                }`}
              >
                <Icon className="size-3.5" />
                <span>{t(tab.labelKey)}</span>
              </button>
            );
          })}
        </div>

        {/* Create Button */}
        <Button
          variant="brand"
          onClick={handleCreateNew}
          className="shrink-0 font-semibold shadow-2xs"
        >
          <Plus className="mr-1.5 size-4" />
          <span>{t("sharing.createContent")}</span>
        </Button>
      </div>

      {/* Error alert if any */}
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

      {/* Contents List for active tab */}
      {isLoading ? (
        <div className="flex min-h-60 items-center justify-center">
          <Loader2 className="size-8 animate-spin text-neutral-400" />
        </div>
      ) : items.length === 0 ? (
        <div className="flex min-h-72 flex-col items-center justify-center rounded-2xl border border-dashed border-neutral-200 bg-neutral-50/50 p-8 text-center dark:border-zinc-800 dark:bg-zinc-900/30">
          <p className="text-sm font-semibold text-neutral-600 dark:text-zinc-400">
            {t("sharing.emptyContents")}
          </p>
          <p className="mt-1 text-xs text-neutral-400 dark:text-zinc-500">
            No items in {activeTab} status.
          </p>
        </div>
      ) : (
        <div className="space-y-3">
          {items.map((item) => (
            <div
              key={item.id}
              className="flex flex-col gap-4 rounded-2xl border border-neutral-200 bg-white p-5 shadow-2xs transition-all hover:border-neutral-300 sm:flex-row sm:items-center sm:justify-between dark:border-zinc-800 dark:bg-zinc-900 dark:hover:border-zinc-700"
            >
              {/* Item Info */}
              <div className="min-w-0 flex-1 space-y-1">
                <div className="flex flex-wrap items-center gap-2">
                  <StatusBadge status={item.status} />
                  <h3 className="truncate text-base font-bold text-neutral-900 dark:text-white">
                    {item.title}
                  </h3>
                </div>
                <p className="line-clamp-2 text-xs text-neutral-500 dark:text-zinc-400">
                  {item.summary}
                </p>
                <div className="flex items-center gap-4 pt-1 text-[11px] text-neutral-400 dark:text-zinc-500">
                  <span>Slug: {item.slug}</span>
                  {item.publishedAtUtc && (
                    <span className="flex items-center gap-1">
                      <Calendar className="size-3" />
                      {format(new Date(item.publishedAtUtc), "MMM d, yyyy")}
                    </span>
                  )}
                </div>
              </div>

              {/* Action Buttons */}
              <div className="flex shrink-0 flex-wrap items-center gap-2">
                {/* Draft Actions: Edit, Submit */}
                {item.status === "Draft" && (
                  <>
                    <Button
                      variant="outline"
                      size="sm"
                      onClick={() => handleEdit(item)}
                    >
                      <FileEdit className="mr-1.5 size-3.5" />
                      <span>{t("sharing.editContent")}</span>
                    </Button>
                    <Button
                      variant="brand"
                      size="sm"
                      disabled={submitMutation.isPending}
                      onClick={() => handleSubmitReview(item)}
                    >
                      <Send className="mr-1.5 size-3.5" />
                      <span>{t("sharing.submitForReview")}</span>
                    </Button>
                  </>
                )}

                {/* Pending Review Actions: Withdraw */}
                {item.status === "PendingReview" && (
                  <Button
                    variant="outline"
                    size="sm"
                    disabled={withdrawMutation.isPending}
                    onClick={() => handleWithdraw(item)}
                  >
                    <RotateCcw className="mr-1.5 size-3.5" />
                    <span>{t("sharing.withdraw")}</span>
                  </Button>
                )}

                {/* Rejected Actions: View Note, Edit */}
                {item.status === "Rejected" && (
                  <>
                    <Button
                      variant="outline"
                      size="sm"
                      onClick={async () => {
                        setEditingContentId(item.id);
                        // Open review note dialog
                        setReviewNoteDialog({
                          isOpen: true,
                          note: "Loading review note...",
                          title: item.title,
                        });
                      }}
                    >
                      <MessageSquare className="mr-1.5 size-3.5 text-rose-500" />
                      <span>{t("sharing.reviewNote")}</span>
                    </Button>
                    <Button
                      variant="outline"
                      size="sm"
                      onClick={() => handleEdit(item)}
                    >
                      <FileEdit className="mr-1.5 size-3.5" />
                      <span>{t("sharing.editContent")}</span>
                    </Button>
                  </>
                )}

                {/* Published Actions: View */}
                {item.status === "Published" && (
                  <a
                    href={`/sharing/${item.slug}`}
                    className="inline-flex items-center gap-1.5 rounded-lg border border-neutral-200 bg-white px-3 py-1.5 text-xs font-semibold text-neutral-700 shadow-2xs hover:bg-neutral-50 dark:border-zinc-700 dark:bg-zinc-800 dark:text-zinc-200 dark:hover:bg-zinc-700"
                  >
                    <Eye className="size-3.5" />
                    <span>{t("common.viewDetail")}</span>
                  </a>
                )}
              </div>
            </div>
          ))}
        </div>
      )}

      {/* Review Note Dialog */}
      <Dialog
        open={reviewNoteDialog.isOpen}
        onOpenChange={(open) =>
          setReviewNoteDialog((prev) => ({ ...prev, isOpen: open }))
        }
      >
        <DialogContent className="max-w-md p-6">
          <DialogHeader>
            <DialogTitle className="flex items-center gap-2 text-base font-bold text-neutral-900 dark:text-white">
              <MessageSquare className="size-4 text-rose-500" />
              <span>{t("sharing.reviewNote")}</span>
            </DialogTitle>
          </DialogHeader>
          <div className="mt-2 rounded-xl border border-neutral-200 bg-neutral-50 p-4 text-sm text-neutral-700 dark:border-zinc-800 dark:bg-zinc-900 dark:text-zinc-300">
            {isLoadingEditing ? (
              <Loader2 className="size-4 animate-spin text-neutral-400" />
            ) : (
              <p className="leading-relaxed whitespace-pre-wrap">
                {editingData?.reviewNote || "No reviewer feedback provided."}
              </p>
            )}
          </div>
          <DialogFooter className="mt-4">
            <Button
              variant="outline"
              onClick={() =>
                setReviewNoteDialog((prev) => ({ ...prev, isOpen: false }))
              }
            >
              Close
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      {/* Form Dialog for Create / Edit */}
      {isFormOpen && (
        <ContentFormDialog
          isOpen={isFormOpen}
          onClose={() => {
            setIsFormOpen(false);
            setEditingContentId(null);
          }}
          initialData={editingData}
          onSuccess={() => {
            refetch();
          }}
        />
      )}
    </div>
  );
}
