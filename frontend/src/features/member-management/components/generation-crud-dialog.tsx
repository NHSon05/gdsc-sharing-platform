"use client";

import React, { useState } from "react";
import { Layers, Loader2, AlertCircle, Check } from "lucide-react";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogDescription,
  DialogFooter,
} from "@/components/ui/dialog";
import { Button } from "@/components/ui/button";
import { TextField } from "@/components/ui/input";
import {
  useCreateGenerationMutation,
  useUpdateGenerationMutation,
} from "../hooks/use-admin-generation-mutations";
import type { GenerationDto } from "@/features/profile/types/profile.types";
import { useTranslation } from "@/core/i18n/i18n.context";

export interface GenerationCrudDialogProps {
  generation?: GenerationDto | null;
  open: boolean;
  onOpenChange: (open: boolean) => void;
}

function GenerationCrudForm({
  generation,
  onClose,
}: {
  generation?: GenerationDto | null;
  onClose: () => void;
}) {
  const { t } = useTranslation();
  const isEditing = Boolean(generation?.id);

  const [number, setNumber] = useState<number>(generation?.number || 1);
  const [startDate, setStartDate] = useState(
    generation?.startDate?.split("T")[0] || ""
  );
  const [endDate, setEndDate] = useState(
    generation?.endDate?.split("T")[0] || ""
  );
  const [errorMessage, setErrorMessage] = useState<string | null>(null);

  const createMutation = useCreateGenerationMutation();
  const updateMutation = useUpdateGenerationMutation();

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    setErrorMessage(null);

    if (number < 1) {
      setErrorMessage("Số thứ tự Gen phải lớn hơn hoặc bằng 1.");
      return;
    }

    if (startDate && endDate && new Date(startDate) > new Date(endDate)) {
      setErrorMessage("Ngày bắt đầu không được sau ngày kết thúc.");
      return;
    }

    if (isEditing && generation?.id) {
      updateMutation.mutate(
        {
          id: generation.id,
          request: {
            startDate: startDate || null,
            endDate: endDate || null,
          },
        },
        {
          onSuccess: () => onClose(),
          onError: (err) =>
            setErrorMessage(err.message || "Không thể cập nhật nhiệm kỳ."),
        }
      );
    } else {
      createMutation.mutate(
        {
          number,
          startDate: startDate || null,
          endDate: endDate || null,
        },
        {
          onSuccess: () => onClose(),
          onError: (err) =>
            setErrorMessage(err.message || "Không thể tạo nhiệm kỳ."),
        }
      );
    }
  };

  const isPending = createMutation.isPending || updateMutation.isPending;

  return (
    <>
      <DialogHeader>
        <div className="flex items-center gap-2">
          <Layers className="text-brand size-5" />
          <DialogTitle>
            {isEditing
              ? t("memberManagement.editGeneration")
              : t("memberManagement.createGeneration")}
          </DialogTitle>
        </div>
        <DialogDescription>
          {isEditing
            ? "Cập nhật thời gian hoạt động của nhiệm kỳ."
            : "Thêm nhiệm kỳ Gen mới vào câu lạc bộ GDSC."}
        </DialogDescription>
      </DialogHeader>

      {errorMessage && (
        <div className="mb-4 flex items-center gap-2 rounded-2xl border border-rose-200 bg-rose-50 p-3 text-xs text-rose-700 dark:border-rose-900/40 dark:bg-rose-950/40 dark:text-rose-400">
          <AlertCircle className="size-4 shrink-0" />
          <span>{errorMessage}</span>
        </div>
      )}

      <form onSubmit={handleSubmit} className="space-y-4">
        <TextField
          id="genNumber"
          type="number"
          label={t("memberManagement.genNumber")}
          value={String(number)}
          onChange={(e) => setNumber(Number(e.target.value) || 1)}
          disabled={isEditing || isPending}
          required
        />

        <TextField
          id="genStartDate"
          type="date"
          label={t("memberManagement.genStartDate")}
          value={startDate}
          onChange={(e) => setStartDate(e.target.value)}
          disabled={isPending}
        />

        <TextField
          id="genEndDate"
          type="date"
          label={t("memberManagement.genEndDate")}
          value={endDate}
          onChange={(e) => setEndDate(e.target.value)}
          disabled={isPending}
        />

        <DialogFooter className="mt-6">
          <Button
            type="button"
            variant="outline"
            size="md"
            disabled={isPending}
            onClick={onClose}
          >
            {t("memberManagement.cancel")}
          </Button>
          <Button
            type="submit"
            variant="brand"
            size="md"
            disabled={isPending}
            leftIcon={
              isPending ? (
                <Loader2 className="size-4 animate-spin" />
              ) : (
                <Check className="size-4" />
              )
            }
          >
            {isPending ? "Đang lưu..." : t("memberManagement.save")}
          </Button>
        </DialogFooter>
      </form>
    </>
  );
}

export function GenerationCrudDialog({
  generation,
  open,
  onOpenChange,
}: GenerationCrudDialogProps) {
  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="max-w-md">
        <GenerationCrudForm
          key={generation?.id || (open ? "new" : "closed")}
          generation={generation}
          onClose={() => onOpenChange(false)}
        />
      </DialogContent>
    </Dialog>
  );
}
