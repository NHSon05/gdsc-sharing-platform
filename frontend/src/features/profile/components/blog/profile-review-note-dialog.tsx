"use client";

import React from "react";
import { MessageSquare } from "lucide-react";
import { Button } from "@/components/ui/button";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogFooter,
} from "@/components/ui/dialog";

export interface ProfileReviewNoteDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  title: string;
  note: string;
}

export function ProfileReviewNoteDialog({
  open,
  onOpenChange,
  title,
  note,
}: ProfileReviewNoteDialogProps) {
  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="max-w-md rounded-3xl">
        <DialogHeader>
          <div className="flex items-center gap-2 text-amber-600 dark:text-amber-400">
            <MessageSquare className="size-5" />
            <DialogTitle className="text-base font-bold">
              Phản hồi từ Ban Quản trị
            </DialogTitle>
          </div>
          <p className="mt-1 text-xs text-neutral-500 dark:text-zinc-400">
            Bài viết: <span className="font-semibold text-neutral-800 dark:text-zinc-200">{title}</span>
          </p>
        </DialogHeader>

        <div className="my-2 rounded-2xl border border-neutral-200 bg-neutral-50 p-4 text-xs leading-relaxed text-neutral-700 dark:border-zinc-800 dark:bg-zinc-900/80 dark:text-zinc-300">
          {note || "Không có ghi chú chi tiết từ người kiểm duyệt."}
        </div>

        <DialogFooter>
          <Button
            variant="outline"
            size="sm"
            onClick={() => onOpenChange(false)}
            className="w-full sm:w-auto"
          >
            Đóng
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
