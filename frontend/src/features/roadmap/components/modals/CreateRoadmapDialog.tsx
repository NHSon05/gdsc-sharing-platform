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
import { useCreateRoadmapMutation } from "../../hooks/use-roadmap-admin-mutations";
import type {
  CategoryResponse,
  RoadmapLevel,
  CreateRoadmapRequest,
} from "../../types/roadmap.types";
import { Loader2 } from "lucide-react";

interface CreateRoadmapDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  categories: CategoryResponse[];
}

function slugify(text: string): string {
  return text
    .toLowerCase()
    .trim()
    .replace(/[^\w\s-]/g, "")
    .replace(/[\s_-]+/g, "-")
    .replace(/^-+|-+$/g, "");
}

export function CreateRoadmapDialog({
  open,
  onOpenChange,
  categories,
}: CreateRoadmapDialogProps) {
  const createMutation = useCreateRoadmapMutation();

  const [title, setTitle] = useState("");
  const [slug, setSlug] = useState("");
  const [shortDescription, setShortDescription] = useState("");
  const [description, setDescription] = useState("");
  const [categoryId, setCategoryId] = useState("");
  const [level, setLevel] = useState<RoadmapLevel>("Beginner");
  const [estimatedDuration, setEstimatedDuration] = useState("");
  const [prerequisites, setPrerequisites] = useState("");
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
      setErrorMsg("Title is required.");
      return;
    }
    if (!slug.trim()) {
      setErrorMsg("Slug is required.");
      return;
    }
    if (!categoryId) {
      setErrorMsg("Please select a category.");
      return;
    }

    const payload: CreateRoadmapRequest = {
      title: title.trim(),
      slug: slug.trim(),
      shortDescription: shortDescription.trim() || title.trim(),
      description: description.trim() || null,
      categoryId,
      level,
      estimatedDuration: estimatedDuration.trim() || null,
      prerequisites: prerequisites.trim() || null,
      sortOrder: 0,
    };

    try {
      await createMutation.mutateAsync(payload);
      onOpenChange(false);
      // Reset form
      setTitle("");
      setSlug("");
      setShortDescription("");
      setDescription("");
      setEstimatedDuration("");
      setPrerequisites("");
    } catch (err: unknown) {
      const msg =
        err && typeof err === "object" && "message" in err
          ? String(err.message)
          : "Failed to create roadmap. Please check your inputs.";
      setErrorMsg(msg);
    }
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="max-w-lg">
        <form onSubmit={handleSubmit}>
          <DialogHeader>
            <DialogTitle>Create New Roadmap</DialogTitle>
          </DialogHeader>

          <div className="space-y-4 py-4">
            {errorMsg && (
              <div className="rounded-xl border border-rose-200 bg-rose-50 p-3 text-xs font-semibold text-rose-700 dark:border-rose-900/50 dark:bg-rose-950/40 dark:text-rose-300">
                {errorMsg}
              </div>
            )}

            <div>
              <label className="block text-xs font-semibold text-neutral-700 dark:text-zinc-300">
                Roadmap Title *
              </label>
              <input
                type="text"
                value={title}
                onChange={(e) => handleTitleChange(e.target.value)}
                placeholder="e.g. Modern Frontend Developer"
                className="mt-1 h-9 w-full rounded-xl border border-neutral-200 bg-white px-3 text-xs dark:border-zinc-800 dark:bg-zinc-900"
                required
              />
            </div>

            <div>
              <label className="block text-xs font-semibold text-neutral-700 dark:text-zinc-300">
                Slug (URL Identifier) *
              </label>
              <input
                type="text"
                value={slug}
                onChange={(e) => setSlug(e.target.value)}
                placeholder="e.g. modern-frontend-developer"
                className="mt-1 h-9 w-full rounded-xl border border-neutral-200 bg-white px-3 text-xs font-mono dark:border-zinc-800 dark:bg-zinc-900"
                required
              />
            </div>

            <div className="grid grid-cols-2 gap-3">
              <div>
                <label className="block text-xs font-semibold text-neutral-700 dark:text-zinc-300">
                  Category *
                </label>
                <select
                  value={categoryId}
                  onChange={(e) => setCategoryId(e.target.value)}
                  className="mt-1 h-9 w-full rounded-xl border border-neutral-200 bg-white px-3 text-xs dark:border-zinc-800 dark:bg-zinc-900"
                  required
                >
                  <option value="">Select category...</option>
                  {categories.map((cat) => (
                    <option key={cat.id} value={cat.id}>
                      {cat.name}
                    </option>
                  ))}
                </select>
              </div>

              <div>
                <label className="block text-xs font-semibold text-neutral-700 dark:text-zinc-300">
                  Level *
                </label>
                <select
                  value={level}
                  onChange={(e) => setLevel(e.target.value as RoadmapLevel)}
                  className="mt-1 h-9 w-full rounded-xl border border-neutral-200 bg-white px-3 text-xs dark:border-zinc-800 dark:bg-zinc-900"
                >
                  <option value="Beginner">Beginner</option>
                  <option value="Intermediate">Intermediate</option>
                  <option value="Advanced">Advanced</option>
                  <option value="AllLevels">All Levels</option>
                </select>
              </div>
            </div>

            <div>
              <label className="block text-xs font-semibold text-neutral-700 dark:text-zinc-300">
                Short Description
              </label>
              <input
                type="text"
                value={shortDescription}
                onChange={(e) => setShortDescription(e.target.value)}
                placeholder="Brief summary shown on catalog cards"
                className="mt-1 h-9 w-full rounded-xl border border-neutral-200 bg-white px-3 text-xs dark:border-zinc-800 dark:bg-zinc-900"
              />
            </div>

            <div>
              <label className="block text-xs font-semibold text-neutral-700 dark:text-zinc-300">
                Estimated Duration
              </label>
              <input
                type="text"
                value={estimatedDuration}
                onChange={(e) => setEstimatedDuration(e.target.value)}
                placeholder="e.g. 6 months"
                className="mt-1 h-9 w-full rounded-xl border border-neutral-200 bg-white px-3 text-xs dark:border-zinc-800 dark:bg-zinc-900"
              />
            </div>

            <div>
              <label className="block text-xs font-semibold text-neutral-700 dark:text-zinc-300">
                Prerequisites
              </label>
              <input
                type="text"
                value={prerequisites}
                onChange={(e) => setPrerequisites(e.target.value)}
                placeholder="e.g. Basic programming knowledge"
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
              <span>Create Roadmap</span>
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}
