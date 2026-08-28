import * as React from "react";
import { cva, type VariantProps } from "class-variance-authority";

import { cn } from "@/lib/utils";

const buttonVariants = cva(
  "group/button relative inline-flex shrink-0 items-center justify-center font-sans font-medium whitespace-nowrap rounded-full transition-all duration-200 outline-none select-none cursor-pointer disabled:pointer-events-none disabled:cursor-not-allowed [&_svg]:pointer-events-none [&_svg]:shrink-0 active:scale-[0.98]",
  {
    variants: {
      variant: {
        // 1. Solid Dark / Primary (Image 1 - Col 1)
        primary:
          "bg-zinc-950 text-white hover:bg-zinc-800 active:bg-black focus-visible:ring-2 focus-visible:ring-offset-2 focus-visible:ring-zinc-950 dark:bg-zinc-100 dark:text-zinc-950 dark:hover:bg-white dark:active:bg-zinc-200 dark:focus-visible:ring-zinc-100 disabled:bg-zinc-100 disabled:text-zinc-400 dark:disabled:bg-zinc-900 dark:disabled:text-zinc-600",

        // Default alias to primary
        default:
          "bg-zinc-950 text-white hover:bg-zinc-800 active:bg-black focus-visible:ring-2 focus-visible:ring-offset-2 focus-visible:ring-zinc-950 dark:bg-zinc-100 dark:text-zinc-950 dark:hover:bg-white dark:active:bg-zinc-200 dark:focus-visible:ring-zinc-100 disabled:bg-zinc-100 disabled:text-zinc-400 dark:disabled:bg-zinc-900 dark:disabled:text-zinc-600",

        // 2. Secondary Outline Light (Image 1 - Col 2)
        outline:
          "border border-zinc-200/90 bg-white text-zinc-900 hover:bg-zinc-50 hover:border-zinc-300 active:bg-zinc-100 focus-visible:ring-2 focus-visible:ring-offset-2 focus-visible:ring-zinc-950 dark:border-zinc-800 dark:bg-zinc-950 dark:text-zinc-100 dark:hover:bg-zinc-900 dark:hover:border-zinc-700 dark:active:bg-zinc-800 dark:focus-visible:ring-zinc-100 disabled:border-zinc-100 disabled:bg-transparent disabled:text-zinc-300 dark:disabled:border-zinc-900 dark:disabled:text-zinc-700",

        // 3. Subtle / Muted Gray (Image 1 - Col 3)
        subtle:
          "bg-zinc-100 text-zinc-800 hover:bg-zinc-200/80 hover:text-zinc-950 active:bg-zinc-200 focus-visible:ring-2 focus-visible:ring-offset-2 focus-visible:ring-zinc-950 dark:bg-zinc-900 dark:text-zinc-200 dark:hover:bg-zinc-800 dark:hover:text-white dark:active:bg-zinc-800/90 dark:focus-visible:ring-zinc-100 disabled:bg-zinc-100/50 disabled:text-zinc-400 dark:disabled:bg-zinc-900/50 dark:disabled:text-zinc-600",

        // 4. Vibrant Brand Blue (#4285F4 GDSC Primary)
        brand:
          "bg-brand text-brand-foreground hover:bg-brand-hover active:bg-blue-700 shadow-[0_4px_14px_-2px_var(--brand-glow)] hover:shadow-[0_6px_20px_-2px_var(--brand-glow)] focus-visible:ring-2 focus-visible:ring-offset-2 focus-visible:ring-brand disabled:bg-brand/40 disabled:text-white/60 disabled:shadow-none",

        // 5. Elevated Soft Drop Shadow Glass (Image 3)
        elevated:
          "border border-zinc-100/80 bg-white text-zinc-900 shadow-[0_8px_24px_-4px_rgba(0,0,0,0.12),0_2px_6px_-1px_rgba(0,0,0,0.06)] hover:shadow-[0_12px_30px_-4px_rgba(0,0,0,0.16)] hover:-translate-y-0.5 active:translate-y-0 active:shadow-[0_4px_12px_-2px_rgba(0,0,0,0.1)] focus-visible:ring-2 focus-visible:ring-offset-2 focus-visible:ring-zinc-950 dark:border-zinc-800/80 dark:bg-zinc-900 dark:text-zinc-100 dark:shadow-[0_8px_24px_-4px_rgba(0,0,0,0.5),0_2px_6px_-1px_rgba(0,0,0,0.3)] dark:hover:shadow-[0_12px_30px_-4px_rgba(0,0,0,0.65)] dark:focus-visible:ring-zinc-100 disabled:shadow-none disabled:border-transparent disabled:bg-zinc-100/60 disabled:text-zinc-300 dark:disabled:bg-zinc-900/60 dark:disabled:text-zinc-700",

        // 6. Ghost (Transparent)
        ghost:
          "text-zinc-700 hover:bg-zinc-100 hover:text-zinc-900 active:bg-zinc-200 focus-visible:ring-2 focus-visible:ring-offset-2 focus-visible:ring-zinc-950 dark:text-zinc-300 dark:hover:bg-zinc-900 dark:hover:text-white dark:active:bg-zinc-800 dark:focus-visible:ring-zinc-100 disabled:text-zinc-300 dark:disabled:text-zinc-700",

        // 7. Destructive Red
        destructive:
          "bg-red-500 text-white hover:bg-red-600 active:bg-red-700 focus-visible:ring-2 focus-visible:ring-offset-2 focus-visible:ring-red-500 disabled:bg-red-200 disabled:text-red-50 dark:bg-red-600 dark:hover:bg-red-500 dark:disabled:bg-red-950/60 dark:disabled:text-red-700",
      },
      size: {
        // Large: 48px height (Image 1 Top Row)
        lg: "h-12 px-6 text-[15px] gap-2.5 [&_svg:not([class*='size-'])]:size-5",

        // Medium / Default: 40px height (Image 1 Middle Row)
        default:
          "h-10 px-5 text-sm gap-2 [&_svg:not([class*='size-'])]:size-4.5",
        md: "h-10 px-5 text-sm gap-2 [&_svg:not([class*='size-'])]:size-4.5",

        // Small: 32px height (Image 1 Bottom Row)
        sm: "h-8 px-3.5 text-xs gap-1.5 [&_svg:not([class*='size-'])]:size-3.5",

        // Icon-only buttons (Image 2)
        "icon-lg": "size-12 p-0 [&_svg:not([class*='size-'])]:size-5",
        icon: "size-10 p-0 [&_svg:not([class*='size-'])]:size-4.5",
        "icon-md": "size-10 p-0 [&_svg:not([class*='size-'])]:size-4.5",
        "icon-sm": "size-8 p-0 [&_svg:not([class*='size-'])]:size-3.5",
      },
    },
    defaultVariants: {
      variant: "default",
      size: "default",
    },
  }
);

