"use client";

import * as React from "react";
import { ChevronUp, ChevronDown } from "lucide-react";
import { cn } from "@/lib/utils";

interface SidebarContextValue {
  collapsed: boolean;
}

const SidebarContext = React.createContext<SidebarContextValue>({
  collapsed: false,
});

export function useSidebar() {
  return React.useContext(SidebarContext);
}

export interface SidebarProps extends React.HTMLAttributes<HTMLElement> {
  collapsed?: boolean;
  children: React.ReactNode;
}

export function Sidebar({
  collapsed = false,
  className,
  children,
  ...props
}: SidebarProps) {
  return (
    <SidebarContext.Provider value={{ collapsed }}>
      <aside
        data-collapsed={collapsed}
        className={cn(
          "relative flex flex-col justify-between border-r border-neutral-200/80 bg-[#FAFAFA] transition-all duration-300 ease-in-out select-none dark:border-zinc-800/80 dark:bg-[#0C0C0E]",
          collapsed ? "w-18" : "w-64",
          className
        )}
        {...props}
      >
        {children}
      </aside>
    </SidebarContext.Provider>
  );
}

export interface SidebarHeaderProps extends React.HTMLAttributes<HTMLDivElement> {
  children: React.ReactNode;
}

export function SidebarHeader({
  className,
  children,
  ...props
}: SidebarHeaderProps) {
  const { collapsed } = useSidebar();
  return (
    <div
      className={cn(
        "mb-6 flex items-center gap-2",
        collapsed ? "justify-center" : "justify-between px-2",
        className
      )}
      {...props}
    >
      {children}
    </div>
  );
}

export interface SidebarContentProps extends React.HTMLAttributes<HTMLDivElement> {
  children: React.ReactNode;
}

export function SidebarContent({
  className,
  children,
  ...props
}: SidebarContentProps) {
  return (
    <div
      className={cn(
        "flex min-h-0 flex-1 scrollbar-none flex-col overflow-y-auto px-3 py-4",
        className
      )}
      {...props}
    >
      {children}
    </div>
  );
}

export interface SidebarFooterProps extends React.HTMLAttributes<HTMLDivElement> {
  children: React.ReactNode;
}

export function SidebarFooter({
  className,
  children,
  ...props
}: SidebarFooterProps) {
  return (
    <div
      className={cn(
        "space-y-3 border-t border-neutral-200/70 p-3 dark:border-zinc-800/70",
        className
      )}
      {...props}
    >
      {children}
    </div>
  );
}

/* ==========================================================================
   Sidebar Navigation & Menu Items
   ========================================================================== */

export interface SidebarNavProps extends React.HTMLAttributes<HTMLElement> {
  children: React.ReactNode;
}

export function SidebarNav({ className, children, ...props }: SidebarNavProps) {
  return (
    <nav className={cn("space-y-1", className)} {...props}>
      {children}
    </nav>
  );
}

export interface SidebarNavItemProps {
  asChild?: boolean;
  active?: boolean;
  icon?: React.ComponentType<{ className?: string }>;
  title: string;
  badge?: React.ReactNode;
  children?: React.ReactNode;
  className?: string;
  onClick?: () => void;
}

export function SidebarNavItem({
  active,
  icon: Icon,
  title,
  badge,
  className,
  children,
  ...props
}: SidebarNavItemProps) {
  const { collapsed } = useSidebar();

  const content = (
    <>
      {Icon && (
        <Icon
          className={cn(
            "size-4.5 shrink-0 transition-colors",
            active
              ? "text-brand dark:text-brand-hover stroke-[2.2]"
              : "stroke-[1.8] text-neutral-500 group-hover:text-neutral-800 dark:text-zinc-400 dark:group-hover:text-zinc-200"
          )}
        />
      )}
      {!collapsed && (
        <span className="flex-1 truncate text-left tracking-tight">
          {title}
        </span>
      )}
      {!collapsed && badge && <span className="shrink-0">{badge}</span>}
    </>
  );

  return (
    <div
      title={collapsed ? title : undefined}
      className={cn(
        "group flex cursor-pointer items-center rounded-xl text-sm font-medium transition-all duration-200",
        collapsed ? "mx-auto size-10 justify-center" : "gap-3 px-3 py-2.5",
        active
          ? "bg-brand/10 text-brand dark:bg-brand/15 dark:text-brand-hover border-brand/20 dark:border-brand/30 border font-semibold shadow-2xs"
          : "text-neutral-600 hover:bg-neutral-200/50 hover:text-neutral-900 dark:text-zinc-400 dark:hover:bg-zinc-800/40 dark:hover:text-zinc-100",
        className
      )}
      {...props}
    >
      {children || content}
    </div>
  );
}

