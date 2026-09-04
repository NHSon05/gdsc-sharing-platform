"use client";

import React, { useState } from "react";
import {
  Mail,
  KeyRound,
  CheckCircle2,
  Loader2,
  AlertCircle,
} from "lucide-react";
import { useRouter } from "next/navigation";
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
  useRequestEmailChangeMutation,
  useConfirmEmailChangeMutation,
} from "../hooks/use-email-change-mutation";
import { useTranslation } from "@/core/i18n/i18n.context";

export interface EmailChangeDialogProps {
  currentEmail: string;
  open: boolean;
  onOpenChange: (open: boolean) => void;
}

export function EmailChangeDialog({
  currentEmail,
  open,
  onOpenChange,
}: EmailChangeDialogProps) {
  const { t } = useTranslation();
  const router = useRouter();

  const [step, setStep] = useState<"request" | "confirm" | "success">(
    "request"
  );
  const [newEmail, setNewEmail] = useState("");
  const [password, setPassword] = useState("");
  const [token, setToken] = useState("");
  const [errorMessage, setErrorMessage] = useState<string | null>(null);

  const requestMutation = useRequestEmailChangeMutation();
  const confirmMutation = useConfirmEmailChangeMutation();

  const handleRequestSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    setErrorMessage(null);

    if (!newEmail.trim() || !password) return;

    requestMutation.mutate(
      { newEmail: newEmail.trim(), currentPassword: password },
      {
        onSuccess: () => {
          setStep("confirm");
        },
        onError: (err) => {
          setErrorMessage(
            err.message ||
              "Không thể gửi yêu cầu đổi email. Vui lòng kiểm tra lại mật khẩu."
          );
        },
      }
    );
  };

  const handleConfirmSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    setErrorMessage(null);

    if (!token.trim()) return;

    confirmMutation.mutate(
      { token: token.trim(), email: newEmail.trim() },
      {
        onSuccess: () => {
          setStep("success");
          setTimeout(() => {
            handleClose();
            router.push("/login");
          }, 2000);
        },
        onError: (err) => {
          setErrorMessage(
            err.message || "Mã xác nhận không hợp lệ hoặc đã hết hạn."
          );
        },
      }
    );
  };

  const handleClose = () => {
    setStep("request");
    setNewEmail("");
    setPassword("");
    setToken("");
    setErrorMessage(null);
    onOpenChange(false);
  };

  const isPending = requestMutation.isPending || confirmMutation.isPending;

  return (
    <Dialog open={open} onOpenChange={handleClose}>
      <DialogContent className="max-w-md">
        <DialogHeader>
          <DialogTitle>
            {step === "confirm"
              ? t("profile.confirmTitle")
              : step === "success"
                ? "Hoàn tất đổi Email"
                : t("profile.emailChangeTitle")}
          </DialogTitle>
          <DialogDescription>
            {step === "confirm"
              ? t("profile.confirmDesc")
              : step === "success"
                ? t("profile.emailChangeSuccess")
                : t("profile.emailChangeDesc")}
          </DialogDescription>
        </DialogHeader>

        {/* Error message */}
        {errorMessage && (
          <div className="mb-4 flex items-center gap-2 rounded-2xl border border-rose-200 bg-rose-50 p-3 text-xs text-rose-700 dark:border-rose-900/40 dark:bg-rose-950/40 dark:text-rose-400">
            <AlertCircle className="size-4 shrink-0" />
            <span>{errorMessage}</span>
          </div>
        )}

        {/* Step 1: Request Form */}
        {step === "request" && (
          <form onSubmit={handleRequestSubmit} className="space-y-4">
            <div>
              <label className="mb-1 block text-xs font-semibold text-neutral-500 dark:text-zinc-400">
                Email hiện tại
              </label>
              <div className="flex items-center gap-2 rounded-2xl border border-neutral-200 bg-neutral-50 px-3.5 py-2.5 text-xs text-neutral-600 dark:border-zinc-800 dark:bg-zinc-900/60 dark:text-zinc-400">
                <Mail className="size-4 text-neutral-400" />
                <span>{currentEmail}</span>
              </div>
            </div>

            <TextField
              id="newEmail"
              type="email"
              label={t("profile.newEmail")}
              value={newEmail}
              onChange={(e) => setNewEmail(e.target.value)}
              placeholder={t("profile.newEmailPlaceholder")}
              required
              disabled={isPending}
            />

            <TextField
              id="currentPassword"
              type="password"
              label={t("profile.currentPassword")}
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              placeholder={t("profile.currentPasswordPlaceholder")}
              required
              disabled={isPending}
            />

            <DialogFooter className="mt-6">
              <Button
                type="button"
                variant="outline"
                size="md"
                disabled={isPending}
                onClick={handleClose}
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
                    <Mail className="size-4" />
                  )
                }
              >
                {isPending ? "Đang gửi..." : t("profile.sendConfirmation")}
              </Button>
            </DialogFooter>
          </form>
        )}

        {/* Step 2: Confirm Form */}
        {step === "confirm" && (
          <form onSubmit={handleConfirmSubmit} className="space-y-4">
            <div className="border-brand/20 bg-brand/5 rounded-2xl border p-3.5 text-xs text-neutral-700 dark:text-zinc-300">
              Mã xác minh gồm chuỗi ký tự đã được gửi đến:{" "}
              <strong className="text-brand">{newEmail}</strong>.
            </div>

            <TextField
              id="token"
              type="text"
              label={t("profile.tokenLabel")}
              value={token}
              onChange={(e) => setToken(e.target.value)}
              placeholder={t("profile.tokenPlaceholder")}
              required
              disabled={isPending}
              startIcon={<KeyRound className="size-4 text-neutral-400" />}
            />

            <DialogFooter className="mt-6">
              <Button
                type="button"
                variant="outline"
                size="md"
                disabled={isPending}
                onClick={() => setStep("request")}
              >
                Quay lại
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
                    <CheckCircle2 className="size-4" />
                  )
                }
              >
                {isPending ? "Đang xác thực..." : t("profile.confirmButton")}
              </Button>
            </DialogFooter>
          </form>
        )}

        {/* Step 3: Success Screen */}
        {step === "success" && (
          <div className="flex flex-col items-center justify-center p-6 text-center">
            <div className="mb-4 flex size-14 items-center justify-center rounded-full bg-emerald-500/10 text-emerald-600">
              <CheckCircle2 className="size-8" />
            </div>
            <h4 className="text-base font-bold text-neutral-900 dark:text-white">
              Đổi Email Thành Công!
            </h4>
            <p className="mt-1.5 text-xs text-neutral-500 dark:text-zinc-400">
              Đang chuyển hướng về trang đăng nhập...
            </p>
          </div>
        )}
      </DialogContent>
    </Dialog>
  );
}
