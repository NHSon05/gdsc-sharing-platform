"use client";

import React, { useState } from "react";
import {
  useMineContentsQuery,
  useMineContentByIdQuery,
  useSubmitContentMutation,
  useWithdrawContentMutation,
  ContentFormDialog,
  FeedPostCard,
  type ContentSummary,
  QuickShareComposer,
} from "@/features/sharing";
import { Button } from "@/components/ui/button";
import { AlertCircle, Loader2 } from "lucide-react";
import {
  ProfileBlogFilters,
  type BlogFilterStatus,
} from "./blog/profile-blog-filters";
import { ProfileBlogEmptyState } from "./blog/profile-blog-empty-state";
import { ProfileReviewNoteDialog } from "./blog/profile-review-note-dialog";
import { useCurrentUserQuery } from "@/features/auth";

export interface ProfileBlogSectionProps {
  className?: string;
}

export function ProfileBlogSection({
  className = "",
}: ProfileBlogSectionProps) {
  const [activeFilter, setActiveFilter] = useState<BlogFilterStatus>("ALL");
  const [searchTerm, setSearchTerm] = useState("");
  const [page, setPage] = useState(1);
  const pageSize = 8;

  // Query user's authored contents
  const {
    data: contentsData,
    isLoading,
    isError,
    error,
    refetch,
  } = useMineContentsQuery({
    status: activeFilter === "ALL" ? undefined : activeFilter,
    search: searchTerm.trim() || undefined,
    page,
    pageSize,
  });
  const { data: currentUser } = useCurrentUserQuery();

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

  // Query editing full content when modal is opened
  const { data: editingData, isLoading: isLoadingEditing } =
    useMineContentByIdQuery(editingContentId || "", Boolean(editingContentId));

  const items = contentsData?.items || [];
  const totalCount = contentsData?.totalCount ?? 0;
  const totalPages = Math.max(1, Math.ceil(totalCount / pageSize));

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
      setFeedbackError(apiErr?.message || "Không thể gửi yêu cầu duyệt bài.");
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
      setFeedbackError(apiErr?.message || "Không thể thu hồi bài viết.");
    }
  };

  return (
    <div className={`space-y-6 ${className}`}>
      {/* Top Banner Card */}
      <QuickShareComposer user={currentUser} />

      {/* Filter and Search Bar */}
      <ProfileBlogFilters
        activeFilter={activeFilter}
        onFilterChange={(status) => {
          setActiveFilter(status);
          setPage(1);
          setFeedbackError(null);
        }}
        searchTerm={searchTerm}
        onSearchChange={(search) => {
          setSearchTerm(search);
          setPage(1);
        }}
      />

      {/* Feedback Error Alert */}
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
            Làm mới
          </button>
        </div>
      )}

      {/* Main Content Area */}
      {isLoading ? (
        <div className="flex min-h-64 flex-col items-center justify-center rounded-3xl border border-neutral-200/80 bg-white p-8 text-center dark:border-zinc-800/80 dark:bg-[#0C0C0E]">
          <Loader2 className="text-brand size-8 animate-spin" />
          <p className="mt-3 text-xs font-semibold text-neutral-500 dark:text-zinc-400">
            Đang tải danh sách bài viết...
          </p>
        </div>
      ) : isError ? (
        <div className="flex min-h-64 flex-col items-center justify-center rounded-3xl border border-neutral-200/80 bg-white p-8 text-center dark:border-zinc-800/80 dark:bg-[#0C0C0E]">
          <AlertCircle className="size-8 text-rose-500" />
          <p className="mt-2 text-sm font-bold text-neutral-900 dark:text-zinc-200">
            Không thể tải bài viết
          </p>
          <p className="mt-1 text-xs text-neutral-500 dark:text-zinc-400">
            {error?.message || "Đã xảy ra lỗi khi kết nối máy chủ."}
          </p>
          <Button
            variant="outline"
            size="sm"
            onClick={() => refetch()}
            className="mt-4"
          >
            Thử lại
          </Button>
        </div>
      ) : items.length === 0 ? (
        <ProfileBlogEmptyState
          searchTerm={searchTerm}
          activeFilter={activeFilter}
          onCreateNew={handleCreateNew}
        />
      ) : (
        <div className="space-y-6">
          <div className="space-y-4">
            {items.map((item) => (
              <FeedPostCard
                key={item.id}
                content={item}
                onEdit={handleEdit}
                onSubmitReview={handleSubmitReview}
                onWithdraw={handleWithdraw}
                onViewNote={(content) => {
                  setEditingContentId(content.id);
                  setReviewNoteDialog({
                    isOpen: true,
                    note: "Đang tải phản hồi...",
                    title: content.title,
                  });
                }}
                isSubmitting={submitMutation.isPending}
                isWithdrawing={withdrawMutation.isPending}
              />
            ))}
          </div>

          {/* Pagination */}
          {totalPages > 1 && (
            <div className="flex items-center justify-between border-t border-neutral-100 pt-4 dark:border-zinc-800">
              <span className="text-xs text-neutral-500 dark:text-zinc-400">
                Hiển thị trang {page} / {totalPages} (Tổng cộng {totalCount} bài
                viết)
              </span>

              <div className="flex items-center gap-2">
                <Button
                  variant="outline"
                  size="sm"
                  disabled={page <= 1}
                  onClick={() => setPage((p) => Math.max(1, p - 1))}
                  className="h-8 text-xs"
                >
                  Trước
                </Button>
                <Button
                  variant="outline"
                  size="sm"
                  disabled={page >= totalPages}
                  onClick={() => setPage((p) => p + 1)}
                  className="h-8 text-xs"
                >
                  Sau
                </Button>
              </div>
            </div>
          )}
        </div>
      )}

      {/* Review Note Dialog */}
      <ProfileReviewNoteDialog
        open={reviewNoteDialog.isOpen}
        onOpenChange={(isOpen) =>
          setReviewNoteDialog((prev) => ({ ...prev, isOpen }))
        }
        title={reviewNoteDialog.title}
        note={
          editingData?.reviewNote ||
          (isLoadingEditing
            ? "Đang tải phản hồi..."
            : "Không có ghi chú chi tiết.")
        }
      />

      {/* Content Form Dialog */}
      <ContentFormDialog
        isOpen={isFormOpen}
        onClose={() => {
          setIsFormOpen(false);
          setEditingContentId(null);
        }}
        initialData={editingData}
      />
    </div>
  );
}