/* ==========================================================================
   Sidebar Accordion / Collapsible Group
   ========================================================================== */

export interface SidebarAccordionProps {
  title: string;
  defaultOpen?: boolean;
  isOpen?: boolean;
  onToggle?: () => void;
  children: React.ReactNode;
  className?: string;
}

export function SidebarAccordion({
  title,
  defaultOpen = true,
  isOpen,
  onToggle,
  children,
  className,
}: SidebarAccordionProps) {
  const { collapsed } = useSidebar();
  const [internalOpen, setInternalOpen] = React.useState(defaultOpen);

  const open = isOpen !== undefined ? isOpen : internalOpen;

  const handleToggle = () => {
    if (onToggle) {
      onToggle();
    } else {
      setInternalOpen(!open);
    }
  };

  if (collapsed) return null;

  return (
    <div className={cn("mt-4", className)}>
      <button
        type="button"
        onClick={handleToggle}
        className="flex w-full cursor-pointer items-center justify-between px-3 py-1.5 text-xs font-semibold tracking-wider text-neutral-500 uppercase transition-colors hover:text-neutral-800 dark:text-zinc-400 dark:hover:text-zinc-200"
      >
        <span>{title}</span>
        {open ? (
          <ChevronUp className="size-3.5" />
        ) : (
          <ChevronDown className="size-3.5" />
        )}
      </button>

      {open && <div className="mt-1 space-y-1">{children}</div>}
    </div>
  );
}

/* ==========================================================================
   Sidebar User Profile Card Component
   ========================================================================== */

export interface SidebarUserProfileProps {
  displayName: string;
  role?: string;
  avatarInitial?: string;
  avatarSrc?: string;
  actionSlot?: React.ReactNode;
  className?: string;
}

export function SidebarUserProfile({
  displayName,
  role,
  avatarInitial,
  actionSlot,
  className,
}: SidebarUserProfileProps) {
  const { collapsed } = useSidebar();
  const initial =
    avatarInitial || (displayName ? displayName.charAt(0).toUpperCase() : "U");

  return (
    <div
      className={cn(
        "flex items-center rounded-2xl border border-neutral-200/80 bg-white shadow-2xs transition-all dark:border-zinc-800 dark:bg-zinc-900/90",
        collapsed ? "justify-center p-1.5" : "justify-between p-2.5",
        className
      )}
    >
      <div className="flex min-w-0 items-center gap-2.5">
        <div className="bg-brand text-brand-foreground flex size-9 shrink-0 items-center justify-center rounded-full text-xs font-bold shadow-xs">
          {initial}
        </div>
        {!collapsed && (
          <div className="min-w-0">
            <p className="truncate text-xs font-bold text-neutral-900 dark:text-white">
              {displayName}
            </p>
            {role && (
              <p className="truncate text-[11px] font-medium text-neutral-500 dark:text-zinc-400">
                {role}
              </p>
            )}
          </div>
        )}
      </div>

      {!collapsed && actionSlot && <div className="shrink-0">{actionSlot}</div>}
    </div>
  );
}

/* ==========================================================================
   Sidebar Upgrade Card Component
   ========================================================================== */

export interface SidebarUpgradeCardProps {
  title: string;
  description: string;
  buttonText: string;
  icon?: React.ReactNode;
  onUpgrade?: () => void;
  className?: string;
}

export function SidebarUpgradeCard({
  title,
  description,
  buttonText,
  icon,
  onUpgrade,
  className,
}: SidebarUpgradeCardProps) {
  const { collapsed } = useSidebar();

  if (collapsed) return null;

  return (
    <div
      className={cn(
        "rounded-2xl border border-neutral-200/80 bg-white p-4 shadow-2xs dark:border-zinc-800 dark:bg-zinc-900/90",
        className
      )}
    >
      <div className="flex items-center gap-1.5">
        {icon}
        <h4 className="text-xs font-bold text-neutral-900 dark:text-white">
          {title}
        </h4>
      </div>
      <p className="mt-1 text-[11px] leading-tight text-neutral-500 dark:text-zinc-400">
        {description}
      </p>
      <button
        type="button"
        onClick={onUpgrade}
        className="bg-brand text-brand-foreground hover:bg-brand-hover mt-3 w-full cursor-pointer rounded-xl px-3 py-2 text-xs font-semibold shadow-md transition-all hover:shadow-lg active:scale-[0.98]"
      >
        {buttonText}
      </button>
    </div>
  );
}
