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
import { useCreateNodeMutation } from "../../hooks/use-roadmap-admin-mutations";
import type {
  RoadmapNodeType,
  CreateNodeRequest,
} from "../../types/roadmap.types";
import { Loader2 } from "lucide-react";

interface CreateNodeDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  roadmapId: string;
  defaultPosition?: { x: number; y: number };
}

function slugify(text: string): string {
  return text
    .toLowerCase()
    .trim()
    .replace(/[^\w\s-]/g, "")
    .replace(/[\s_-]+/g, "-")
    .replace(/^-+|-+$/g, "");
}

export function CreateNodeDialog({
  open,
  onOpenChange,
  roadmapId,
  defaultPosition = { x: 300, y: 250 },
}: CreateNodeDialogProps) {
  const createMutation = useCreateNodeMutation(roadmapId);

  const [title, setTitle] = useState("");
  const [slug, setSlug] = useState("");
  const [nodeType, setNodeType] = useState<RoadmapNodeType>("Topic");
  const [description, setDescription] = useState("");
  const [learningObjectives, setLearningObjectives] = useState("");
  const [estimatedDuration, setEstimatedDuration] = useState("");
  const [color, setColor] = useState("#4285F4");
  const [errorMsg, setErrorMsg] = useState<string | null>(null);

  const handleTitleChange = (val: string) => {
    setTitle(val);
    if (!slug || slug === slugify(title)) {
      setSlug(slugify(val));
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setErrorMsg(null);

    if (!title.trim()) {
      setErrorMsg("Node title is required.");
      return;
    }
    if (!slug.trim()) {
      setErrorMsg("Slug is required.");
      return;
    }

    const payload: CreateNodeRequest = {
      title: title.trim(),
      slug: slug.trim(),
      nodeType,
      description: description.trim() || null,
      learningObjectives: learningObjectives.trim() || null,
      estimatedDuration: estimatedDuration.trim() || null,
      color: nodeType === "Milestone" ? "#F59E0B" : color,
      positionX: defaultPosition.x,
      positionY: defaultPosition.y,
      sortOrder: 0,
    };

    try {
      await createMutation.mutateAsync(payload);
      onOpenChange(false);
      setTitle("");
      setSlug("");
      setDescription("");
      setLearningObjectives("");
      setEstimatedDuration("");
    } catch (err: unknown) {
      const msg =
        err && typeof err === "object" && "message" in err
          ? String(err.message)
          : "Failed to create node.";
      setErrorMsg(msg);
    }
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="max-w-md">
        <form onSubmit={handleSubmit}>
          <DialogHeader>
            <DialogTitle>Add Node to Canvas</DialogTitle>
          </DialogHeader>

          <div className="space-y-4 py-4">
            {errorMsg && (
              <div className="rounded-xl border border-rose-200 bg-rose-50 p-2.5 text-xs font-semibold text-rose-700 dark:border-rose-900/50 dark:bg-rose-950/40 dark:text-rose-300">
                {errorMsg}
              </div>
            )}

            <div>
              <label className="block text-xs font-semibold text-neutral-700 dark:text-zinc-300">
                Node Title *
              </label>
              <input
                type="text"
                value={title}
                onChange={(e) => handleTitleChange(e.target.value)}
                placeholder="e.g. React Fundamentals"
                className="mt-1 h-9 w-full rounded-xl border border-neutral-200 bg-white px-3 text-xs dark:border-zinc-800 dark:bg-zinc-900"
                required
              />
            </div>

            <div className="grid grid-cols-2 gap-3">
              <div>
                <label className="block text-xs font-semibold text-neutral-700 dark:text-zinc-300">
                  Node Type *
                </label>
                <select
                  value={nodeType}
                  onChange={(e) => setNodeType(e.target.value as RoadmapNodeType)}
                  className="mt-1 h-9 w-full rounded-xl border border-neutral-200 bg-white px-3 text-xs dark:border-zinc-800 dark:bg-zinc-900"
                >
                  <option value="Topic">Topic (Core Skill)</option>
                  <option value="Milestone">Milestone (Checkpoint)</option>
                  <option value="Group">Group (Skill Cluster)</option>
                </select>
              </div>

              <div>
                <label className="block text-xs font-semibold text-neutral-700 dark:text-zinc-300">
                  Estimated Duration
                </label>
                <input
                  type="text"
                  value={estimatedDuration}
                  onChange={(e) => setEstimatedDuration(e.target.value)}
                  placeholder="e.g. 2 weeks"
                  className="mt-1 h-9 w-full rounded-xl border border-neutral-200 bg-white px-3 text-xs dark:border-zinc-800 dark:bg-zinc-900"
                />
              </div>
            </div>

            <div>
              <label className="block text-xs font-semibold text-neutral-700 dark:text-zinc-300">
                Accent Color
              </label>
              <div className="mt-1.5 flex items-center gap-2">
                {[
                  { name: "Blue", hex: "#4285F4" },
                  { name: "Emerald", hex: "#10B981" },
                  { name: "Amber", hex: "#F59E0B" },
                  { name: "Purple", hex: "#8B5CF6" },
                  { name: "Rose", hex: "#EF4444" },
                ].map((c) => (
                  <button
                    key={c.hex}
                    type="button"
                    onClick={() => setColor(c.hex)}
                    style={{ backgroundColor: c.hex }}
                    className={`size-6 rounded-full transition-transform ${
                      color === c.hex
                        ? "ring-2 ring-offset-2 ring-neutral-800 scale-110 dark:ring-white"
                        : "opacity-80 hover:opacity-100"
                    }`}
                    title={c.name}
                  />
                ))}
              </div>
            </div>

            <div>
              <label className="block text-xs font-semibold text-neutral-700 dark:text-zinc-300">
                Description
              </label>
              <textarea
                rows={2}
                value={description}
                onChange={(e) => setDescription(e.target.value)}
                placeholder="Brief summary of concepts learned in this node"
                className="mt-1 w-full rounded-xl border border-neutral-200 bg-white p-2.5 text-xs dark:border-zinc-800 dark:bg-zinc-900"
              />
            </div>

            <div>
              <label className="block text-xs font-semibold text-neutral-700 dark:text-zinc-300">
                Learning Objectives
              </label>
              <textarea
                rows={2}
                value={learningObjectives}
                onChange={(e) => setLearningObjectives(e.target.value)}
                placeholder="Key takeaways and practical outcomes"
                className="mt-1 w-full rounded-xl border border-neutral-200 bg-white p-2.5 text-xs dark:border-zinc-800 dark:bg-zinc-900"
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
              <span>Add Node</span>
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}
