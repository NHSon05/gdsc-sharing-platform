"use client";

import React, { useSyncExternalStore } from "react";

function subscribe(callback: () => void) {
  if (typeof window === "undefined") return () => {};
  window.addEventListener("storage", callback);
  const mediaQuery = window.matchMedia("(prefers-color-scheme: dark)");
  mediaQuery.addEventListener("change", callback);
  return () => {
    window.removeEventListener("storage", callback);
    mediaQuery.removeEventListener("change", callback);
  };
}

function getSnapshot(): "light" | "dark" {
  if (typeof document === "undefined") return "light";
  return document.documentElement.classList.contains("dark") ? "dark" : "light";
}

function getServerSnapshot(): "light" | "dark" {
  return "light";
}

export function ThemeToggle() {
  const [mounted, setMounted] = React.useState(false);
  const theme = useSyncExternalStore(subscribe, getSnapshot, getServerSnapshot);

  React.useEffect(() => {
    setMounted(true);
  }, []);

  const toggleTheme = () => {
    const isDark = document.documentElement.classList.contains("dark");
    if (isDark) {
      document.documentElement.classList.remove("dark");
      localStorage.setItem("theme", "light");
    } else {
      document.documentElement.classList.add("dark");
      localStorage.setItem("theme", "dark");
    }
    // Notify other listeners
    window.dispatchEvent(new Event("storage"));
  };

  if (!mounted) {
    return (
      <div className="w-10 h-10 rounded-full bg-neutral-100 dark:bg-zinc-900 border border-neutral-200/80 dark:border-zinc-800" />
    );
  }

  return (
    <button
      type="button"
      onClick={toggleTheme}
      aria-label="Toggle theme"
      className="relative flex items-center justify-center w-10 h-10 rounded-full bg-neutral-100 dark:bg-zinc-900 border border-neutral-200/80 dark:border-zinc-800 text-neutral-700 dark:text-zinc-300 hover:text-neutral-900 dark:hover:text-white transition-all shadow-xs hover:scale-105 active:scale-95 cursor-pointer"
    >
      {theme === "light" ? (
        /* Sun Icon */
        <svg
          className="w-4 h-4 transition-transform duration-300 rotate-0 hover:rotate-45"
          viewBox="0 0 24 24"
          fill="none"
          stroke="currentColor"
          strokeWidth="2"
          strokeLinecap="round"
          strokeLinejoin="round"
        >
          <circle cx="12" cy="12" r="4" />
          <path d="M12 2v2" />
          <path d="M12 20v2" />
          <path d="m4.93 4.93 1.41 1.41" />
          <path d="m17.66 17.66 1.41 1.41" />
          <path d="M2 12h2" />
          <path d="M20 12h2" />
          <path d="m6.34 17.66-1.41 1.41" />
          <path d="m19.07 4.93-1.41 1.41" />
        </svg>
      ) : (
        /* Moon Icon */
        <svg
          className="w-4 h-4 transition-transform duration-300 -rotate-12 hover:rotate-0"
          viewBox="0 0 24 24"
          fill="none"
          stroke="currentColor"
          strokeWidth="2"
          strokeLinecap="round"
          strokeLinejoin="round"
        >
          <path d="M12 3a6 6 0 0 0 9 9 9 9 0 1 1-9-9Z" />
        </svg>
      )}
    </button>
  );
}

