"use client";

import React, { useEffect } from "react";
import { QueryProvider } from "@/core/query/query-provider";
import { I18nProvider } from "@/core/i18n/i18n.context";
import { initSessionFromCookies } from "@/core/session/session.store";

export function AppProviders({ children }: { children: React.ReactNode }) {
  useEffect(() => {
    initSessionFromCookies();
  }, []);

  return (
    <QueryProvider>
      <I18nProvider>{children}</I18nProvider>
    </QueryProvider>
  );
}
