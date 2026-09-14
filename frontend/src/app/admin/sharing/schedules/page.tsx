"use client";

import React, { useState } from "react";
import Link from "next/link";
import { format } from "date-fns";
import {
  useAdminSchedulesQuery,
  usePublishScheduleMutation,
  useStartScheduleMutation,
  useCompleteScheduleMutation,
  useCancelScheduleMutation,
  useDeleteScheduleMutation,
  StatusBadge,
  DeliveryModeBadge,
  ScheduleFormDialog,
  type ScheduleResponse,
} from "@/features/sharing";
import { useTranslation } from "@/core/i18n/i18n.context";
import { Button } from "@/components/ui/button";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogFooter,
} from "@/components/ui/dialog";
import {
  Clock,
  Plus,
  ArrowLeft,
  Loader2,
  CheckCircle2,
  PlayCircle,
  XCircle,
  Trash2,
  AlertCircle,
  FileEdit,
  Send,
} from "lucide-react";

export default function AdminSchedulesPage() {
  const { t } = useTranslation();

  const {
    data: pageData,
    isLoading,
    refetch,
  } = useAdminSchedulesQuery({
    page: 1,
    pageSize: 50,
  });

  const publishMutation = usePublishScheduleMutation();
  const startMutation = useStartScheduleMutation();
  const completeMutation = useCompleteScheduleMutation();
  const cancelMutation = useCancelScheduleMutation();
  const deleteMutation = useDeleteScheduleMutation();

  const [feedbackError, setFeedbackError] = useState<string | null>(null);

  // Dialog states
  const [isFormOpen, setIsFormOpen] = useState(false);
  const [editingSchedule, setEditingSchedule] =
    useState<ScheduleResponse | null>(null);
  const [cancelDialog, setCancelDialog] = useState<{
    isOpen: boolean;
    item: ScheduleResponse | null;
    reason: string;
  }>({ isOpen: false, item: null, reason: "" });

  const schedules = pageData?.items || [];

  const handleCreate = () => {
    setEditingSchedule(null);
    setIsFormOpen(true);
  };

  const handleEdit = (item: ScheduleResponse) => {
    setEditingSchedule(item);
    setIsFormOpen(true);
  };

  const handlePublish = async (item: ScheduleResponse) => {
    setFeedbackError(null);
    try {
      await publishMutation.mutateAsync({ id: item.id, version: item.version });
    } catch (err: unknown) {
      const apiErr = err as { status?: number; message?: string } | undefined;
      if (apiErr?.status === 412) {
        setFeedbackError(t("sharing.concurrencyError"));
      } else {
        setFeedbackError(apiErr?.message || "Failed to publish schedule.");
      }
    }
  };

  const handleStart = async (item: ScheduleResponse) => {
    setFeedbackError(null);
    try {
      await startMutation.mutateAsync({ id: item.id, version: item.version });
    } catch (err: unknown) {
      const apiErr = err as { status?: number; message?: string } | undefined;
      if (apiErr?.status === 412) {
        setFeedbackError(t("sharing.concurrencyError"));
      } else {
        setFeedbackError(apiErr?.message || "Failed to start schedule.");
      }
    }
  };

  const handleComplete = async (item: ScheduleResponse) => {
    setFeedbackError(null);
    try {
      await completeMutation.mutateAsync({
        id: item.id,
        version: item.version,
      });
    } catch (err: unknown) {
      const apiErr = err as { status?: number; message?: string } | undefined;
      if (apiErr?.status === 412) {
        setFeedbackError(t("sharing.concurrencyError"));
      } else {
        setFeedbackError(apiErr?.message || "Failed to complete schedule.");
      }
    }
  };

  const handleConfirmCancel = async () => {
    const { item, reason } = cancelDialog;
    if (!item || !reason.trim()) return;

    setFeedbackError(null);
    try {
      await cancelMutation.mutateAsync({
        id: item.id,
        request: { reason: reason.trim() },
        version: item.version,
      });
      setCancelDialog({ isOpen: false, item: null, reason: "" });
    } catch (err: unknown) {
      const apiErr = err as { status?: number; message?: string } | undefined;
      if (apiErr?.status === 412) {
        setFeedbackError(t("sharing.concurrencyError"));
      } else {
        setFeedbackError(apiErr?.message || "Failed to cancel schedule.");
      }
    }
  };

  const handleDelete = async (item: ScheduleResponse) => {
    if (
      !confirm("Are you sure you want to permanently delete this schedule?")
    ) {
      return;
    }
    setFeedbackError(null);
    try {
      await deleteMutation.mutateAsync({ id: item.id, version: item.version });
    } catch (err: unknown) {
      const apiErr = err as { status?: number; message?: string } | undefined;
      if (apiErr?.status === 412) {
        setFeedbackError(t("sharing.concurrencyError"));
      } else {
        setFeedbackError(apiErr?.message || "Failed to delete schedule.");
      }
    }
  };

  return (
    <div className="mx-auto max-w-7xl space-y-6 px-4 py-8 sm:px-6 lg:px-8">
      {/* Header Bar */}
      <div className="flex flex-col gap-4 border-b border-neutral-200 pb-5 sm:flex-row sm:items-center sm:justify-between dark:border-zinc-800">
        <div>
          <h1 className="text-2xl font-bold tracking-tight text-neutral-900 sm:text-3xl dark:text-white">
            Manage Sharing Schedules
          </h1>
          <p className="mt-1 text-xs text-neutral-500 dark:text-zinc-400">
            Publish, update status, and manage attendance for club sharing
            events.
          </p>
        </div>

        <div className="flex items-center gap-3">
          <Link
            href="/admin"
            className="inline-flex items-center gap-1 text-xs font-semibold text-neutral-500 hover:text-neutral-900 dark:text-zinc-400 dark:hover:text-white"
          >
            <ArrowLeft className="size-3.5" />
            <span>Dashboard</span>
          </Link>

          <Button onClick={handleCreate}>
            <Plus className="mr-1.5 size-4" />
            <span>{t("sharing.createSchedule")}</span>
          </Button>
        </div>
      </div>

      {/* Error alert */}
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

      {/* List */}
      {isLoading ? (
        <div className="flex min-h-60 items-center justify-center">
          <Loader2 className="size-8 animate-spin text-neutral-400" />
        </div>
      ) : schedules.length === 0 ? (
        <div className="flex min-h-72 flex-col items-center justify-center rounded-2xl border border-dashed border-neutral-200 bg-neutral-50/50 p-8 text-center dark:border-zinc-800 dark:bg-zinc-900/30">
          <p className="text-sm font-semibold text-neutral-600 dark:text-zinc-400">
            {t("sharing.emptySchedules")}
          </p>
        </div>
      ) : (
        <div className="space-y-3">
          {schedules.map((item) => (
            <div
              key={item.id}
              className="flex flex-col gap-4 rounded-2xl border border-neutral-200 bg-white p-5 shadow-2xs transition-all sm:flex-row sm:items-center sm:justify-between dark:border-zinc-800 dark:bg-zinc-900"
            >
              <div className="min-w-0 flex-1 space-y-1">
                <div className="flex flex-wrap items-center gap-2">
                  <DeliveryModeBadge mode={item.deliveryMode} />
                  <StatusBadge status={item.status} />
                  <h3 className="truncate text-base font-bold text-neutral-900 dark:text-white">
                    {item.title}
                  </h3>
                </div>
                <div className="flex flex-wrap items-center gap-3 pt-1 text-xs text-neutral-500 dark:text-zinc-400">
                  <span className="flex items-center gap-1 font-semibold text-neutral-700 dark:text-zinc-300">
                    <Clock className="size-3.5" />
                    {format(new Date(item.startsAtUtc), "MMM d, yyyy HH:mm")}
                  </span>
                  <span>Type: {item.sharingType}</span>
                  {item.presenters[0] && (
                    <span>Speaker: {item.presenters[0].fullName}</span>
                  )}
                </div>
              </div>

              {/* Action Buttons */}
              <div className="flex shrink-0 flex-wrap items-center gap-2">
                {/* Draft: Publish */}
                {item.status === "Draft" && (
                  <Button
                    size="sm"
                    onClick={() => handlePublish(item)}
                    className="bg-blue-600 text-white hover:bg-blue-700"
                  >
                    <Send className="mr-1.5 size-3.5" />
                    <span>Publish</span>
                  </Button>
                )}

                {/* Scheduled: Start */}
                {item.status === "Scheduled" && (
                  <Button
                    size="sm"
                    onClick={() => handleStart(item)}
                    className="bg-amber-600 text-white hover:bg-amber-700"
                  >
                    <PlayCircle className="mr-1.5 size-3.5" />
                    <span>Start Session</span>
                  </Button>
                )}

                {/* InProgress: Complete */}
                {item.status === "InProgress" && (
                  <Button
                    size="sm"
                    onClick={() => handleComplete(item)}
                    className="bg-emerald-600 text-white hover:bg-emerald-700"
                  >
                    <CheckCircle2 className="mr-1.5 size-3.5" />
                    <span>Complete</span>
                  </Button>
                )}

                {/* Edit */}
                <Button
                  variant="outline"
                  size="sm"
                  onClick={() => handleEdit(item)}
                >
                  <FileEdit className="mr-1.5 size-3.5" />
                  <span>Edit</span>
                </Button>

                {/* Cancel (if not Completed / Cancelled) */}
                {item.status !== "Completed" && item.status !== "Cancelled" && (
                  <Button
                    variant="outline"
                    size="sm"
                    onClick={() =>
                      setCancelDialog({ isOpen: true, item, reason: "" })
                    }
                  >
                    <XCircle className="mr-1.5 size-3.5 text-rose-500" />
                    <span>Cancel</span>
                  </Button>
                )}

                {/* Delete */}
                <Button
                  variant="destructive"
                  size="sm"
                  onClick={() => handleDelete(item)}
                >
                  <Trash2 className="size-3.5" />
                </Button>
              </div>
            </div>
          ))}
        </div>
      )}

      {/* Cancel Dialog */}
      <Dialog
        open={cancelDialog.isOpen}
        onOpenChange={(open) =>
          !open && setCancelDialog({ isOpen: false, item: null, reason: "" })
        }
      >
        <DialogContent className="max-w-md p-6">
          <DialogHeader>
            <DialogTitle className="flex items-center gap-2 text-base font-bold text-neutral-900 dark:text-white">
              <XCircle className="size-4 text-rose-500" />
              <span>Cancel Session</span>
            </DialogTitle>
          </DialogHeader>
          <div className="space-y-3 pt-2">
            <p className="text-xs text-neutral-500 dark:text-zinc-400">
              Please enter the reason for cancelling this session.
            </p>
            <textarea
              value={cancelDialog.reason}
              onChange={(e) =>
                setCancelDialog((prev) => ({
                  ...prev,
                  reason: e.target.value,
                }))
              }
              placeholder="e.g. Speaker unavailable, rescheduled..."
              rows={3}
              className="w-full rounded-xl border border-neutral-200 bg-white p-3 text-sm text-neutral-900 placeholder:text-neutral-400 focus:border-neutral-400 focus:outline-hidden dark:border-zinc-800 dark:bg-zinc-900 dark:text-white dark:placeholder:text-zinc-500"
              required
            />
          </div>
          <DialogFooter className="mt-4 flex justify-end gap-2">
            <Button
              variant="outline"
              onClick={() =>
                setCancelDialog({ isOpen: false, item: null, reason: "" })
              }
            >
              Back
            </Button>
            <Button
              variant="destructive"
              disabled={!cancelDialog.reason.trim() || cancelMutation.isPending}
              onClick={handleConfirmCancel}
            >
              {cancelMutation.isPending && (
                <Loader2 className="mr-1.5 size-3.5 animate-spin" />
              )}
              Confirm Cancel
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      {/* Form Dialog for Create / Edit */}
      {isFormOpen && (
        <ScheduleFormDialog
          isOpen={isFormOpen}
          onClose={() => {
            setIsFormOpen(false);
            setEditingSchedule(null);
          }}
          initialData={editingSchedule}
          isAdmin={true}
          onSuccess={() => {
            refetch();
          }}
        />
      )}
    </div>
  );
}
