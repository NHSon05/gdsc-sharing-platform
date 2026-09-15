"use client";

import React, { useState } from "react";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogFooter,
} from "@/components/ui/dialog";
import { Button } from "@/components/ui/button";
import { TextField } from "@/components/ui/input";
import { MarkdownEditor } from "../common/MarkdownEditor";
import { useTranslation } from "@/core/i18n/i18n.context";
import { useTagsQuery } from "../../hooks/use-tags-query";
import {
  useCreateContentMutation,
  useUpdateContentMutation,
  useSubmitContentMutation,
  useApproveContentMutation,
} from "../../hooks/use-content-mutations";
import { useSessionStore } from "@/core/session/session.store";
import { selectCurrentUser } from "@/core/session/session.selectors";
import type {
  ContentRequest,
  ContentResponse,
} from "../../types/sharing.types";
import { AlertCircle, Loader2, Tag, Send } from "lucide-react";

interface ContentFormDialogProps {
  isOpen: boolean;
  onClose: () => void;
  initialData?: ContentResponse | null;
  onSuccess?: (content: ContentResponse) => void;
}

function generateSlug(text: string): string {
  return text
    .toLowerCase()
    .normalize("NFD")
    .replace(/[\u0300-\u036f]/g, "")
    .replace(/đ/g, "d")
    .replace(/[^a-z0-9]+/g, "-")
    .replace(/^-+|-+$/g, "");
}

interface ContentFormInnerProps {
  initialData?: ContentResponse | null;
  onClose: () => void;
  onSuccess?: (content: ContentResponse) => void;
}

