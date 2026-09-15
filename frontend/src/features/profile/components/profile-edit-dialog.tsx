"use client";

import React from "react";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { ProfileForm } from "./profile-form";
import type { UserProfileDto } from "../types/profile.types";
import { UserCheck } from "lucide-react";

export interface ProfileEditDialogProps {
  profile: UserProfileDto;
  open: boolean;
  onOpenChange: (open: boolean) => void;
}

export function ProfileEditDialog({
  profile,
  open,
  onOpenChange,
}: ProfileEditDialogProps) {
  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="max-h-[90vh] max-w-2xl overflow-y-auto rounded-3xl p-6 sm:p-8">
        <DialogHeader className="mb-2">
          <div className="flex items-center gap-2 text-emerald-600 dark:text-emerald-400">
            <UserCheck className="size-5" />
            <DialogTitle className="text-lg font-bold">
              Chỉnh sửa thông tin hồ sơ
            </DialogTitle>
          </div>
        </DialogHeader>

        <ProfileForm
          profile={profile}
          className="border-0 p-0 shadow-none dark:bg-transparent"
        />
      </DialogContent>
    </Dialog>
  );
}
