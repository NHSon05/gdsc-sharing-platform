"use client";

import React from "react";
import Link from "next/link";
import { Card } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import type { RoadmapSummary } from "../types/roadmap.types";
import {
  Compass,
  Clock,
  Layers,
  ArrowRight,
  Code2,
  Database,
  Cpu,
  Smartphone,
  Globe,
  Archive,
  FileCheck,
} from "lucide-react";

interface RoadmapCardProps {
  roadmap: RoadmapSummary;
}

function getCategoryIcon(categoryName: string) {
  const lower = categoryName.toLowerCase();
  if (lower.includes("front") || lower.includes("web"))
    return <Globe className="size-5 text-brand" />;
  if (lower.includes("back") || lower.includes("server"))
    return <Code2 className="size-5 text-emerald-500" />;
  if (lower.includes("data") || lower.includes("database"))
    return <Database className="size-5 text-amber-500" />;
  if (lower.includes("ai") || lower.includes("intel") || lower.includes("ml"))
    return <Cpu className="size-5 text-purple-500" />;
  if (lower.includes("mobile") || lower.includes("app"))
    return <Smartphone className="size-5 text-rose-500" />;
  return <Compass className="size-5 text-brand" />;
}

function getLevelBadge(level: string) {
  switch (level) {
    case "Beginner":
      return (
        <span className="inline-flex items-center rounded-full border border-emerald-200 bg-emerald-50 px-2.5 py-0.5 text-xs font-semibold text-emerald-700 dark:border-emerald-800/60 dark:bg-emerald-950/40 dark:text-emerald-300">
          Beginner
        </span>
      );
    case "Intermediate":
      return (
        <span className="inline-flex items-center rounded-full border border-sky-200 bg-sky-50 px-2.5 py-0.5 text-xs font-semibold text-sky-700 dark:border-sky-800/60 dark:bg-sky-950/40 dark:text-sky-300">
          Intermediate
        </span>
      );
    case "Advanced":
      return (
        <span className="inline-flex items-center rounded-full border border-purple-200 bg-purple-50 px-2.5 py-0.5 text-xs font-semibold text-purple-700 dark:border-purple-800/60 dark:bg-purple-950/40 dark:text-purple-300">
          Advanced
        </span>
      );
    default:
      return (
        <span className="inline-flex items-center rounded-full border border-neutral-200 bg-neutral-100 px-2.5 py-0.5 text-xs font-semibold text-neutral-700 dark:border-zinc-700 dark:bg-zinc-800 dark:text-zinc-300">
          All Levels
        </span>
      );
  }
}

export function RoadmapCard({ roadmap }: RoadmapCardProps) {
  const nodeCount = roadmap.nodeCount ?? roadmap.sectionCount ?? 0;

  return (
    <Card
      variant="liquid-glass"
      className="group relative flex flex-col justify-between overflow-hidden p-6 transition-all duration-300 hover:-translate-y-1 hover:border-brand/40 hover:shadow-xl dark:hover:shadow-zinc-950/50"
    >
      {/* Top row: Category info & Level */}
      <div>
        <div className="flex items-start justify-between gap-3">
          <div className="flex items-center gap-2.5">
            <div className="flex size-10 items-center justify-center rounded-xl bg-white shadow-2xs dark:bg-zinc-800">
              {getCategoryIcon(roadmap.category.name)}
            </div>
            <div>
              <span className="text-xs font-semibold text-neutral-500 uppercase tracking-wider dark:text-zinc-400">
                {roadmap.category.name}
              </span>
            </div>
          </div>
          <div className="flex items-center gap-1.5">
            {roadmap.status === "Archived" && (
              <Badge
                variant="outline"
                className="gap-1 border-amber-300 bg-amber-50 text-amber-800 text-[11px] dark:border-amber-800 dark:bg-amber-950/30 dark:text-amber-300"
              >
                <Archive className="size-3" />
                Archived
              </Badge>
            )}
            {roadmap.status === "Draft" && (
              <Badge
                variant="outline"
                className="gap-1 border-neutral-300 bg-neutral-100 text-neutral-700 text-[11px] dark:border-zinc-700 dark:bg-zinc-800 dark:text-zinc-300"
              >
                <FileCheck className="size-3" />
                Draft
              </Badge>
            )}
            {getLevelBadge(roadmap.level)}
          </div>
        </div>

        {/* Title and Short Description */}
        <div className="mt-4 space-y-2">
          <h3 className="group-hover:text-brand text-lg font-bold tracking-tight text-neutral-900 transition-colors dark:text-zinc-100">
            {roadmap.title}
          </h3>
          <p className="line-clamp-2 text-sm leading-relaxed text-neutral-600 dark:text-zinc-400">
            {roadmap.shortDescription || "Structured roadmap with nodes, guides and learning checkpoints."}
          </p>
        </div>
      </div>

      {/* Footer info & CTA button */}
      <div className="mt-6 border-t border-neutral-200/60 pt-4 dark:border-zinc-800/60">
        <div className="flex items-center justify-between">
          <div className="flex items-center gap-4 text-xs text-neutral-500 dark:text-zinc-400">
            <div className="flex items-center gap-1" title="Interactive topics & milestones">
              <Layers className="size-3.5 text-brand" />
              <span>{nodeCount} {nodeCount === 1 ? "node" : "nodes"}</span>
            </div>
            {roadmap.estimatedDuration && (
              <div className="flex items-center gap-1" title="Estimated duration">
                <Clock className="size-3.5 text-amber-500" />
                <span>{roadmap.estimatedDuration}</span>
              </div>
            )}
          </div>

          <Link
            href={`/roadmaps/${encodeURIComponent(roadmap.slug || roadmap.id)}`}
            className="shrink-0"
          >
            <Button
              variant="brand"
              size="sm"
              rightIcon={
                <ArrowRight className="size-3.5 transition-transform duration-200 group-hover/btn:translate-x-0.5" />
              }
              className="group/btn shrink-0 whitespace-nowrap rounded-full px-4 text-xs font-semibold shadow-xs"
            >
              Explore
            </Button>
          </Link>
        </div>
      </div>
    </Card>
  );
}
