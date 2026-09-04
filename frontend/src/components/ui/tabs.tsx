"use client";

import * as React from "react";
import { cn } from "@/lib/utils";

interface TabsContextValue {
  value: string;
  onValueChange: (value: string) => void;
}

const TabsContext = React.createContext<TabsContextValue | null>(null);

export function useTabs() {
  const context = React.useContext(TabsContext);
  if (!context) {
    throw new Error("useTabs must be used within a <Tabs />");
  }
  return context;
}

export interface TabsProps extends React.HTMLAttributes<HTMLDivElement> {
  value?: string;
  defaultValue?: string;
  onValueChange?: (value: string) => void;
  children: React.ReactNode;
}

export function Tabs({
  value: controlledValue,
  defaultValue = "",
  onValueChange: controlledOnValueChange,
  className,
  children,
  ...props
}: TabsProps) {
  const [uncontrolledValue, setUncontrolledValue] =
    React.useState(defaultValue);

  const value =
    controlledValue !== undefined ? controlledValue : uncontrolledValue;
  const onValueChange = controlledOnOpenChangeOrSet(
    controlledOnValueChange,
    setUncontrolledValue
  );

  return (
    <TabsContext.Provider value={{ value, onValueChange }}>
      <div className={cn("w-full space-y-4", className)} {...props}>
        {children}
      </div>
    </TabsContext.Provider>
  );
}

function controlledOnOpenChangeOrSet(
  controlled?: (val: string) => void,
  uncontrolled?: React.Dispatch<React.SetStateAction<string>>
) {
  return (val: string) => {
    controlled?.(val);
    uncontrolled?.(val);
  };
}

export function TabsList({
  className,
  children,
  ...props
}: React.HTMLAttributes<HTMLDivElement>) {
  return (
    <div
      role="tablist"
      className={cn(
        "inline-flex items-center gap-1.5 rounded-2xl border border-neutral-200/80 bg-neutral-100/80 p-1 select-none dark:border-zinc-800 dark:bg-zinc-900/90",
        className
      )}
      {...props}
    >
      {children}
    </div>
  );
}

export interface TabsTriggerProps extends React.ButtonHTMLAttributes<HTMLButtonElement> {
  value: string;
  children: React.ReactNode;
  icon?: React.ReactNode;
}

export function TabsTrigger({
  value,
  children,
  icon,
  className,
  ...props
}: TabsTriggerProps) {
  const { value: activeValue, onValueChange } = useTabs();
  const isActive = activeValue === value;

  return (
    <button
      type="button"
      role="tab"
      aria-selected={isActive}
      onClick={() => onValueChange(value)}
      className={cn(
        "inline-flex cursor-pointer items-center justify-center gap-2 rounded-xl px-4 py-2 text-xs font-semibold tracking-tight transition-all duration-150 sm:text-sm",
        isActive
          ? "text-brand bg-white font-bold shadow-xs dark:bg-zinc-800 dark:text-white"
          : "text-neutral-500 hover:text-neutral-900 dark:text-zinc-400 dark:hover:text-zinc-100",
        className
      )}
      {...props}
    >
      {icon && <span className="size-4">{icon}</span>}
      <span>{children}</span>
    </button>
  );
}

export interface TabsContentProps extends React.HTMLAttributes<HTMLDivElement> {
  value: string;
  children: React.ReactNode;
}

export function TabsContent({
  value,
  children,
  className,
  ...props
}: TabsContentProps) {
  const { value: activeValue } = useTabs();
  if (activeValue !== value) return null;

  return (
    <div
      role="tabpanel"
      className={cn(
        "animate-in fade-in-50 duration-200 focus-visible:outline-hidden",
        className
      )}
      {...props}
    >
      {children}
    </div>
  );
}
