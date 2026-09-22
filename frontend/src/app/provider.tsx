"use client";
import React from "react";
import { QueryProvider } from "@/core/query/query-provider";
import { I18nProvider } from "@/core/i18n/i18n.context";
import { SessionBoundary } from "@/features/auth/components/session-boundary";

export function AppProviders({ children }: { children: React.ReactNode }) {
  return (
    <QueryProvider>
      <SessionBoundary>
        <I18nProvider>{children}</I18nProvider>
      </SessionBoundary>
    </QueryProvider>
  );
}
