"use client";

import React, { useState } from "react";
import { Check, Copy } from "lucide-react";

interface MarkdownViewerProps {
  content: string;
  className?: string;
}

function CodeBlock({ code, language }: { code: string; language?: string }) {
  const [copied, setCopied] = useState(false);

  const handleCopy = async () => {
    try {
      await navigator.clipboard.writeText(code);
      setCopied(true);
      setTimeout(() => setCopied(false), 2000);
    } catch {
      // ignore
    }
  };

  return (
    <div className="relative my-4 overflow-hidden rounded-xl border border-neutral-200 bg-neutral-900 text-neutral-100 dark:border-zinc-800">
      <div className="flex items-center justify-between border-b border-neutral-800 bg-neutral-950 px-4 py-1.5 text-xs text-neutral-400">
        <span className="font-mono">{language || "text"}</span>
        <button
          onClick={handleCopy}
          type="button"
          className="flex cursor-pointer items-center gap-1 rounded px-2 py-0.5 text-neutral-400 transition-colors hover:text-white"
        >
          {copied ? (
            <>
              <Check className="size-3 text-emerald-400" />
              <span className="text-emerald-400">Copied</span>
            </>
          ) : (
            <>
              <Copy className="size-3" />
              <span>Copy</span>
            </>
          )}
        </button>
      </div>
      <pre className="overflow-x-auto p-4 font-mono text-sm leading-relaxed">
        <code>{code}</code>
      </pre>
    </div>
  );
}

