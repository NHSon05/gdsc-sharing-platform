"use client";

import React, { useState, useRef } from "react";
import Image from "next/image";
import {
  UploadCloud,
  Trash2,
  Camera,
  Loader2,
  AlertCircle,
} from "lucide-react";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogDescription,
  DialogFooter,
} from "@/components/ui/dialog";
import { Button } from "@/components/ui/button";
import { useUploadAvatarMutation } from "../hooks/use-upload-avatar-mutation";
import { useDeleteAvatarMutation } from "../hooks/use-delete-avatar-mutation";
import { useTranslation } from "@/core/i18n/i18n.context";
import { cn } from "@/lib/utils";

export interface AvatarUploaderProps {
  currentAvatarUrl?: string | null;
  displayName: string;
  open: boolean;
  onOpenChange: (open: boolean) => void;
}

const MAX_FILE_SIZE = 2 * 1024 * 1024; // 2MB
const ALLOWED_TYPES = ["image/jpeg", "image/png", "image/webp"];

export function AvatarUploader({
  currentAvatarUrl,
  displayName,
  open,
  onOpenChange,
}: AvatarUploaderProps) {
  const { t } = useTranslation();
  const fileInputRef = useRef<HTMLInputElement>(null);

  const [selectedFile, setSelectedFile] = useState<File | null>(null);
  const [previewUrl, setPreviewUrl] = useState<string | null>(null);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);

  const uploadMutation = useUploadAvatarMutation();
  const deleteMutation = useDeleteAvatarMutation();

  const handleFileSelect = (file: File) => {
    setErrorMessage(null);

    if (!ALLOWED_TYPES.includes(file.type)) {
      setErrorMessage("Chỉ hỗ trợ file ảnh định dạng .jpg, .png hoặc .webp.");
      return;
    }

    if (file.size > MAX_FILE_SIZE) {
      setErrorMessage(
        "Kích thước file vượt quá giới hạn cho phép (tối đa 2MB)."
      );
      return;
    }

    setSelectedFile(file);
    const objectUrl = URL.createObjectURL(file);
    setPreviewUrl(objectUrl);
  };

  const handleDragOver = (e: React.DragEvent) => {
    e.preventDefault();
  };

  const handleDrop = (e: React.DragEvent) => {
    e.preventDefault();
    if (e.dataTransfer.files && e.dataTransfer.files[0]) {
      handleFileSelect(e.dataTransfer.files[0]);
    }
  };

  const handleUpload = () => {
    if (!selectedFile) return;

    uploadMutation.mutate(selectedFile, {
      onSuccess: () => {
        handleClose();
      },
      onError: (err) => {
        setErrorMessage(err.message || "Không thể tải lên ảnh đại diện.");
      },
    });
  };

  const handleDelete = () => {
    deleteMutation.mutate(undefined, {
      onSuccess: () => {
        handleClose();
      },
      onError: (err) => {
        setErrorMessage(err.message || "Không thể xóa ảnh đại diện.");
      },
    });
  };

  const handleClose = () => {
    setSelectedFile(null);
    if (previewUrl) {
      URL.revokeObjectURL(previewUrl);
      setPreviewUrl(null);
    }
    setErrorMessage(null);
    onOpenChange(false);
  };

  const initialLetter = displayName.charAt(0).toUpperCase() || "U";
  const displayImage = previewUrl || currentAvatarUrl;
  const isPending = uploadMutation.isPending || deleteMutation.isPending;

  return (
    <Dialog open={open} onOpenChange={handleClose}>
      <DialogContent className="max-w-md">
        <DialogHeader>
          <DialogTitle>{t("profile.avatarTitle")}</DialogTitle>
          <DialogDescription>{t("profile.uploadPrompt")}</DialogDescription>
        </DialogHeader>

        <div className="space-y-6">
          {/* Error message */}
          {errorMessage && (
            <div className="flex items-center gap-2 rounded-2xl border border-rose-200 bg-rose-50 p-3 text-xs text-rose-700 dark:border-rose-900/40 dark:bg-rose-950/40 dark:text-rose-400">
              <AlertCircle className="size-4 shrink-0" />
              <span>{errorMessage}</span>
            </div>
          )}

          {/* Avatar Preview Center */}
          <div className="flex flex-col items-center justify-center gap-3">
            <div className="relative flex size-28 items-center justify-center overflow-hidden rounded-full border-4 border-neutral-100 shadow-md ring-2 ring-neutral-200 dark:border-zinc-800 dark:ring-zinc-700">
              {displayImage ? (
                <Image
                  src={displayImage}
                  alt={displayName}
                  fill
                  className="object-cover"
                />
              ) : (
                <div className="bg-brand text-brand-foreground flex size-full items-center justify-center text-3xl font-bold">
                  {initialLetter}
                </div>
              )}
            </div>
          </div>

          {/* Drag & Drop Upload Zone */}
          <div
            onDragOver={handleDragOver}
            onDrop={handleDrop}
            onClick={() => fileInputRef.current?.click()}
            className={cn(
              "hover:border-brand/60 hover:bg-brand/[0.02] dark:hover:border-brand/60 flex cursor-pointer flex-col items-center justify-center rounded-2xl border-2 border-dashed border-neutral-200/80 bg-neutral-50/50 p-6 text-center transition-colors dark:border-zinc-800 dark:bg-zinc-900/40",
              isPending && "pointer-events-none opacity-50"
            )}
          >
            <input
              ref={fileInputRef}
              type="file"
              accept={ALLOWED_TYPES.join(",")}
              onChange={(e) => {
                if (e.target.files && e.target.files[0]) {
                  handleFileSelect(e.target.files[0]);
                }
              }}
              className="hidden"
            />
            <div className="bg-brand/10 text-brand mb-2 flex size-10 items-center justify-center rounded-xl">
              <UploadCloud className="size-5" />
            </div>
            <p className="text-xs font-semibold text-neutral-800 dark:text-zinc-200">
              {selectedFile ? selectedFile.name : t("profile.changeAvatar")}
            </p>
            <p className="mt-1 text-[11px] text-neutral-400 dark:text-zinc-500">
              JPEG, PNG, WEBP (Max 2MB)
            </p>
          </div>
        </div>

        <DialogFooter className="mt-6">
          {currentAvatarUrl && !selectedFile && (
            <Button
              type="button"
              variant="outline"
              size="md"
              disabled={isPending}
              onClick={handleDelete}
              leftIcon={<Trash2 className="size-4 text-rose-500" />}
              className="hover:border-rose-300 hover:bg-rose-50 hover:text-rose-600 dark:hover:bg-rose-950/30"
            >
              {t("profile.deleteAvatar")}
            </Button>
          )}

          <Button
            type="button"
            variant="outline"
            size="md"
            disabled={isPending}
            onClick={handleClose}
          >
            {t("memberManagement.cancel")}
          </Button>

          {selectedFile && (
            <Button
              type="button"
              variant="brand"
              size="md"
              disabled={isPending}
              onClick={handleUpload}
              leftIcon={
                isPending ? (
                  <Loader2 className="size-4 animate-spin" />
                ) : (
                  <Camera className="size-4" />
                )
              }
            >
              {isPending ? "Đang tải lên..." : "Tải lên ảnh mới"}
            </Button>
          )}
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