function ContentFormInner({
  initialData,
  onClose,
  onSuccess,
}: ContentFormInnerProps) {
  const { t } = useTranslation();
  const { data: tags = [] } = useTagsQuery();

  const createMutation = useCreateContentMutation();
  const updateMutation = useUpdateContentMutation();
  const submitMutation = useSubmitContentMutation();
  const approveMutation = useApproveContentMutation();

  const storeUser = useSessionStore(selectCurrentUser);
  const isAdminOrLead = storeUser?.roles?.some((r) =>
    ["Admin", "Lead", "SubLead", "CoreTeam"].includes(r)
  );

  const isEditing = Boolean(initialData);

  const [title, setTitle] = useState(initialData?.content.title ?? "");
  const [slug, setSlug] = useState(initialData?.content.slug ?? "");
  const [summary, setSummary] = useState(initialData?.content.summary ?? "");
  const [coverImageUrl, setCoverImageUrl] = useState(
    initialData?.content.coverImageUrl ?? ""
  );
  const [bodyMarkdown, setBodyMarkdown] = useState(
    initialData?.bodyMarkdown ?? "# Giới thiệu\n\nNội dung chia sẻ..."
  );
  const [selectedTagIds, setSelectedTagIds] = useState<string[]>(
    initialData?.tags?.map((item) => item.id) ?? []
  );
  const [errorMessage, setErrorMessage] = useState<string | null>(null);
  const [activeAction, setActiveAction] = useState<"draft" | "publish">(
    "publish"
  );

  const handleTitleChange = (val: string) => {
    setTitle(val);
    if (!isEditing) {
      setSlug(generateSlug(val));
    }
  };

  const toggleTag = (tagId: string) => {
    setSelectedTagIds((prev) =>
      prev.includes(tagId)
        ? prev.filter((id) => id !== tagId)
        : [...prev, tagId]
    );
  };

  const handleSave = async (action: "draft" | "publish") => {
    setActiveAction(action);
    setErrorMessage(null);

    if (!title.trim()) {
      setErrorMessage("Title is required.");
      return;
    }
    if (!slug.trim()) {
      setErrorMessage("Slug is required.");
      return;
    }
    if (!summary.trim()) {
      setErrorMessage("Summary is required.");
      return;
    }
    if (!bodyMarkdown.trim()) {
      setErrorMessage("Content markdown is required.");
      return;
    }

    const payload: ContentRequest = {
      title: title.trim(),
      slug: slug.trim(),
      summary: summary.trim(),
      bodyMarkdown: bodyMarkdown.trim(),
      tagIds: selectedTagIds,
      contributorUserIds: [],
      coverImageUrl: coverImageUrl.trim() || null,
    };

    try {
      if (isEditing && initialData) {
        const result = await updateMutation.mutateAsync({
          id: initialData.content.id,
          request: payload,
          version: initialData.content.version,
        });
        onSuccess?.(result);
      } else {
        const created = await createMutation.mutateAsync(payload);
        if (action === "publish") {
          try {
            const submitted = await submitMutation.mutateAsync({
              id: created.content.id,
              version: created.content.version,
            });

            if (isAdminOrLead) {
              try {
                const approved = await approveMutation.mutateAsync({
                  id: submitted.content.id,
                  version: submitted.content.version,
                });
                onSuccess?.(approved);
                onClose();
                return;
              } catch {
                // fall back to submitted
              }
            }
            onSuccess?.(submitted);
          } catch {
            onSuccess?.(created);
          }
        } else {
          onSuccess?.(created);
        }
      }
      onClose();
    } catch (err: unknown) {
      const apiErr = err as { status?: number; message?: string } | undefined;
      if (apiErr?.status === 412) {
        setErrorMessage(t("sharing.concurrencyError"));
      } else {
        setErrorMessage(
          apiErr?.message || "An error occurred while saving the content."
        );
      }
    }
  };

  const isPending =
    createMutation.isPending ||
    updateMutation.isPending ||
    submitMutation.isPending ||
    approveMutation.isPending;

  return (
    <>
      <DialogHeader>
        <DialogTitle className="text-xl font-bold text-neutral-900 dark:text-white">
          {isEditing ? t("sharing.editContent") : t("sharing.createContent")}
        </DialogTitle>
      </DialogHeader>

      {errorMessage && (
        <div className="flex items-center gap-2 rounded-xl border border-rose-200 bg-rose-50 p-3 text-xs font-semibold text-rose-700 dark:border-rose-900/60 dark:bg-rose-950/40 dark:text-rose-300">
          <AlertCircle className="size-4 shrink-0" />
          <span>{errorMessage}</span>
        </div>
      )}

      <form
        onSubmit={(e) => {
          e.preventDefault();
          handleSave("publish");
        }}
        className="space-y-4"
      >
        {/* Title & Slug */}
        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
          <div>
            <label className="mb-1 block text-xs font-semibold text-neutral-700 dark:text-zinc-300">
              {t("sharing.contentTitle")} *
            </label>
            <TextField
              value={title}
              onChange={(e) => handleTitleChange(e.target.value)}
              placeholder="e.g. Clean Architecture in ASP.NET Core"
              required
            />
          </div>
          <div>
            <label className="mb-1 block text-xs font-semibold text-neutral-700 dark:text-zinc-300">
              {t("sharing.slug")} *
            </label>
            <TextField
              value={slug}
              onChange={(e) => setSlug(e.target.value)}
              placeholder="clean-architecture-aspnet-core"
              required
            />
          </div>
        </div>

        {/* Summary */}
        <div>
          <label className="mb-1 block text-xs font-semibold text-neutral-700 dark:text-zinc-300">
            {t("sharing.summary")} *
          </label>
          <textarea
            value={summary}
            onChange={(e) => setSummary(e.target.value)}
            placeholder="Brief summary of what this sharing covers..."
            rows={2}
            className="w-full rounded-xl border border-neutral-200 bg-white p-3 text-sm text-neutral-900 placeholder:text-neutral-400 focus:border-neutral-400 focus:outline-hidden dark:border-zinc-800 dark:bg-zinc-900 dark:text-white dark:placeholder:text-zinc-500"
            required
          />
        </div>

        {/* Cover Image URL */}
        <div>
          <label className="mb-1 block text-xs font-semibold text-neutral-700 dark:text-zinc-300">
            {t("sharing.coverImageUrl")}
          </label>
          <TextField
            value={coverImageUrl}
            onChange={(e) => setCoverImageUrl(e.target.value)}
            placeholder="https://images.unsplash.com/photo-..."
          />
        </div>

        {/* Tags Selection */}
        {tags.length > 0 && (
          <div>
            <label className="mb-1 block text-xs font-semibold text-neutral-700 dark:text-zinc-300">
              {t("sharing.tags")}
            </label>
            <div className="flex flex-wrap gap-1.5 pt-1">
              {tags.map((tag) => {
                const selected = selectedTagIds.includes(tag.id);
                return (
                  <button
                    key={tag.id}
                    type="button"
                    onClick={() => toggleTag(tag.id)}
                    className={`inline-flex cursor-pointer items-center gap-1 rounded-lg px-2.5 py-1 text-xs font-semibold transition-all ${
                      selected
                        ? "bg-brand text-white shadow-xs"
                        : "border border-neutral-200 bg-neutral-50 text-neutral-600 hover:bg-neutral-100 dark:border-zinc-800 dark:bg-zinc-900 dark:text-zinc-400 dark:hover:bg-zinc-800"
                    }`}
                  >
                    <Tag className="size-3" />
                    <span>{tag.name}</span>
                  </button>
                );
              })}
            </div>
          </div>
        )}

        {/* Markdown Content */}
        <div>
          <label className="mb-1 block text-xs font-semibold text-neutral-700 dark:text-zinc-300">
            {t("sharing.bodyMarkdown")} *
          </label>
          <MarkdownEditor
            value={bodyMarkdown}
            onChange={setBodyMarkdown}
            placeholder="Write your article in Markdown..."
            minHeight="min-h-80"
          />
        </div>

        <DialogFooter className="flex flex-wrap items-center justify-end gap-2 border-t border-neutral-100 pt-4 dark:border-zinc-800">
          <Button
            type="button"
            variant="outline"
            onClick={onClose}
            disabled={isPending}
          >
            Cancel
          </Button>

          {!isEditing && (
            <Button
              type="button"
              variant="subtle"
              disabled={isPending}
              onClick={() => handleSave("draft")}
            >
              {isPending && activeAction === "draft" && (
                <Loader2 className="mr-2 size-4 animate-spin" />
              )}
              {t("sharing.saveDraft")}
            </Button>
          )}

          <Button
            type="button"
            variant="brand"
            disabled={isPending}
            onClick={() => handleSave("publish")}
            className="font-semibold shadow-xs"
          >
            {isPending && activeAction === "publish" ? (
              <Loader2 className="mr-2 size-4 animate-spin" />
            ) : (
              <Send className="mr-2 size-4" />
            )}
            {isEditing ? "Save Changes" : t("sharing.sharePost")}
          </Button>
        </DialogFooter>
      </form>
    </>
  );
}

export function ContentFormDialog({
  isOpen,
  onClose,
  initialData,
  onSuccess,
}: ContentFormDialogProps) {
  return (
    <Dialog open={isOpen} onOpenChange={(open) => !open && onClose()}>
      <DialogContent className="max-h-[90vh] max-w-4xl overflow-y-auto p-6">
        {isOpen && (
          <ContentFormInner
            key={initialData?.content.id ?? "new-content"}
            initialData={initialData}
            onClose={onClose}
            onSuccess={onSuccess}
          />
        )}
      </DialogContent>
    </Dialog>
  );
}
