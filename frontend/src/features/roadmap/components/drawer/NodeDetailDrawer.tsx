"use client";

import React, { useState } from "react";
import { useNodeDetailQuery } from "../../hooks/use-node-detail-query";
import { downloadResourceFile } from "../../api/roadmap.api";
import type {
  RoadmapNodeDto,
  LearningResourceDto,
} from "../../types/roadmap.types";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import {
  X,
  BookOpen,
  Clock,
  Target,
  ArrowRight,
  ArrowLeft,
  ExternalLink,
  Download,
  FileText,
  Link2,
  Trash2,
  Plus,
  Loader2,
  Power,
} from "lucide-react";

interface NodeDetailDrawerProps {
  roadmapId: string;
  node: RoadmapNodeDto | null;
  isOpen: boolean;
  onClose: () => void;
  onSelectNodeById: (nodeId: string) => void;
  // Admin capabilities
  isAdmin: boolean;
  onOpenAddResource: (nodeId: string) => void;
  onToggleNodeStatus?: (nodeId: string, currentActive: boolean) => void;
  onDeleteResource?: (resourceId: string) => void;
}

function formatFileSize(bytes: number | null): string {
  if (!bytes) return "0 KB";
  if (bytes < 1024) return `${bytes} B`;
  if (bytes < 1024 * 1024) return `${Math.round(bytes / 1024)} KB`;
  return `${(bytes / (1024 * 1024)).toFixed(1)} MB`;
}

