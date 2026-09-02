import type { ComponentType } from "react";
import type { TranslationKey } from "@/core/i18n/i18n.context";
import {
  Home,
  Map,
  Calendar,
  Users,
  GraduationCap,
  Search,
  History,
  Star,
  LayoutGrid,
  ArrowLeftRight,
  Boxes,
  Layers,
} from "lucide-react";

export interface SidebarNavItemConfig {
  titleKey: TranslationKey;
  href: string;
  icon: ComponentType<{ className?: string }>;
  exact?: boolean;
}

export const MAIN_NAV_ITEMS: SidebarNavItemConfig[] = [
  {
    titleKey: "sidebar.home",
    href: "/",
    icon: Home,
    exact: true,
  },
  {
    titleKey: "sidebar.roadmaps",
    href: "/roadmaps",
    icon: Map,
  },
  {
    titleKey: "sidebar.schedule",
    href: "/schedule",
    icon: Calendar,
  },
  {
    titleKey: "sidebar.memberDirectory",
    href: "/dashboard/users",
    icon: Users,
  },
  {
    titleKey: "sidebar.learning",
    href: "#learning",
    icon: GraduationCap,
  },
];

export const UTILITY_NAV_ITEMS: SidebarNavItemConfig[] = [
  {
    titleKey: "sidebar.search",
    href: "#search",
    icon: Search,
  },
  {
    titleKey: "sidebar.history",
    href: "#history",
    icon: History,
  },
  {
    titleKey: "sidebar.favorites",
    href: "#favorites",
    icon: Star,
  },
];

export const CATEGORY_NAV_ITEMS: SidebarNavItemConfig[] = [
  {
    titleKey: "sidebar.tags",
    href: "#tags",
    icon: LayoutGrid,
  },
  {
    titleKey: "sidebar.recentUpdates",
    href: "#updates",
    icon: ArrowLeftRight,
  },
];

export const MORE_NAV_ITEMS: SidebarNavItemConfig[] = [
  {
    titleKey: "sidebar.mcpServer",
    href: "#mcp",
    icon: Boxes,
  },
  {
    titleKey: "sidebar.openClawSkill",
    href: "#knowledge",
    icon: Layers,
  },
];

/**
 * Helper function to retrieve translated sidebar navigation items.
 */
export function getSidebarNavItems(t: (key: TranslationKey) => string) {
  return {
    mainNavItems: MAIN_NAV_ITEMS.map((item) => ({
      ...item,
      title: t(item.titleKey),
    })),
    utilityNavItems: UTILITY_NAV_ITEMS.map((item) => ({
      ...item,
      title: t(item.titleKey),
    })),
    categoryItems: CATEGORY_NAV_ITEMS.map((item) => ({
      ...item,
      title: t(item.titleKey),
    })),
    moreItems: MORE_NAV_ITEMS.map((item) => ({
      ...item,
      title: t(item.titleKey),
    })),
  };
}