export interface ButtonProps
  extends
    React.ButtonHTMLAttributes<HTMLButtonElement>,
    VariantProps<typeof buttonVariants> {
  asChild?: boolean;
  leftIcon?: React.ReactNode;
  rightIcon?: React.ReactNode;
  loading?: boolean;
}

const Button = React.forwardRef<HTMLButtonElement, ButtonProps>(
  (
    {
      className,
      variant = "default",
      size = "default",
      leftIcon,
      rightIcon,
      loading = false,
      children,
      disabled,
      ...props
    },
    ref
  ) => {
    return (
      <button
        ref={ref}
        data-slot="button"
        disabled={disabled || loading}
        className={cn(buttonVariants({ variant, size, className }))}
        {...props}
      >
        {loading && (
          <svg
            className="mr-2 -ml-1 size-4 animate-spin text-current"
            xmlns="http://www.w3.org/2000/svg"
            fill="none"
            viewBox="0 0 24 24"
          >
            <circle
              className="opacity-25"
              cx="12"
              cy="12"
              r="10"
              stroke="currentColor"
              strokeWidth="4"
            />
            <path
              className="opacity-75"
              fill="currentColor"
              d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"
            />
          </svg>
        )}
        {!loading && leftIcon && (
          <span className="inline-flex shrink-0">{leftIcon}</span>
        )}
        {children && <span>{children}</span>}
        {!loading && rightIcon && (
          <span className="inline-flex shrink-0">{rightIcon}</span>
        )}
      </button>
    );
  }
);

Button.displayName = "Button";

export { Button, buttonVariants };
