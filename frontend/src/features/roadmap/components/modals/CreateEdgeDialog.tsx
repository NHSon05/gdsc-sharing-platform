"use client";

import React, { useState } from "react";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogFooter,
} from "@/components/ui/dialog";
import { Button } from "@/components/ui/button";
import { useCreateEdgeMutation } from "../../hooks/use-roadmap-admin-mutations";
import type {
  RoadmapNodeDto,
  RoadmapRelationType,
  RoadmapLineStyle,
  CreateEdgeRequest,
} from "../../types/roadmap.types";
import { Loader2 } from "lucide-react";

interface CreateEdgeDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  roadmapId: string;
  nodes: RoadmapNodeDto[];
}

export function CreateEdgeDialog({
  open,
  onOpenChange,
  roadmapId,
  nodes,
}: CreateEdgeDialogProps) {
  const createMutation = useCreateEdgeMutation(roadmapId);

  const [sourceNodeId, setSourceNodeId] = useState("");
  const [targetNodeId, setTargetNodeId] = useState("");
  const [relationType, setRelationType] =
    useState<RoadmapRelationType>("Required");
  const [lineStyle, setLineStyle] = useState<RoadmapLineStyle>("Solid");
  const [label, setLabel] = useState("");
  const [errorMsg, setErrorMsg] = useState<string | null>(null);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setErrorMsg(null);

    if (!sourceNodeId) {
      setErrorMsg("Please select a source node.");
      return;
    }
    if (!targetNodeId) {
      setErrorMsg("Please select a target node.");
      return;
    }
    if (sourceNodeId === targetNodeId) {
      setErrorMsg("Source and target nodes cannot be the same.");
      return;
    }

    const payload: CreateEdgeRequest = {
      sourceNodeId,
      targetNodeId,
      relationType,
      lineStyle,
      label: label.trim() || null,
      sortOrder: 0,
    };

    try {
      await createMutation.mutateAsync(payload);
      onOpenChange(false);
      setLabel("");
    } catch (err: unknown) {
      const msg =
        err && typeof err === "object" && "message" in err
          ? String(err.message)
          : "Failed to connect nodes. Edge might already exist or create a cycle.";
      setErrorMsg(msg);
    }
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="max-w-md">
        <form onSubmit={handleSubmit}>
          <DialogHeader>
            <DialogTitle>Connect Two Nodes</DialogTitle>
          </DialogHeader>

          <div className="space-y-4 py-4">
            {errorMsg && (
              <div className="rounded-xl border border-rose-200 bg-rose-50 p-2.5 text-xs font-semibold text-rose-700 dark:border-rose-900/50 dark:bg-rose-950/40 dark:text-rose-300">
                {errorMsg}
              </div>
            )}

            <div>
              <label className="block text-xs font-semibold text-neutral-700 dark:text-zinc-300">
                Source Node (From) *
              </label>
              <select
                value={sourceNodeId}
                onChange={(e) => setSourceNodeId(e.target.value)}
                className="mt-1 h-9 w-full rounded-xl border border-neutral-200 bg-white px-3 text-xs dark:border-zinc-800 dark:bg-zinc-900"
                required
              >
                <option value="">Select source node...</option>
                {nodes.map((n) => (
                  <option key={n.id} value={n.id}>
                    {n.title} ({n.nodeType})
                  </option>
                ))}
              </select>
            </div>

            <div>
              <label className="block text-xs font-semibold text-neutral-700 dark:text-zinc-300">
                Target Node (To) *
              </label>
              <select
                value={targetNodeId}
                onChange={(e) => setTargetNodeId(e.target.value)}
                className="mt-1 h-9 w-full rounded-xl border border-neutral-200 bg-white px-3 text-xs dark:border-zinc-800 dark:bg-zinc-900"
                required
              >
                <option value="">Select target node...</option>
                {nodes
                  .filter((n) => n.id !== sourceNodeId)
                  .map((n) => (
                    <option key={n.id} value={n.id}>
                      {n.title} ({n.nodeType})
                    </option>
                  ))}
              </select>
            </div>

            <div className="grid grid-cols-2 gap-3">
              <div>
                <label className="block text-xs font-semibold text-neutral-700 dark:text-zinc-300">
                  Relation Type *
                </label>
                <select
                  value={relationType}
                  onChange={(e) => {
                    const rel = e.target.value as RoadmapRelationType;
                    setRelationType(rel);
                    if (rel === "Required") setLineStyle("Solid");
                    else setLineStyle("Dashed");
                  }}
                  className="mt-1 h-9 w-full rounded-xl border border-neutral-200 bg-white px-3 text-xs dark:border-zinc-800 dark:bg-zinc-900"
                >
                  <option value="Required">Required</option>
                  <option value="Recommended">Recommended</option>
                  <option value="Optional">Optional</option>
                </select>
              </div>

              <div>
                <label className="block text-xs font-semibold text-neutral-700 dark:text-zinc-300">
                  Line Style *
                </label>
                <select
                  value={lineStyle}
                  onChange={(e) =>
                    setLineStyle(e.target.value as RoadmapLineStyle)
                  }
                  className="mt-1 h-9 w-full rounded-xl border border-neutral-200 bg-white px-3 text-xs dark:border-zinc-800 dark:bg-zinc-900"
                >
                  <option value="Solid">Solid</option>
                  <option value="Dashed">Dashed</option>
                </select>
              </div>
            </div>

            <div>
              <label className="block text-xs font-semibold text-neutral-700 dark:text-zinc-300">
                Optional Label
              </label>
              <input
                type="text"
                value={label}
                onChange={(e) => setLabel(e.target.value)}
                placeholder="e.g. Next, or Alternative"
                className="mt-1 h-9 w-full rounded-xl border border-neutral-200 bg-white px-3 text-xs dark:border-zinc-800 dark:bg-zinc-900"
              />
            </div>
          </div>

          <DialogFooter>
            <Button
              type="button"
              variant="outline"
              size="sm"
              onClick={() => onOpenChange(false)}
              className="rounded-full"
            >
              Cancel
            </Button>
            <Button
              type="submit"
              variant="brand"
              size="sm"
              disabled={createMutation.isPending}
              className="gap-1.5 rounded-full"
            >
              {createMutation.isPending && (
                <Loader2 className="size-3.5 animate-spin" />
              )}
              <span>Create Connection</span>
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}
