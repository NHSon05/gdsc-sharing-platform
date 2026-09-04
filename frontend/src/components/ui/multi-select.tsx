"use client";

import * as React from "react";
import { Check, ChevronDown, X, Search } from "lucide-react";
import { cn } from "@/lib/utils";

export interface MultiSelectOption {
  value: string;
  label: string;
  description?: string;
  color?: string;
  badge?: string;
}

export interface MultiSelectProps {
  options: MultiSelectOption[];
  value: string[];
  onChange: (value: string[]) => void;
  placeholder?: string;
  searchPlaceholder?: string;
  disabled?: boolean;
  className?: string;
  maxDisplayCount?: number;
}

export function MultiSelect({
  options = [],
  value = [],
  onChange,
  placeholder = "Select options...",
  searchPlaceholder = "Search...",
  disabled = false,
  className,
  maxDisplayCount = 3,
}: MultiSelectProps) {
  const [isOpen, setIsOpen] = React.useState(false);
  const [search, setSearch] = React.useState("");
  const containerRef = React.useRef<HTMLDivElement>(null);

  // Close when clicking outside
  React.useEffect(() => {
    const handleClickOutside = (e: MouseEvent) => {
      if (
        containerRef.current &&
        !containerRef.current.contains(e.target as Node)
      ) {
        setIsOpen(false);
      }
    };
    document.addEventListener("mousedown", handleClickOutside);
    return () => document.removeEventListener("mousedown", handleClickOutside);
  }, []);

  const filteredOptions = React.useMemo(() => {
    if (!search.trim()) return options;
    const q = search.toLowerCase();
    return options.filter(
      (opt) =>
        opt.label.toLowerCase().includes(q) ||
        opt.description?.toLowerCase().includes(q)
    );
  }, [options, search]);

  const toggleOption = (optValue: string, e?: React.MouseEvent) => {
    e?.stopPropagation();
    if (disabled) return;
    if (value.includes(optValue)) {
      onChange(value.filter((v) => v !== optValue));
    } else {
      onChange([...value, optValue]);
    }
  };

  const removeOption = (optValue: string, e: React.MouseEvent) => {
    e.stopPropagation();
    if (disabled) return;
    onChange(value.filter((v) => v !== optValue));
  };

  const clearAll = (e: React.MouseEvent) => {
    e.stopPropagation();
    if (disabled) return;
    onChange([]);
  };

  const selectedOptions = React.useMemo(() => {
    return options.filter((opt) => value.includes(opt.value));
  }, [options, value]);

  return (
    <div ref={containerRef} className={cn("relative w-full", className)}>
      {/* Trigger Box */}
      <div
        role="button"
        tabIndex={disabled ? -1 : 0}
        onClick={() => !disabled && setIsOpen(!isOpen)}
        onKeyDown={(e) => {
          if (e.key === "Enter" || e.key === " ") {
            e.preventDefault();
            if (!disabled) setIsOpen(!isOpen);
          }
        }}
        className={cn(
          "flex min-h-11 w-full cursor-pointer items-center justify-between gap-2 rounded-2xl border border-neutral-200/90 bg-white px-3.5 py-2 text-sm shadow-2xs transition-all dark:border-zinc-800 dark:bg-zinc-900/90",
          isOpen && "border-brand ring-brand/20 ring-2",
          disabled && "cursor-not-allowed opacity-50"
        )}
      >
        <div className="flex flex-1 flex-wrap items-center gap-1.5 overflow-hidden">
          {selectedOptions.length === 0 ? (
            <span className="text-neutral-400 select-none dark:text-zinc-500">
              {placeholder}
            </span>
          ) : (
            <>
              {selectedOptions.slice(0, maxDisplayCount).map((opt) => (
                <span
                  key={opt.value}
                  className="bg-brand-muted text-brand dark:bg-brand/15 dark:text-brand inline-flex items-center gap-1 rounded-xl px-2.5 py-1 text-xs font-semibold shadow-2xs select-none"
                >
                  <span className="max-w-[120px] truncate">{opt.label}</span>
                  <button
                    type="button"
                    onClick={(e) => removeOption(opt.value, e)}
                    aria-label={`Remove ${opt.label}`}
                    className="rounded-xs transition-colors hover:text-rose-600 dark:hover:text-rose-400"
                  >
                    <X className="size-3" />
                  </button>
                </span>
              ))}

              {selectedOptions.length > maxDisplayCount && (
                <span className="rounded-xl border border-neutral-200 bg-neutral-100 px-2 py-0.5 text-xs font-semibold text-neutral-600 select-none dark:border-zinc-700 dark:bg-zinc-800 dark:text-zinc-300">
                  +{selectedOptions.length - maxDisplayCount}
                </span>
              )}
            </>
          )}
        </div>

        {/* Right Action Icons */}
        <div className="flex shrink-0 items-center gap-1.5 text-neutral-400 dark:text-zinc-500">
          {selectedOptions.length > 0 && !disabled && (
            <button
              type="button"
              onClick={clearAll}
              title="Clear all"
              className="p-0.5 transition-colors hover:text-neutral-700 dark:hover:text-zinc-200"
            >
              <X className="size-3.5" />
            </button>
          )}
          <ChevronDown
            className={cn(
              "size-4 transition-transform duration-200",
              isOpen && "rotate-180"
            )}
          />
        </div>
      </div>

      {/* Dropdown Menu */}
      {isOpen && (
        <div className="animate-in fade-in zoom-in-95 absolute top-full z-50 mt-1.5 max-h-60 w-full overflow-hidden rounded-2xl border border-neutral-200/90 bg-white shadow-xl dark:border-zinc-800 dark:bg-zinc-900">
          {/* Search Input */}
          <div className="border-b border-neutral-100 p-2 dark:border-zinc-800/80">
            <div className="relative flex items-center">
              <Search className="absolute left-2.5 size-3.5 text-neutral-400 dark:text-zinc-500" />
              <input
                type="text"
                value={search}
                onChange={(e) => setSearch(e.target.value)}
                placeholder={searchPlaceholder}
                className="w-full rounded-xl bg-neutral-50 py-1.5 pr-3 pl-8 text-xs text-neutral-900 placeholder:text-neutral-400 focus:outline-hidden dark:bg-zinc-800/60 dark:text-zinc-100 dark:placeholder:text-zinc-500"
                onClick={(e) => e.stopPropagation()}
              />
            </div>
          </div>

          {/* Options List */}
          <div className="max-h-48 scrollbar-none overflow-y-auto p-1.5">
            {filteredOptions.length === 0 ? (
              <div className="py-4 text-center text-xs text-neutral-400 select-none dark:text-zinc-500">
                No results found
              </div>
            ) : (
              filteredOptions.map((opt) => {
                const isSelected = value.includes(opt.value);
                return (
                  <div
                    key={opt.value}
                    onClick={(e) => toggleOption(opt.value, e)}
                    className={cn(
                      "flex cursor-pointer items-center justify-between rounded-xl px-3 py-2 text-xs font-medium transition-colors select-none",
                      isSelected
                        ? "bg-brand/10 text-brand dark:bg-brand/15 dark:text-brand font-semibold"
                        : "text-neutral-700 hover:bg-neutral-100/70 dark:text-zinc-300 dark:hover:bg-zinc-800/60"
                    )}
                  >
                    <div className="flex flex-col">
                      <div className="flex items-center gap-2">
                        {opt.color && (
                          <span
                            className="size-2 shrink-0 rounded-full"
                            style={{ backgroundColor: opt.color }}
                          />
                        )}
                        <span>{opt.label}</span>
                      </div>
                      {opt.description && (
                        <span className="text-[10px] text-neutral-400 dark:text-zinc-500">
                          {opt.description}
                        </span>
                      )}
                    </div>

                    <div
                      className={cn(
                        "flex size-4 items-center justify-center rounded-md border transition-all",
                        isSelected
                          ? "border-brand bg-brand text-white"
                          : "border-neutral-300 dark:border-zinc-700"
                      )}
                    >
                      {isSelected && <Check className="size-3 stroke-[3]" />}
                    </div>
                  </div>
                );
              })
            )}
          </div>
        </div>
      )}
    </div>
  );
}
