"use client";

import React, { useState } from "react";
import { Building2, Loader2, AlertCircle, Check } from "lucide-react";
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
  useCreateDepartmentMutation,
  useUpdateDepartmentMutation,
} from "../hooks/use-admin-department-mutations";
import type { DepartmentDto } from "@/features/profile/types/profile.types";
import { useTranslation } from "@/core/i18n/i18n.context";

export interface DepartmentCrudDialogProps {
  department?: DepartmentDto | null;
  open: boolean;
  onOpenChange: (open: boolean) => void;
}

function DepartmentCrudForm({
  department,
  onClose,
}: {
  department?: DepartmentDto | null;
  onClose: () => void;
}) {
  const { t } = useTranslation();
  const isEditing = Boolean(department?.id);

  const [name, setName] = useState(department?.name || "");
  const [slug, setSlug] = useState(department?.slug || "");
  const [description, setDescription] = useState(department?.description || "");
  const [color, setColor] = useState(department?.color || "#2563EB");
  const [sortOrder, setSortOrder] = useState(department?.sortOrder || 0);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);

  const createMutation = useCreateDepartmentMutation();
  const updateMutation = useUpdateDepartmentMutation();

  const handleNameChange = (val: string) => {
    setName(val);
    if (!isEditing) {
      const generatedSlug = val
        .toLowerCase()
        .replace(/[^a-z0-9]+/g, "-")
        .replace(/(^-|-$)+/g, "");
      setSlug(generatedSlug);
    }
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    setErrorMessage(null);

    if (!name.trim() || !slug.trim()) {
      setErrorMessage("Tên phòng ban và slug không được để trống.");
      return;
    }

    if (isEditing && department?.id) {
      updateMutation.mutate(
        {
          id: department.id,
          request: {
            name: name.trim(),
            slug: slug.trim(),
            description: description.trim() || null,
            color,
            sortOrder,
          },
        },
        {
          onSuccess: () => onClose(),
          onError: (err) =>
            setErrorMessage(err.message || "Không thể cập nhật phòng ban."),
        }
      );
    } else {
      createMutation.mutate(
        {
          name: name.trim(),
          slug: slug.trim(),
          description: description.trim() || null,
          color,
          sortOrder,
        },
        {
          onSuccess: () => onClose(),
          onError: (err) =>
            setErrorMessage(err.message || "Không thể tạo phòng ban."),
        }
      );
    }
  };

  const isPending = createMutation.isPending || updateMutation.isPending;

  return (
    <>
      <DialogHeader>
        <div className="flex items-center gap-2">
          <Building2 className="text-brand size-5" />
          <DialogTitle>
            {isEditing
              ? t("memberManagement.editDepartment")
              : t("memberManagement.createDepartment")}
          </DialogTitle>
        </div>
        <DialogDescription>
          {isEditing
            ? "Cập nhật thông tin và màu sắc của phòng ban."
            : "Thêm phòng ban mới vào hệ thống câu lạc bộ GDSC."}
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
          id="deptName"
          label={t("memberManagement.deptName")}
          value={name}
          onChange={(e) => handleNameChange(e.target.value)}
          placeholder="VD: Software Engineering"
          required
          disabled={isPending}
        />

        <TextField
          id="deptSlug"
          label={t("memberManagement.deptSlug")}
          value={slug}
          onChange={(e) => setSlug(e.target.value)}
          placeholder="VD: software"
          required
          disabled={isPending}
        />

        <TextField
          id="deptDescription"
          label={t("memberManagement.deptDescription")}
          value={description}
          onChange={(e) => setDescription(e.target.value)}
          placeholder="Mô tả nhiệm vụ của phòng ban..."
          disabled={isPending}
        />

        <div className="grid grid-cols-2 gap-3">
          <div>
            <label
              htmlFor="deptColor"
              className="mb-1.5 block text-xs font-semibold text-neutral-700 dark:text-zinc-300"
            >
              {t("memberManagement.deptColor")}
            </label>
            <div className="flex items-center gap-2">
              <input
                type="color"
                id="deptColor"
                value={color}
                onChange={(e) => setColor(e.target.value)}
                disabled={isPending}
                className="size-9 cursor-pointer rounded-xl border border-neutral-200 p-0.5 dark:border-zinc-700 dark:bg-zinc-800"
              />
              <span className="font-mono text-xs text-neutral-600 dark:text-zinc-300">
                {color}
              </span>
            </div>
          </div>

          <div>
            <TextField
              id="deptSortOrder"
              type="number"
              label={t("memberManagement.deptSortOrder")}
              value={String(sortOrder)}
              onChange={(e) => setSortOrder(Number(e.target.value) || 0)}
              disabled={isPending}
            />
          </div>
        </div>

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

export function DepartmentCrudDialog({
  department,
  open,
  onOpenChange,
}: DepartmentCrudDialogProps) {
  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="max-w-md">
        <DepartmentCrudForm
          key={department?.id || (open ? "new" : "closed")}
          department={department}
          onClose={() => onOpenChange(false)}
        />
      </DialogContent>
    </Dialog>
  );
}
