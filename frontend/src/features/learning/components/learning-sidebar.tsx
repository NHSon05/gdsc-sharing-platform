"use client";

import React, { useState, useMemo } from "react";
import {
  Search,
  ChevronLeft,
  ChevronRight,
  Monitor,
  Server,
  Cloud,
  Cpu,
  Database,
  Briefcase,
  Layers,
  Zap,
  Wrench,
  Globe,
  Shield,
  Smartphone,
  GitBranch,
  Terminal,
  Network,
  Users,
  Box,
} from "lucide-react";
import StackIcon, { type IconName } from "tech-stack-icons";
import { LEARNING_CATEGORY_GROUPS } from "../config/learning-categories.config";
import { useTranslation } from "@/core/i18n/i18n.context";

export interface LearningSidebarProps {
  activeCategory: string;
  onSelectCategory: (category: {
    department?: string;
    topic?: string;
    label: string;
  }) => void;
  totalCount?: number;
  activeCategoryCount?: number;
  className?: string;
}

const STACK_ICON_MAP: Record<string, IconName> = {
  // Roles / Department items
  "dept-frontend": "react",
  "dept-backend": "nodejs",
  "dept-data-and-messaging": "postgresql",
  "dept-devops": "docker",
  "dept-mobile": "flutter",

  // Frontend topics
  "topic-html": "html5",
  "topic-css": "css3",
  "topic-javascript": "js",
  "topic-typescript": "typescript",
  "topic-react": "react",
  "topic-nextjs": "nextjs",
  "topic-vue": "vuejs",
  "topic-angular": "angular",

  // Backend topics
  "topic-backend-api": "nodejs",
  "topic-nodejs": "nodejs",
  "topic-nestjs": "nestjs",
  "topic-python": "python",
  "topic-django": "django",
  "topic-java": "java",
  "topic-spring-boot": "spring",
  "topic-golang": "go",
  "topic-csharp": "c#",
  "topic-cpp": "c++",
  "topic-php": "php",
  "topic-laravel": "laravel",
  "topic-ruby": "ruby",
  "topic-rails": "rails",
  "topic-graphql": "graphql",

  // Database topics
  "topic-database": "postgresql",
  "topic-postgresql": "postgresql",
  "topic-mongodb": "mongodb",
  "topic-redis": "redis",
  "topic-elasticsearch": "elastic",
  "topic-rabbitmq": "rabbitmq",

  // DevOps topics
  "topic-docker-k8s": "docker",
  "topic-cloud-aws": "aws",
  "topic-terraform": "terraform",
  "topic-git": "git",

  // Mobile topics
  "topic-flutter": "flutter",
  "topic-react-native": "reactnative",
  "topic-android": "android",
};

function getCategoryIcon(id: string, label: string) {
  // Check direct stack icon map from stack.md first
  const stackName = STACK_ICON_MAP[id];
  if (stackName) {
    return <StackIcon name={stackName} className="size-4.5" />;
  }

  const norm = label.toLowerCase();
  if (norm.includes("frontend"))
    return <Monitor className="size-4 text-blue-500" />;
  if (norm.includes("backend"))
    return <Server className="size-4 text-indigo-500" />;
  if (norm.includes("devops") || norm.includes("cloud"))
    return <Cloud className="size-4 text-sky-500" />;
  if (norm.includes("ai") || norm.includes("machine learning"))
    return <Cpu className="size-4 text-pink-500" />;
  if (
    norm.includes("database") ||
    norm.includes("postgres") ||
    norm.includes("mongo")
  )
    return <Database className="size-4 text-emerald-500" />;
  if (
    norm.includes("mobile") ||
    norm.includes("android") ||
    norm.includes("flutter") ||
    norm.includes("react native")
  )
    return <Smartphone className="size-4 text-purple-500" />;
  if (norm.includes("system design") || norm.includes("architecture"))
    return <Box className="size-4 text-orange-500" />;
  if (
    norm.includes("security") ||
    norm.includes("bảo mật") ||
    norm.includes("testing")
  )
    return <Shield className="size-4 text-rose-500" />;
  if (norm.includes("ba") || norm.includes("business"))
    return <Briefcase className="size-4 text-teal-500" />;
  if (
    norm.includes("hr") ||
    norm.includes("hành vi") ||
    norm.includes("career")
  )
    return <Users className="size-4 text-amber-500" />;
  if (norm.includes("mạng") || norm.includes("network"))
    return <Network className="size-4 text-cyan-500" />;
  if (
    norm.includes("linux") ||
    norm.includes("terminal") ||
    norm.includes("operating system")
  )
    return <Terminal className="size-4 text-zinc-500" />;
  if (norm.includes("git"))
    return <GitBranch className="size-4 text-orange-600" />;

  switch (norm) {
    case "state management":
      return <Layers className="size-4 text-purple-500" />;
    case "micro-frontend":
      return <Layers className="size-4 text-blue-400" />;
    case "performance":
      return <Zap className="size-4 text-amber-500" />;
    case "build tools":
      return <Wrench className="size-4 text-neutral-500" />;
    case "seo":
      return <Globe className="size-4 text-indigo-400" />;
    case "fastapi":
      return <span className="text-md font-black text-teal-500">⚡</span>;
    case "kafka":
      return <span className="text-md font-black text-neutral-800 dark:text-neutral-200">K</span>;
    case "cicd":
      return <Zap className="size-4 text-indigo-500" />;
    default:
      return <span className="size-2 rounded-full bg-neutral-400" />;
  }
}

