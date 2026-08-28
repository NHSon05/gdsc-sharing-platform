import * as React from "react";
import { cva, type VariantProps } from "class-variance-authority";

import { cn } from "@/lib/utils";

const cardVariants = cva(
  "group/card flex flex-col gap-(--card-spacing) overflow-hidden rounded-2xl text-sm transition-all duration-300 [--card-spacing:--spacing(5)] has-data-[slot=card-footer]:pb-0 has-[>img:first-child]:pt-0 data-[size=sm]:[--card-spacing:--spacing(3.5)] data-[size=sm]:has-data-[slot=card-footer]:pb-0 *:[img:first-child]:rounded-t-2xl *:[img:last-child]:rounded-b-2xl",
  {
    variants: {
      variant: {
        default:
          "bg-card text-card-foreground ring-1 ring-foreground/10 shadow-xs",
        outline:
          "bg-transparent text-card-foreground border border-neutral-200 dark:border-zinc-800 shadow-none",
        glass:
          "bg-white/70 dark:bg-zinc-900/60 backdrop-blur-md border border-neutral-200/80 dark:border-zinc-800/80 text-card-foreground shadow-[0_8px_30px_rgb(0,0,0,0.04)] dark:shadow-[0_8px_30px_rgb(0,0,0,0.3)]",
        liquid_glass:
          "bg-white/75 dark:bg-zinc-900/65 backdrop-blur-xl border border-white/90 dark:border-white/10 text-card-foreground shadow-[0_15px_35px_-10px_rgba(0,0,0,0.06),inset_0_1px_0_rgba(255,255,255,0.85)] dark:shadow-[0_15px_35px_-10px_rgba(0,0,0,0.5),inset_0_1px_0_rgba(255,255,255,0.12)] ring-0",
        "liquid-glass":
          "bg-white/75 dark:bg-zinc-900/65 backdrop-blur-xl border border-white/90 dark:border-white/10 text-card-foreground shadow-[0_15px_35px_-10px_rgba(0,0,0,0.06),inset_0_1px_0_rgba(255,255,255,0.85)] dark:shadow-[0_15px_35px_-10px_rgba(0,0,0,0.5),inset_0_1px_0_rgba(255,255,255,0.12)] ring-0",
      },
      size: {
        default: "",
        sm: "rounded-xl *:[img:first-child]:rounded-t-xl *:[img:last-child]:rounded-b-xl",
      },
    },
    defaultVariants: {
      variant: "default",
      size: "default",
    },
  }
);

export interface CardProps
  extends React.ComponentProps<"div">, VariantProps<typeof cardVariants> {}

function Card({
  className,
  variant = "default",
  size = "default",
  ...props
}: CardProps) {
  return (
    <div
      data-slot="card"
      data-size={size}
      data-variant={variant}
      className={cn(cardVariants({ variant, size }), className)}
      {...props}
    />
  );
}

function CardHeader({ className, ...props }: React.ComponentProps<"div">) {
  return (
    <div
      data-slot="card-header"
      className={cn(
        "group/card-header @container/card-header grid auto-rows-min items-start gap-1.5 px-(--card-spacing) has-data-[slot=card-action]:grid-cols-[1fr_auto] has-data-[slot=card-description]:grid-rows-[auto_auto] [.border-b]:pb-(--card-spacing)",
        className
      )}
      {...props}
    />
  );
}

function CardTitle({ className, ...props }: React.ComponentProps<"div">) {
  return (
    <div
      data-slot="card-title"
      className={cn(
        "font-heading text-base leading-snug font-semibold tracking-tight text-neutral-900 group-data-[size=sm]/card:text-sm md:text-lg dark:text-zinc-50",
        className
      )}
      {...props}
    />
  );
}

function CardDescription({ className, ...props }: React.ComponentProps<"div">) {
  return (
    <div
      data-slot="card-description"
      className={cn(
        "text-sm leading-relaxed text-neutral-600 dark:text-zinc-400",
        className
      )}
      {...props}
    />
  );
}

function CardAction({ className, ...props }: React.ComponentProps<"div">) {
  return (
    <div
      data-slot="card-action"
      className={cn(
        "col-start-2 row-span-2 row-start-1 self-start justify-self-end",
        className
      )}
      {...props}
    />
  );
}

function CardContent({ className, ...props }: React.ComponentProps<"div">) {
  return (
    <div
      data-slot="card-content"
      className={cn("px-(--card-spacing)", className)}
      {...props}
    />
  );
}

function CardFooter({ className, ...props }: React.ComponentProps<"div">) {
  return (
    <div
      data-slot="card-footer"
      className={cn(
        "flex items-center border-t p-(--card-spacing)",
        // Liquid glass footer styling
        "group-data-[variant=liquid-glass]/card:border-white/60 group-data-[variant=liquid-glass]/card:bg-white/40 group-data-[variant=liquid-glass]/card:dark:border-white/5 group-data-[variant=liquid-glass]/card:dark:bg-zinc-950/40",
        // Glass footer styling
        "group-data-[variant=glass]/card:border-neutral-200/60 group-data-[variant=glass]/card:bg-white/30 group-data-[variant=glass]/card:dark:border-zinc-800/60 group-data-[variant=glass]/card:dark:bg-zinc-900/30",
        // Default footer styling
        "group-data-[variant=default]/card:bg-muted/50 group-data-[variant=default]/card:border-border",
        // Outline footer styling
        "group-data-[variant=outline]/card:border-neutral-200 group-data-[variant=outline]/card:dark:border-zinc-800",
        className
      )}
      {...props}
    />
  );
}

export {
  Card,
  CardHeader,
  CardFooter,
  CardTitle,
  CardAction,
  CardDescription,
  CardContent,
  cardVariants,
};
