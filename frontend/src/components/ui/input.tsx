"use client";

import * as React from "react";
import { cn } from "@/lib/utils";

export interface TextFieldProps extends Omit<
  React.ComponentProps<"input">,
  "size"
> {
  /** Label text */
  label?: string;
  /** Whether the field is required (displays red asterisk *) */
  required?: boolean;
  /** Position of the label: "outside" (top) or "inside" (embedded top row) */
  labelVariant?: "outside" | "inside";
  /** Helper text displayed below the field */
  supportingText?: string;
  /** Error state flag or custom error message text */
  error?: boolean | string;
  /** Explicit error message string */
  errorMessage?: string;
  /** Leading icon / slot (e.g. search icon) */
  startIcon?: React.ReactNode;
  /** Trailing custom action or content */
  endIcon?: React.ReactNode;
  /** Enable quick clear (x) button when input has value */
  clearable?: boolean;
  /** Callback when clear button is clicked */
  onClear?: () => void;
  /** Trailing value with separator and chevron dropdown (matching Figma design system) */
  trailingValue?: string | { label: string; onClick?: () => void };
  /** Input size */
  sizeVariant?: "sm" | "md" | "lg";
}

const TextField = React.forwardRef<HTMLInputElement, TextFieldProps>(
  (
    {
      className,
      label,
      required,
      labelVariant = "outside",
      supportingText,
      error,
      errorMessage: errorMessageProp,
      startIcon,
      endIcon,
      clearable = false,
      onClear,
      trailingValue,
      sizeVariant = "md",
      disabled,
      value,
      defaultValue,
      onChange,
      placeholder,
      id: customId,
      ...props
    },
    ref
  ) => {
    const generatedId = React.useId();
    const id = customId || generatedId;

    const [internalValue, setInternalValue] = React.useState<string>(
      (value !== undefined
        ? value
        : defaultValue !== undefined
          ? defaultValue
          : "") as string
    );

    const isControlled = value !== undefined;
    const currentValue = isControlled ? (value as string) : internalValue;
    const hasValue =
      currentValue !== undefined &&
      currentValue !== null &&
      currentValue !== "";

    const isError = Boolean(error || errorMessageProp);
    const errorMessage =
      errorMessageProp || (typeof error === "string" ? error : undefined);

    const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
      if (!isControlled) {
        setInternalValue(e.target.value);
      }
      onChange?.(e);
    };

    const handleClear = (e: React.MouseEvent) => {
      e.preventDefault();
      e.stopPropagation();
      if (!isControlled) {
        setInternalValue("");
      }
      onClear?.();
    };

    const sizeClasses = {
      sm:
        labelVariant === "inside"
          ? "min-h-[50px] px-3 py-1.5"
          : "h-9 px-3 py-1.5",
      md:
        labelVariant === "inside"
          ? "min-h-[58px] px-3.5 py-2"
          : "h-11 px-3.5 py-2",
      lg:
        labelVariant === "inside"
          ? "min-h-[66px] px-4 py-2.5"
          : "h-13 px-4 py-2.5",
    };

    return (
      <div className={cn("flex w-full flex-col font-sans", className)}>
        {/* Outside Label */}
        {label && labelVariant === "outside" && (
          <label
            htmlFor={id}
            className={cn(
              "mb-1.5 flex items-center gap-1 text-xs font-medium tracking-tight transition-colors select-none md:text-sm",
              isError
                ? "text-rose-600 dark:text-rose-400"
                : disabled
                  ? "text-neutral-400 dark:text-zinc-600"
                  : "text-neutral-800 dark:text-zinc-200"
            )}
          >
            <span>{label}</span>
            {required && (
              <span className="leading-none font-semibold text-rose-500 dark:text-rose-400">
                *
              </span>
            )}
          </label>
        )}

        {/* Main Input Container Box */}
        <div
          className={cn(
            "group/field relative flex w-full items-center rounded-xl border transition-all duration-200",
            sizeClasses[sizeVariant],
            // Resting state
            !isError &&
              !disabled &&
              "border-neutral-300/90 bg-white shadow-2xs hover:border-neutral-400 dark:border-zinc-800 dark:bg-zinc-900/90 dark:hover:border-zinc-700",
            // Focus state (Brand glow per design system)
            !isError &&
              !disabled &&
              "focus-within:border-brand dark:focus-within:border-brand focus-within:ring-brand/15 dark:focus-within:ring-brand/20 focus-within:ring-3",
            // Error state (Red/Rose tint per design system)
            isError &&
              !disabled &&
              "border-rose-400 bg-rose-50/60 text-rose-950 focus-within:border-rose-500 focus-within:ring-3 focus-within:ring-rose-500/20 dark:border-rose-700/80 dark:bg-rose-950/25 dark:text-rose-100",
            // Disabled state
            disabled &&
              "pointer-events-none cursor-not-allowed border-neutral-200 bg-neutral-100/80 opacity-60 dark:border-zinc-800/60 dark:bg-zinc-900/40"
          )}
        >
          {/* Leading / Prefix Icon */}
          {startIcon && (
            <div
              className={cn(
                "mr-2.5 flex shrink-0 items-center justify-center transition-colors",
                isError
                  ? "text-rose-500 dark:text-rose-400"
                  : disabled
                    ? "text-neutral-400 dark:text-zinc-600"
                    : "group-focus-within/field:text-brand dark:group-focus-within/field:text-brand text-neutral-500 dark:text-zinc-400"
              )}
            >
              {startIcon}
            </div>
          )}

          {/* Center Input / Embedded Label Content */}
          <div className="flex min-w-0 flex-1 flex-col justify-center">
            {/* Inside Label (Stacked at top inside box) */}
            {label && labelVariant === "inside" && (
              <label
                htmlFor={id}
                className={cn(
                  "mb-1 flex items-center gap-0.5 text-[11px] leading-none font-medium tracking-tight transition-colors select-none",
                  isError
                    ? "font-semibold text-rose-600 dark:text-rose-400"
                    : disabled
                      ? "text-neutral-400 dark:text-zinc-600"
                      : "group-focus-within/field:text-brand dark:group-focus-within/field:text-brand text-neutral-500 dark:text-zinc-400"
                )}
              >
                <span>{label}</span>
                {required && (
                  <span className="font-bold text-rose-500 dark:text-rose-400">
                    *
                  </span>
                )}
              </label>
            )}

            {/* Native HTML Input */}
            <input
              ref={ref}
              id={id}
              disabled={disabled}
              value={currentValue}
              onChange={handleChange}
              placeholder={placeholder}
              className={cn(
                "w-full border-none bg-transparent p-0 text-sm leading-normal font-normal shadow-none transition-colors outline-none md:text-[14px]",
                // Text colors
                isError
                  ? "text-rose-900 placeholder:text-rose-400/80 dark:text-rose-100 dark:placeholder:text-rose-400/60"
                  : disabled
                    ? "text-neutral-400 placeholder:text-neutral-400/60 dark:text-zinc-600 dark:placeholder:text-zinc-600"
                    : "text-neutral-900 placeholder:text-neutral-400 dark:text-zinc-100 dark:placeholder:text-zinc-500"
              )}
              {...props}
            />
          </div>

          {/* Suffix Actions & Clear Button Slot */}
          <div className="ml-2 flex shrink-0 items-center gap-1.5">
            {/* Quick Clear (x) Button */}
            {clearable && hasValue && !disabled && (
              <button
                type="button"
                onClick={handleClear}
                tabIndex={-1}
                aria-label="Clear input"
                className="flex h-4.5 w-4.5 cursor-pointer items-center justify-center rounded-full bg-neutral-800 text-white transition-all hover:opacity-80 active:scale-95 dark:bg-zinc-200 dark:text-zinc-900"
              >
                <svg
                  className="h-2.5 w-2.5 fill-none stroke-current"
                  viewBox="0 0 24 24"
                  strokeWidth="3"
                  strokeLinecap="round"
                  strokeLinejoin="round"
                >
                  <path d="M18 6 6 18" />
                  <path d="m6 6 12 12" />
                </svg>
              </button>
            )}

            {/* Custom End Icon */}
            {endIcon && !trailingValue && (
              <div
                className={cn(
                  "flex items-center justify-center text-neutral-400 dark:text-zinc-500",
                  isError && "text-rose-500 dark:text-rose-400"
                )}
              >
                {endIcon}
              </div>
            )}

            {/* Trailing Value with Divider & Chevron Down (Figma design system) */}
            {trailingValue && (
              <div className="flex items-center">
                {/* Vertical Divider */}
                <div
                  className={cn(
                    "mx-2.5 h-5 w-px transition-colors",
                    isError
                      ? "bg-rose-300 dark:bg-rose-800"
                      : "bg-neutral-200 dark:bg-zinc-800"
                  )}
                />

                {/* Trailing Value & Chevron */}
                <button
                  type="button"
                  disabled={disabled}
                  onClick={
                    typeof trailingValue === "object"
                      ? trailingValue.onClick
                      : undefined
                  }
                  className={cn(
                    "flex items-center gap-1.5 text-xs font-medium transition-colors select-none md:text-sm",
                    isError
                      ? "text-rose-800 dark:text-rose-300"
                      : disabled
                        ? "text-neutral-400 dark:text-zinc-600"
                        : "text-neutral-700 hover:text-neutral-900 dark:text-zinc-300 dark:hover:text-white"
                  )}
                >
                  <span>
                    {typeof trailingValue === "string"
                      ? trailingValue
                      : trailingValue.label}
                  </span>
                  <svg
                    className={cn(
                      "h-3.5 w-3.5 fill-none stroke-current transition-transform",
                      isError
                        ? "text-rose-600 dark:text-rose-400"
                        : "text-neutral-500 dark:text-zinc-400"
                    )}
                    viewBox="0 0 24 24"
                    strokeWidth="2"
                    strokeLinecap="round"
                    strokeLinejoin="round"
                  >
                    <path d="m6 9 6 6 6-6" />
                  </svg>
                </button>
              </div>
            )}
          </div>
        </div>

        {/* Supporting / Helper Text / Error Message */}
        {(supportingText || errorMessage) && (
          <p
            className={cn(
              "mt-1.5 text-xs font-normal tracking-tight transition-colors",
              isError
                ? "font-medium text-rose-600 dark:text-rose-400"
                : disabled
                  ? "text-neutral-400 dark:text-zinc-600"
                  : "text-neutral-500 dark:text-zinc-400"
            )}
          >
            {errorMessage || supportingText}
          </p>
        )}
      </div>
    );
  }
);

TextField.displayName = "TextField";

/** Primitive standalone Input for backward compatibility with base UI */
const Input = React.forwardRef<HTMLInputElement, React.ComponentProps<"input">>(
  ({ className, type, ...props }, ref) => {
    return (
      <input
        ref={ref}
        type={type}
        className={cn(
          "focus-visible:border-brand dark:focus-visible:border-brand focus-visible:ring-brand/15 dark:focus-visible:ring-brand/20 h-10 w-full min-w-0 rounded-xl border border-neutral-300/90 bg-white px-3.5 py-2 text-sm text-neutral-900 transition-all outline-none placeholder:text-neutral-400 focus-visible:ring-3 disabled:pointer-events-none disabled:cursor-not-allowed disabled:opacity-50 aria-invalid:border-rose-500 dark:border-zinc-800 dark:bg-zinc-900 dark:text-zinc-100 dark:placeholder:text-zinc-500 dark:aria-invalid:border-rose-500",
          className
        )}
        {...props}
      />
    );
  }
);

Input.displayName = "Input";

export { Input, TextField };