export function LearningSidebar({
  activeCategory,
  onSelectCategory,
  totalCount,
  activeCategoryCount,
  className = "",
}: LearningSidebarProps) {
  const [collapsed, setCollapsed] = useState(false);
  const [filterText, setFilterText] = useState("");
  const { t, locale } = useTranslation();

  const isEn = locale === "en";

  // Filter groups according to filter input (checking both VI and EN labels)
  const filteredGroups = useMemo(() => {
    if (!filterText.trim()) return LEARNING_CATEGORY_GROUPS;
    const query = filterText.toLowerCase();

    return LEARNING_CATEGORY_GROUPS.map((group) => {
      const groupTitle = isEn && group.titleEn ? group.titleEn : group.title;
      const titleMatches =
        groupTitle.toLowerCase().includes(query) ||
        group.title.toLowerCase().includes(query);
      const filteredItems = group.items.filter((item) => {
        const itemLabel = isEn && item.labelEn ? item.labelEn : item.label;
        return (
          itemLabel.toLowerCase().includes(query) ||
          item.label.toLowerCase().includes(query) ||
          (item.labelEn && item.labelEn.toLowerCase().includes(query))
        );
      });

      if (titleMatches) return group;
      if (filteredItems.length > 0) {
        return { ...group, items: filteredItems };
      }
      return null;
    }).filter(Boolean) as typeof LEARNING_CATEGORY_GROUPS;
  }, [filterText, isEn]);

  return (
    <aside
      className={`relative flex flex-col border-r border-neutral-200/80 bg-white transition-all duration-300 dark:border-zinc-800/80 dark:bg-[#0C0C0E] ${
        collapsed ? "w-14" : "w-72 sm:w-80"
      } ${className}`}
    >
      {/* Collapse Toggle Button on Border */}
      <button
        type="button"
        onClick={() => setCollapsed(!collapsed)}
        className="absolute top-6 -right-3.5 z-20 flex size-8 cursor-pointer items-center justify-center rounded-full border border-neutral-200 bg-white text-neutral-800 shadow-xs transition-colors hover:bg-neutral-50 dark:border-zinc-700 dark:bg-zinc-800 dark:text-zinc-300 dark:hover:bg-zinc-700"
        title={
          collapsed
            ? isEn
              ? "Expand categories"
              : "Mở rộng danh mục"
            : isEn
              ? "Collapse categories"
              : "Thu gọn danh mục"
        }
      >
        {collapsed ? (
          <ChevronRight className="size-4" />
        ) : (
          <ChevronLeft className="size-4" />
        )}
      </button>

      {!collapsed ? (
        <div className="flex h-full flex-col p-4">
          {/* Master "All" Button */}
          <button
            type="button"
            onClick={() => onSelectCategory({ label: isEn ? "All" : "Tất Cả" })}
            className={`flex w-full cursor-pointer items-center justify-between rounded-xl px-4 py-2 text-sm font-bold transition-all ${
              activeCategory === "all" || !activeCategory
                ? "border border-blue-500/30 bg-blue-100 text-blue-600 shadow-2xs dark:border-blue-500/30 dark:bg-blue-950/40 dark:text-blue-400"
                : "text-neutral-800 hover:bg-neutral-100 dark:text-zinc-300 dark:hover:bg-zinc-900"
            }`}
          >
            <span>{t("learning.allCategories")}</span>
            {totalCount !== undefined && (
              <span
                className={`rounded-full px-2 py-1 text-xs font-semibold ${
                  activeCategory === "all" || !activeCategory
                    ? "bg-blue-600 text-white"
                    : "bg-neutral-200/80 text-neutral-600 dark:bg-zinc-800 dark:text-zinc-400"
                }`}
              >
                {totalCount}
              </span>
            )}
          </button>

          {/* Search within Categories */}
          <div className="relative mt-3">
            <Search className="absolute top-1/2 left-3 size-3.5 -translate-y-1/2 text-neutral-400 dark:text-zinc-500" />
            <input
              type="text"
              value={filterText}
              onChange={(e) => setFilterText(e.target.value)}
              placeholder={t("learning.filterCategoriesPlaceholder")}
              className="focus:border-brand w-full rounded-xl border border-neutral-200/90 bg-neutral-50/50 py-2 pr-3 pl-8 text-sm text-neutral-800 transition-colors placeholder:text-neutral-400 focus:bg-white focus:outline-hidden dark:border-zinc-800 dark:bg-zinc-900/80 dark:text-zinc-200"
            />
          </div>

          {/* Category Groups List */}
          <div className="mt-4 flex-1 space-y-6 overflow-y-auto pr-1">
            {filteredGroups.map((group) => {
              const displayTitle =
                isEn && group.titleEn ? group.titleEn : group.title;
              return (
                <div key={group.id} className="space-y-1">
                  {/* Group Title */}
                  <h3 className="px-2 text-sm font-bold tracking-wider text-blue-500 uppercase dark:text-blue-500">
                    {displayTitle}
                  </h3>

                  {/* Group Items */}
                  <div className="space-y-0.5">
                    {group.items.map((item) => {
                      const isSelected = activeCategory === item.id;
                      const countToShow = isSelected
                        ? activeCategoryCount
                        : undefined;
                      const displayLabel =
                        isEn && item.labelEn ? item.labelEn : item.label;

                      return (
                        <button
                          key={item.id}
                          type="button"
                          onClick={() =>
                            onSelectCategory({
                              department: item.department,
                              topic: item.topic,
                              label: displayLabel,
                            })
                          }
                          className={`group flex w-full cursor-pointer items-center justify-between rounded-xl px-2 py-2 text-sm font-medium transition-colors ${
                            isSelected
                              ? "bg-blue-100 font-semibold text-blue-600 dark:bg-blue-950/40 dark:text-blue-400"
                              : "text-neutral-800 hover:bg-neutral-100 hover:text-neutral-900 dark:text-zinc-300 dark:hover:bg-zinc-900 dark:hover:text-white"
                          }`}
                        >
                          <div className="flex items-center gap-2.5 truncate">
                            <div className="flex size-8 shrink-0 items-center justify-center">
                              {getCategoryIcon(item.id, item.label)}
                            </div>
                            <span className="truncate text-sm">
                              {displayLabel}
                            </span>
                            {item.isNew && (
                              <span className="py-0.2 rounded bg-blue-100 px-1 text-[8px] font-bold text-blue-600 dark:bg-blue-950 dark:text-blue-400">
                                {t("learning.isNew")}
                              </span>
                            )}
                          </div>

                          {countToShow !== undefined && (
                            <span className="py-0.2 rounded-full bg-blue-100/70 px-2 text-[10px] font-bold text-blue-600 dark:bg-blue-950/60 dark:text-blue-400">
                              {countToShow}
                            </span>
                          )}
                        </button>
                      );
                    })}
                  </div>
                </div>
              );
            })}
          </div>
        </div>
      ) : (
        /* Collapsed Icon-Only View */
        <div className="flex flex-col items-center gap-3 py-6"></div>
      )}
    </aside>
  );
}
