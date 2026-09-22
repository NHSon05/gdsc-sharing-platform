"use client";

import type { ComponentProps } from "react";
import { useState } from "react";
import Link from "next/link";
import { useSearchParams } from "next/navigation";
import { safeReturnPath } from "@/core/session/safe-return-path";
import { cn } from "@/lib/utils";
import { Button } from "@/components/ui/button";
import { Card, CardContent } from "@/components/ui/card";
import {
  Field,
  FieldDescription,
  FieldGroup,
  FieldSeparator,
} from "@/components/ui/field";
import { TextField } from "@/components/ui/input";
import { useTranslation } from "@/core/i18n/i18n.context";
import { useLoginMutation } from "@/features/auth/hooks/use-login-mutation";
import { Loader2 } from "lucide-react";

export function LoginForm({ className, ...props }: ComponentProps<"div">) {
  const { t, locale } = useTranslation();
  const params = useSearchParams();
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const loginMutation = useLoginMutation();
  const [googlePending, setGooglePending] = useState(false);
  const pending = loginMutation.isPending || googlePending;

  const handleSubmit: ComponentProps<"form">["onSubmit"] = (e) => {
    e.preventDefault();
    if (!email || !password || pending) return;

    loginMutation.mutate({
      email: email.trim(),
      password,
    });
  };

  const errorMessage =
    loginMutation.error?.message ||
    (params.get("error") === "external_login_failed"
      ? locale === "vi"
        ? "Không thể đăng nhập Google. Vui lòng thử lại."
        : "Google login failed. Please try again."
      : undefined);
  const validationErrors = loginMutation.error?.validationErrors;

  return (
    <div className={cn("flex flex-col gap-5", className)} {...props}>
      <Card className="overflow-hidden p-0" variant="liquid-glass">
        <CardContent className="grid p-0">
          <form onSubmit={handleSubmit} className="p-6 md:p-8">
            <FieldGroup>
              <div className="mb-1 flex flex-col items-center gap-1.5 text-center">
                <h1 className="text-2xl font-bold tracking-tight text-neutral-900 dark:text-zinc-50">
                  {t("login.title")}
                </h1>
                <p className="text-sm text-neutral-500 dark:text-zinc-400">
                  {t("login.subtitle")}
                </p>
              </div>

              {/* Error Alert if login fails */}
              {errorMessage && (
                <div
                  role="alert"
                  className="animate-in fade-in rounded-xl border border-rose-500/20 bg-rose-500/10 p-3 text-center text-xs font-medium text-rose-600 dark:text-rose-400"
                >
                  {errorMessage}
                </div>
              )}

              {/* Email field */}
              <TextField
                id="email"
                type="email"
                label={t("login.emailLabel")}
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                required
                disabled={pending}
                labelVariant="outside"
                placeholder={t("login.emailPlaceholder")}
                autoComplete="email"
                errorMessage={
                  validationErrors?.Email?.[0] || validationErrors?.email?.[0]
                }
              />

              {/* Password field */}
              <div>
                <div className="mb-1.5 flex items-center justify-between">
                  <label
                    htmlFor="password"
                    className="text-xs font-medium tracking-tight text-neutral-900 md:text-sm dark:text-zinc-200"
                  >
                    {t("login.passwordLabel")}{" "}
                    <span className="text-rose-500">*</span>
                  </label>
                  <a
                    href="#"
                    className="text-brand hover:text-brand-hover text-xs font-medium transition-colors hover:underline"
                  >
                    {t("login.forgotPassword")}
                  </a>
                </div>
                <TextField
                  id="password"
                  type="password"
                  value={password}
                  onChange={(e) => setPassword(e.target.value)}
                  required
                  disabled={pending}
                  placeholder={t("login.passwordPlaceholder")}
                  autoComplete="current-password"
                  errorMessage={
                    validationErrors?.Password?.[0] ||
                    validationErrors?.password?.[0]
                  }
                />
              </div>

              {/* Submit button */}
              <div className="mt-2">
                <Button
                  type="submit"
                  variant="brand"
                  size="md"
                  disabled={pending}
                  className="w-full font-semibold shadow-[0_10px_25px_-5px_var(--brand-glow)] hover:shadow-[0_12px_30px_-4px_var(--brand-glow)]"
                >
                  {loginMutation.isPending ? (
                    <span className="flex items-center justify-center gap-2">
                      <Loader2 className="size-4 animate-spin" />
                      <span>{t("login.signInButton")}...</span>
                    </span>
                  ) : (
                    t("login.signInButton")
                  )}
                </Button>
              </div>

              <FieldSeparator>{t("common.orContinueWith")}</FieldSeparator>

              <Field>
                <Button
                  variant="outline"
                  type="button"
                  disabled={pending}
                  className="w-full font-semibold"
                  onClick={() => {
                    if (pending) return;
                    setGooglePending(true);
                    const returnUrl = safeReturnPath(params.get("returnUrl"));
                    const startUrl = new URL(
                      "/api/auth/google/start",
                      window.location.origin
                    );
                    startUrl.searchParams.set("returnUrl", returnUrl);
                    window.location.assign(startUrl);
                  }}
                >
                  {googlePending && (
                    <Loader2
                      className="size-4 animate-spin"
                      aria-hidden="true"
                    />
                  )}
                  {locale === "vi"
                    ? "Đăng nhập bằng Google"
                    : "Sign in with Google"}
                </Button>
              </Field>

              <FieldDescription className="text-center">
                {t("common.dontHaveAccount")}{" "}
                <Link
                  href="#"
                  className="text-brand font-medium hover:underline"
                >
                  {t("common.signUp")}
                </Link>
              </FieldDescription>
            </FieldGroup>
          </form>
        </CardContent>
      </Card>

      <FieldDescription className="px-6 text-center">
        {t("common.termsNotice")}{" "}
        <a href="#" className="hover:text-brand underline">
          {t("common.termsOfService")}
        </a>{" "}
        {t("common.and")}{" "}
        <a href="#" className="hover:text-brand underline">
          {t("common.privacyPolicy")}
        </a>
        .
      </FieldDescription>
    </div>
  );
}