function parseInline(text: string): React.ReactNode[] {
  // Regex to match inline code, bold, italic, and links safely
  const regex = /(`[^`]+`|\*\*[^*]+\*\*|\*[^*]+\*|\[[^\]]+\]\([^)]+\))/g;
  const parts = text.split(regex);

  return parts.map((part, index) => {
    if (!part) return null;

    // Inline code
    if (part.startsWith("`") && part.endsWith("`")) {
      return (
        <code
          key={index}
          className="rounded bg-neutral-100 px-1.5 py-0.5 font-mono text-xs font-semibold text-pink-600 dark:bg-zinc-800 dark:text-pink-400"
        >
          {part.slice(1, -1)}
        </code>
      );
    }

    // Bold
    if (part.startsWith("**") && part.endsWith("**")) {
      return (
        <strong
          key={index}
          className="font-bold text-neutral-900 dark:text-white"
        >
          {part.slice(2, -2)}
        </strong>
      );
    }

    // Italic
    if (part.startsWith("*") && part.endsWith("*")) {
      return (
        <em key={index} className="text-neutral-800 italic dark:text-zinc-200">
          {part.slice(1, -1)}
        </em>
      );
    }

    // Link [text](url)
    const linkMatch = part.match(/^\[([^\]]+)\]\(([^)]+)\)$/);
    if (linkMatch) {
      const href = linkMatch[2];
      const isExternal =
        href.startsWith("http://") || href.startsWith("https://");
      return (
        <a
          key={index}
          href={href}
          target={isExternal ? "_blank" : undefined}
          rel={isExternal ? "noopener noreferrer" : undefined}
          className="font-medium text-blue-600 underline underline-offset-2 hover:text-blue-700 dark:text-blue-400 dark:hover:text-blue-300"
        >
          {linkMatch[1]}
        </a>
      );
    }

    return part;
  });
}

export function MarkdownViewer({ content, className }: MarkdownViewerProps) {
  if (!content) {
    return null;
  }

  const lines = content.split("\n");
  const elements: React.ReactNode[] = [];

  let inCodeBlock = false;
  let codeBlockLang = "";
  let codeBlockContent: string[] = [];
  let inList = false;
  let listItems: string[] = [];
  let listOrdered = false;

  const flushList = (key: number) => {
    if (listItems.length > 0) {
      if (listOrdered) {
        elements.push(
          <ol
            key={`ol-${key}`}
            className="my-3 list-decimal space-y-1 pl-6 text-sm text-neutral-700 dark:text-zinc-300"
          >
            {listItems.map((item, i) => (
              <li key={i}>{parseInline(item)}</li>
            ))}
          </ol>
        );
      } else {
        elements.push(
          <ul
            key={`ul-${key}`}
            className="my-3 list-disc space-y-1 pl-6 text-sm text-neutral-700 dark:text-zinc-300"
          >
            {listItems.map((item, i) => (
              <li key={i}>{parseInline(item)}</li>
            ))}
          </ul>
        );
      }
      listItems = [];
      inList = false;
    }
  };

  lines.forEach((line, index) => {
    // Fenced Code Blocks ```
    if (line.trim().startsWith("```")) {
      flushList(index);
      if (inCodeBlock) {
        elements.push(
          <CodeBlock
            key={`code-${index}`}
            code={codeBlockContent.join("\n")}
            language={codeBlockLang}
          />
        );
        inCodeBlock = false;
        codeBlockLang = "";
        codeBlockContent = [];
      } else {
        inCodeBlock = true;
        codeBlockLang = line.trim().slice(3).trim();
      }
      return;
    }

    if (inCodeBlock) {
      codeBlockContent.push(line);
      return;
    }

    // Horizontal Rule
    if (/^(\*\*\*|---|___)$/.test(line.trim())) {
      flushList(index);
      elements.push(
        <hr
          key={`hr-${index}`}
          className="my-6 border-t border-neutral-200 dark:border-zinc-800"
        />
      );
      return;
    }

    // Headings
    if (line.startsWith("# ")) {
      flushList(index);
      elements.push(
        <h1
          key={`h1-${index}`}
          className="mt-8 mb-4 text-2xl font-bold tracking-tight text-neutral-900 sm:text-3xl dark:text-white"
        >
          {parseInline(line.slice(2))}
        </h1>
      );
      return;
    }

    if (line.startsWith("## ")) {
      flushList(index);
      elements.push(
        <h2
          key={`h2-${index}`}
          className="mt-6 mb-3 text-xl font-bold tracking-tight text-neutral-900 sm:text-2xl dark:text-white"
        >
          {parseInline(line.slice(3))}
        </h2>
      );
      return;
    }

    if (line.startsWith("### ")) {
      flushList(index);
      elements.push(
        <h3
          key={`h3-${index}`}
          className="mt-5 mb-2 text-lg font-bold text-neutral-900 sm:text-xl dark:text-white"
        >
          {parseInline(line.slice(4))}
        </h3>
      );
      return;
    }

    if (line.startsWith("#### ")) {
      flushList(index);
      elements.push(
        <h4
          key={`h4-${index}`}
          className="mt-4 mb-2 text-base font-semibold text-neutral-900 dark:text-white"
        >
          {parseInline(line.slice(5))}
        </h4>
      );
      return;
    }

    // Blockquote
    if (line.startsWith("> ")) {
      flushList(index);
      elements.push(
        <blockquote
          key={`quote-${index}`}
          className="my-3 rounded-r-lg border-l-4 border-blue-500 bg-blue-50/50 py-2 pr-4 pl-4 text-neutral-700 italic dark:bg-blue-950/20 dark:text-zinc-300"
        >
          {parseInline(line.slice(2))}
        </blockquote>
      );
      return;
    }

    // Unordered List (- or *)
    const ulMatch = line.match(/^(\s*)[-*]\s+(.+)$/);
    if (ulMatch) {
      if (!inList || listOrdered) {
        flushList(index);
        inList = true;
        listOrdered = false;
      }
      listItems.push(ulMatch[2]);
      return;
    }

    // Ordered List (1. )
    const olMatch = line.match(/^(\s*)\d+\.\s+(.+)$/);
    if (olMatch) {
      if (!inList || !listOrdered) {
        flushList(index);
        inList = true;
        listOrdered = true;
      }
      listItems.push(olMatch[2]);
      return;
    }

    // Regular line / Paragraph
    flushList(index);
    if (line.trim().length > 0) {
      elements.push(
        <p
          key={`p-${index}`}
          className="my-2.5 text-sm leading-relaxed text-neutral-700 dark:text-zinc-300"
        >
          {parseInline(line)}
        </p>
      );
    }
  });

  flushList(lines.length);

  return (
    <div className={`prose-neutral max-w-none ${className ?? ""}`}>
      {elements}
    </div>
  );
}
