"use client";

import React, {
  createContext,
  useContext,
  useCallback,
  useSyncExternalStore,
} from "react";
import { en, type TranslationDictionary } from "./locales/en";
import { vi } from "./locales/vi";

export type Locale = "en" | "vi";

export const DEFAULT_LOCALE: Locale = "en";
export const SUPPORTED_LOCALES: Locale[] = ["en", "vi"];

const dictionaries: Record<Locale, TranslationDictionary> = {
  en,
  vi,
};

interface I18nContextType {
  locale: Locale;
  setLocale: (locale: Locale) => void;
  dict: TranslationDictionary;
  t: (key: NestedKeyOf<TranslationDictionary>) => string;
}

// Type-safe nested key path helper (e.g. "hero.title", "login.emailLabel")
type NestedKeyOf<ObjectType extends object> = {
  [Key in keyof ObjectType & (string | number)]: ObjectType[Key] extends object
    ? `${Key}.${NestedKeyOf<ObjectType[Key]>}`
    : `${Key}`;
}[keyof ObjectType & (string | number)];

const I18nContext = createContext<I18nContextType | null>(null);

const LOCALE_STORAGE_KEY = "gdsc_locale";

function getClientLocale(): Locale {
  if (typeof window === "undefined") return DEFAULT_LOCALE;
  try {
    const saved = localStorage.getItem(LOCALE_STORAGE_KEY) as Locale;
    if (saved && (saved === "en" || saved === "vi")) {
      return saved;
    }
  } catch {
    // ignore
  }
  return DEFAULT_LOCALE;
}

const listeners = new Set<() => void>();

function subscribe(callback: () => void) {
  listeners.add(callback);
  return () => listeners.delete(callback);
}

function getSnapshot(): Locale {
  return getClientLocale();
}

function getServerSnapshot(): Locale {
  return DEFAULT_LOCALE;
}

export function I18nProvider({ children }: { children: React.ReactNode }) {
  const locale = useSyncExternalStore(
    subscribe,
    getSnapshot,
    getServerSnapshot
  );

  const handleSetLocale = useCallback((newLocale: Locale) => {
    try {
      localStorage.setItem(LOCALE_STORAGE_KEY, newLocale);
      document.cookie = `NEXT_LOCALE=${newLocale};path=/;max-age=31536000;SameSite=Lax`;
      document.documentElement.lang = newLocale;
    } catch {
      // ignore
    }
    listeners.forEach((l) => l());
  }, []);

  const dict = dictionaries[locale] || dictionaries.en;

  const t = useCallback(
    (keyPath: NestedKeyOf<TranslationDictionary>): string => {
      const keys = (keyPath as string).split(".");
      let current: unknown = dict;

      for (const key of keys) {
        if (current && typeof current === "object" && key in current) {
          current = (current as Record<string, unknown>)[key];
        } else {
          // Fallback to English dictionary
          let fallback: unknown = dictionaries.en;
          for (const fallbackKey of keys) {
            if (
              fallback &&
              typeof fallback === "object" &&
              fallbackKey in fallback
            ) {
              fallback = (fallback as Record<string, unknown>)[fallbackKey];
            } else {
              return keyPath;
            }
          }
          return typeof fallback === "string" ? fallback : keyPath;
        }
      }

      return typeof current === "string" ? current : keyPath;
    },
    [dict]
  );

  return (
    <I18nContext.Provider
      value={{
        locale,
        setLocale: handleSetLocale,
        dict,
        t,
      }}
    >
      {children}
    </I18nContext.Provider>
  );
}

export function useTranslation() {
  const context = useContext(I18nContext);
  if (!context) {
    // Fallback when used outside provider
    return {
      locale: DEFAULT_LOCALE,
      setLocale: () => {},
      dict: dictionaries.en,
      t: (keyPath: NestedKeyOf<TranslationDictionary>) => {
        const keys = (keyPath as string).split(".");
        let cur: unknown = dictionaries.en;
        for (const k of keys) {
          if (cur && typeof cur === "object" && k in cur) {
            cur = (cur as Record<string, unknown>)[k];
          } else {
            return keyPath;
          }
        }
        return typeof cur === "string" ? cur : keyPath;
      },
    };
  }
  return context;
}
