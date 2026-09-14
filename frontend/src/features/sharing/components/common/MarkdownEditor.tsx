"use client";

import React, { useState, useRef } from "react";
import { MarkdownViewer } from "./MarkdownViewer";
import { useTranslation } from "@/core/i18n/i18n.context";
import {
  Bold,
  Italic,
  Code,
  Quote,
  List,
  Heading2,
  Link2,
  FileCode,
  Eye,
  PenTool,
} from "lucide-react";

interface MarkdownEditorProps {
  value: string;
  onChange: (value: string) => void;
  placeholder?: string;
  minHeight?: string;
  className?: string;
}

export function MarkdownEditor({
  value,
  onChange,
  placeholder,
  minHeight = "min-h-72",
  className,
}: MarkdownEditorProps) {
  const { t } = useTranslation();
  const [activeTab, setActiveTab] = useState<"write" | "preview">("write");
  const textareaRef = useRef<HTMLTextAreaElement>(null);

  const insertText = (prefix: string, suffix = "", defaultText = "") => {
    const textarea = textareaRef.current;
    if (!textarea) return;

    const start = textarea.selectionStart;
    const end = textarea.selectionEnd;
    const selectedText = value.substring(start, end) || defaultText;

    const replacement = `${prefix}${selectedText}${suffix}`;
    const newValue =
      value.substring(0, start) + replacement + value.substring(end);

    onChange(newValue);

    setTimeout(() => {
      textarea.focus();
      textarea.setSelectionRange(
        start + prefix.length,
        start + prefix.length + selectedText.length
      );
    }, 0);
  };

  return (
    <div
      className={`overflow-hidden rounded-xl border border-neutral-200 bg-white dark:border-zinc-800 dark:bg-zinc-950 ${
        className ?? ""
      }`}
    >
      {/* Top Header with Tabs and Format Bar */}
      <div className="flex flex-wrap items-center justify-between border-b border-neutral-200 bg-neutral-50/80 px-3 py-2 dark:border-zinc-800 dark:bg-zinc-900/60">
        {/* Write / Preview Tab switcher */}
        <div className="flex items-center gap-1 rounded-lg border border-neutral-200/80 bg-white p-0.5 shadow-2xs dark:border-zinc-800 dark:bg-zinc-900">
          <button
            type="button"
            onClick={() => setActiveTab("write")}
            className={`flex items-center gap-1.5 rounded-md px-3 py-1 text-xs font-semibold transition-all ${
              activeTab === "write"
                ? "bg-brand text-white shadow-xs"
                : "hover:text-brand dark:hover:text-brand text-neutral-600 dark:text-zinc-400"
            }`}
          >
            <PenTool className="size-3" />
            <span>{t("sharing.write")}</span>
          </button>
          <button
            type="button"
            onClick={() => setActiveTab("preview")}
            className={`flex items-center gap-1.5 rounded-md px-3 py-1 text-xs font-semibold transition-all ${
              activeTab === "preview"
                ? "bg-brand text-white shadow-xs"
                : "hover:text-brand dark:hover:text-brand text-neutral-600 dark:text-zinc-400"
            }`}
          >
            <Eye className="size-3" />
            <span>{t("sharing.preview")}</span>
          </button>
        </div>

        {/* Action Toolbar (visible when writing) */}
        {activeTab === "write" && (
          <div className="flex items-center gap-1">
            <button
              type="button"
              onClick={() => insertText("## ", "", "Heading")}
              title="Heading 2"
              className="rounded p-1.5 text-neutral-500 hover:bg-neutral-200/70 hover:text-neutral-900 dark:text-zinc-400 dark:hover:bg-zinc-800 dark:hover:text-white"
            >
              <Heading2 className="size-4" />
            </button>
            <button
              type="button"
              onClick={() => insertText("**", "**", "bold text")}
              title="Bold"
              className="rounded p-1.5 text-neutral-500 hover:bg-neutral-200/70 hover:text-neutral-900 dark:text-zinc-400 dark:hover:bg-zinc-800 dark:hover:text-white"
            >
              <Bold className="size-4" />
            </button>
            <button
              type="button"
              onClick={() => insertText("*", "*", "italic text")}
              title="Italic"
              className="rounded p-1.5 text-neutral-500 hover:bg-neutral-200/70 hover:text-neutral-900 dark:text-zinc-400 dark:hover:bg-zinc-800 dark:hover:text-white"
            >
              <Italic className="size-4" />
            </button>
            <button
              type="button"
              onClick={() => insertText("`", "`", "code")}
              title="Inline Code"
              className="rounded p-1.5 text-neutral-500 hover:bg-neutral-200/70 hover:text-neutral-900 dark:text-zinc-400 dark:hover:bg-zinc-800 dark:hover:text-white"
            >
              <Code className="size-4" />
            </button>
            <button
              type="button"
              onClick={() => insertText("```typescript\n", "\n```", "// code")}
              title="Code Block"
              className="rounded p-1.5 text-neutral-500 hover:bg-neutral-200/70 hover:text-neutral-900 dark:text-zinc-400 dark:hover:bg-zinc-800 dark:hover:text-white"
            >
              <FileCode className="size-4" />
            </button>
            <button
              type="button"
              onClick={() => insertText("> ", "", "quote")}
              title="Quote"
              className="rounded p-1.5 text-neutral-500 hover:bg-neutral-200/70 hover:text-neutral-900 dark:text-zinc-400 dark:hover:bg-zinc-800 dark:hover:text-white"
            >
              <Quote className="size-4" />
            </button>
            <button
              type="button"
              onClick={() => insertText("- ", "", "item")}
              title="Bullet List"
              className="rounded p-1.5 text-neutral-500 hover:bg-neutral-200/70 hover:text-neutral-900 dark:text-zinc-400 dark:hover:bg-zinc-800 dark:hover:text-white"
            >
              <List className="size-4" />
            </button>
            <button
              type="button"
              onClick={() =>
                insertText("[", "](https://example.com)", "Link Title")
              }
              title="Link"
              className="rounded p-1.5 text-neutral-500 hover:bg-neutral-200/70 hover:text-neutral-900 dark:text-zinc-400 dark:hover:bg-zinc-800 dark:hover:text-white"
            >
              <Link2 className="size-4" />
            </button>
          </div>
        )}
      </div>

      {/* Editor Body */}
      <div className="p-3">
        {activeTab === "write" ? (
          <textarea
            ref={textareaRef}
            value={value}
            onChange={(e) => onChange(e.target.value)}
            placeholder={placeholder}
            className={`w-full resize-y font-mono text-sm leading-relaxed text-neutral-900 placeholder:text-neutral-400 focus:outline-hidden dark:text-zinc-100 dark:placeholder:text-zinc-600 ${minHeight}`}
          />
        ) : (
          <div className={`overflow-y-auto px-2 ${minHeight}`}>
            {value.trim() ? (
              <MarkdownViewer content={value} />
            ) : (
              <p className="text-sm text-neutral-400 italic dark:text-zinc-600">
                {t("sharing.preview")}: (Empty content)
              </p>
            )}
          </div>
        )}
      </div>
    </div>
  );
}