export function NodeDetailDrawer({
  roadmapId,
  node,
  isOpen,
  onClose,
  onSelectNodeById,
  isAdmin,
  onOpenAddResource,
  onToggleNodeStatus,
  onDeleteResource,
}: NodeDetailDrawerProps) {
  const [downloadingId, setDownloadingId] = useState<string | null>(null);

  const { data: detail, isLoading } = useNodeDetailQuery(
    roadmapId,
    node?.id
  );

  if (!isOpen || !node) return null;

  const handleDownload = async (resource: LearningResourceDto) => {
    try {
      setDownloadingId(resource.id);
      await downloadResourceFile(
        resource.id,
        resource.originalFileName || `${resource.title}.pdf`
      );
    } catch {
      // Error handling is handled by interceptor
    } finally {
      setDownloadingId(null);
    }
  };

  const learningObjectives =
    detail?.learningObjectives || "Master the core principles and apply in practical projects.";
  const estimatedDuration =
    detail?.estimatedDuration || "1-2 weeks";
  const prerequisites = detail?.prerequisites ?? [];
  const nextNodes = detail?.nextNodes ?? [];
  const resources = detail?.resources ?? [];

  return (
    <>
      {/* Backdrop for mobile */}
      <div
        onClick={onClose}
        className="fixed inset-0 z-40 bg-neutral-950/40 backdrop-blur-xs transition-opacity lg:hidden"
      />

      {/* Slide-over panel */}
      <aside className="fixed inset-y-0 right-0 z-50 flex w-full max-w-md flex-col border-l border-neutral-200/90 bg-white shadow-2xl transition-all duration-300 sm:max-w-lg dark:border-zinc-800 dark:bg-zinc-900 animate-in slide-in-from-right">
        {/* Header */}
        <div className="flex items-start justify-between border-b border-neutral-200/80 p-5 dark:border-zinc-800/80">
          <div className="space-y-1.5 min-w-0 pr-4">
            <div className="flex items-center gap-2">
              <Badge
                variant="outline"
                className={`text-[11px] font-semibold ${
                  node.nodeType === "Milestone"
                    ? "border-amber-400 bg-amber-50 text-amber-700 dark:bg-amber-950/40 dark:text-amber-300"
                    : node.nodeType === "Group"
                    ? "border-purple-300 bg-purple-50 text-purple-700 dark:bg-purple-950/40 dark:text-purple-300"
                    : "border-brand-border bg-brand-muted text-brand"
                }`}
              >
                {node.nodeType}
              </Badge>

              {!node.isActive && (
                <span className="rounded-full bg-rose-100 px-2 py-0.5 text-[10px] font-bold text-rose-700 dark:bg-rose-950/50 dark:text-rose-400">
                  Inactive
                </span>
              )}
            </div>

            <h2 className="text-xl font-bold tracking-tight text-neutral-900 break-words dark:text-zinc-100">
              {node.title}
            </h2>
          </div>

          <button
            type="button"
            onClick={onClose}
            className="flex size-8 shrink-0 items-center justify-center rounded-lg text-neutral-400 hover:bg-neutral-100 hover:text-neutral-900 dark:text-zinc-500 dark:hover:bg-zinc-800 dark:hover:text-zinc-100"
            aria-label="Close detail panel"
          >
            <X className="size-5" />
          </button>
        </div>

        {/* Body content */}
        <div className="flex-1 overflow-y-auto p-5 space-y-6">
          {isLoading ? (
            <div className="space-y-4 animate-pulse">
              <div className="h-20 rounded-xl bg-neutral-100 dark:bg-zinc-800" />
              <div className="h-32 rounded-xl bg-neutral-100 dark:bg-zinc-800" />
              <div className="h-28 rounded-xl bg-neutral-100 dark:bg-zinc-800" />
            </div>
          ) : (
            <>
              {/* Estimated duration strip */}
              <div className="flex items-center gap-2 rounded-xl border border-neutral-200/60 bg-neutral-50/60 px-3.5 py-2 text-xs font-medium text-neutral-700 dark:border-zinc-800 dark:bg-zinc-800/40 dark:text-zinc-300">
                <Clock className="size-4 text-amber-500" />
                <span>Estimated completion:</span>
                <span className="font-bold text-neutral-900 dark:text-white">
                  {estimatedDuration}
                </span>
              </div>

              {/* Description */}
              <div className="space-y-2">
                <h3 className="text-xs font-bold uppercase tracking-wider text-neutral-500 dark:text-zinc-400">
                  Overview
                </h3>
                <p className="text-sm leading-relaxed text-neutral-700 dark:text-zinc-300">
                  {node.description || "Detailed conceptual overview and foundational understanding."}
                </p>
              </div>

              {/* Learning Objectives */}
              <div className="space-y-2">
                <div className="flex items-center gap-1.5 text-xs font-bold uppercase tracking-wider text-neutral-500 dark:text-zinc-400">
                  <Target className="size-3.5 text-brand" />
                  <span>Learning Objectives</span>
                </div>
                <div className="rounded-2xl border border-neutral-200/80 bg-neutral-50/60 p-4 text-xs leading-relaxed text-neutral-700 dark:border-zinc-800 dark:bg-zinc-800/40 dark:text-zinc-300">
                  {learningObjectives}
                </div>
              </div>

              {/* Prerequisites */}
              {prerequisites.length > 0 && (
                <div className="space-y-2">
                  <div className="flex items-center gap-1.5 text-xs font-bold uppercase tracking-wider text-neutral-500 dark:text-zinc-400">
                    <ArrowLeft className="size-3.5 text-amber-500" />
                    <span>Prerequisites</span>
                  </div>
                  <div className="flex flex-wrap gap-2">
                    {prerequisites.map((p) => (
                      <button
                        key={p.id}
                        type="button"
                        onClick={() => onSelectNodeById(p.id)}
                        className="group flex items-center gap-1.5 rounded-full border border-neutral-200 bg-white px-3 py-1 text-xs font-semibold text-neutral-800 shadow-2xs hover:border-brand hover:text-brand dark:border-zinc-700 dark:bg-zinc-800 dark:text-zinc-200 dark:hover:border-brand"
                      >
                        <span>{p.title}</span>
                        <ArrowRight className="size-3 transition-transform group-hover:translate-x-0.5" />
                      </button>
                    ))}
                  </div>
                </div>
              )}

              {/* Next Nodes */}
              {nextNodes.length > 0 && (
                <div className="space-y-2">
                  <div className="flex items-center gap-1.5 text-xs font-bold uppercase tracking-wider text-neutral-500 dark:text-zinc-400">
                    <ArrowRight className="size-3.5 text-emerald-500" />
                    <span>Recommended Next Steps</span>
                  </div>
                  <div className="flex flex-wrap gap-2">
                    {nextNodes.map((n) => (
                      <button
                        key={n.id}
                        type="button"
                        onClick={() => onSelectNodeById(n.id)}
                        className="group flex items-center gap-1.5 rounded-full border border-neutral-200 bg-white px-3 py-1 text-xs font-semibold text-neutral-800 shadow-2xs hover:border-emerald-500 hover:text-emerald-600 dark:border-zinc-700 dark:bg-zinc-800 dark:text-zinc-200 dark:hover:border-emerald-500"
                      >
                        <span>{n.title}</span>
                        <ArrowRight className="size-3 transition-transform group-hover:translate-x-0.5" />
                      </button>
                    ))}
                  </div>
                </div>
              )}

              {/* Learning Resources (Links & Files) */}
              <div className="space-y-3 pt-2">
                <div className="flex items-center justify-between border-t border-neutral-200/80 pt-4 dark:border-zinc-800/80">
                  <div className="flex items-center gap-1.5 text-xs font-bold uppercase tracking-wider text-neutral-500 dark:text-zinc-400">
                    <BookOpen className="size-3.5 text-brand" />
                    <span>Resources & Attachments ({resources.length})</span>
                  </div>

                  {isAdmin && (
                    <Button
                      variant="outline"
                      size="sm"
                      onClick={() => onOpenAddResource(node.id)}
                      className="gap-1 rounded-full text-xs h-7 px-2.5"
                    >
                      <Plus className="size-3" />
                      <span>Add</span>
                    </Button>
                  )}
                </div>

                {resources.length === 0 ? (
                  <p className="text-xs text-neutral-500 italic dark:text-zinc-500">
                    No resources attached yet.
                  </p>
                ) : (
                  <div className="space-y-2">
                    {resources.map((res) => (
                      <div
                        key={res.id}
                        className="flex items-center justify-between gap-3 rounded-xl border border-neutral-200/80 bg-white p-3 shadow-2xs dark:border-zinc-800 dark:bg-zinc-800/60"
                      >
                        <div className="flex items-center gap-2.5 min-w-0 flex-1">
                          <div
                            className={`flex size-8 shrink-0 items-center justify-center rounded-lg ${
                              res.resourceType === "Link"
                                ? "bg-sky-50 text-sky-600 dark:bg-sky-950/50 dark:text-sky-400"
                                : "bg-emerald-50 text-emerald-600 dark:bg-emerald-950/50 dark:text-emerald-400"
                            }`}
                          >
                            {res.resourceType === "Link" ? (
                              <Link2 className="size-4" />
                            ) : (
                              <FileText className="size-4" />
                            )}
                          </div>

                          <div className="min-w-0 flex-1">
                            <h5 className="truncate text-xs font-bold text-neutral-900 dark:text-zinc-100">
                              {res.title}
                            </h5>
                            <div className="flex items-center gap-2 text-[11px] text-neutral-500 dark:text-zinc-400">
                              {res.resourceType === "File" && (
                                <span>{formatFileSize(res.fileSize)}</span>
                              )}
                              {res.description && (
                                <span className="truncate max-w-[150px]">
                                  {res.description}
                                </span>
                              )}
                            </div>
                          </div>
                        </div>

                        <div className="flex items-center gap-1.5 shrink-0">
                          {res.resourceType === "Link" && res.externalUrl && (
                            <a
                              href={res.externalUrl}
                              target="_blank"
                              rel="noreferrer noopener"
                              className="flex size-7 items-center justify-center rounded-lg border border-neutral-200 text-neutral-600 hover:bg-neutral-100 hover:text-brand dark:border-zinc-700 dark:text-zinc-400 dark:hover:bg-zinc-700"
                              title="Open in new tab"
                            >
                              <ExternalLink className="size-3.5" />
                            </a>
                          )}

                          {res.resourceType === "File" && (
                            <Button
                              variant="outline"
                              size="sm"
                              disabled={downloadingId === res.id}
                              onClick={() => handleDownload(res)}
                              className="size-7 p-0 rounded-lg"
                              title="Download file"
                            >
                              {downloadingId === res.id ? (
                                <Loader2 className="size-3.5 animate-spin text-brand" />
                              ) : (
                                <Download className="size-3.5" />
                              )}
                            </Button>
                          )}

                          {isAdmin && onDeleteResource && (
                            <button
                              type="button"
                              onClick={() => onDeleteResource(res.id)}
                              className="flex size-7 items-center justify-center rounded-lg text-neutral-400 hover:bg-rose-50 hover:text-rose-600 dark:hover:bg-rose-950/50"
                              title="Remove resource"
                            >
                              <Trash2 className="size-3.5" />
                            </button>
                          )}
                        </div>
                      </div>
                    ))}
                  </div>
                )}
              </div>
            </>
          )}
        </div>

        {/* Admin Footer Controls */}
        {isAdmin && onToggleNodeStatus && (
          <div className="border-t border-neutral-200/80 p-4 bg-neutral-50/50 dark:border-zinc-800 dark:bg-zinc-900/50">
            <Button
              variant={node.isActive ? "outline" : "brand"}
              size="sm"
              onClick={() => onToggleNodeStatus(node.id, node.isActive)}
              className="w-full gap-2 text-xs rounded-xl"
            >
              <Power className="size-3.5" />
              <span>
                {node.isActive ? "Deactivate Node" : "Activate Node"}
              </span>
            </Button>
          </div>
        )}
      </aside>
    </>
  );
}
