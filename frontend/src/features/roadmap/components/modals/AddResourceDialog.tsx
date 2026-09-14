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
import {
  useCreateLinkResourceMutation,
  useUploadFileResourceMutation,
} from "../../hooks/use-roadmap-admin-mutations";
import { Link2, FileUp, Loader2 } from "lucide-react";

interface AddResourceDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  roadmapId: string;
  nodeId: string;
}

export function AddResourceDialog({
  open,
  onOpenChange,
  roadmapId,
  nodeId,
}: AddResourceDialogProps) {
  const [tab, setTab] = useState<"link" | "file">("link");
  const [title, setTitle] = useState("");
  const [description, setDescription] = useState("");
  const [externalUrl, setExternalUrl] = useState("");
  const [selectedFile, setSelectedFile] = useState<File | null>(null);
  const [errorMsg, setErrorMsg] = useState<string | null>(null);

  const createLinkMutation = useCreateLinkResourceMutation(roadmapId, nodeId);
  const uploadFileMutation = useUploadFileResourceMutation(roadmapId, nodeId);

  const isPending =
    createLinkMutation.isPending || uploadFileMutation.isPending;

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setErrorMsg(null);

    if (!title.trim()) {
      setErrorMsg("Title is required.");
      return;
    }

    try {
      if (tab === "link") {
        if (!externalUrl.trim()) {
          setErrorMsg("External URL is required.");
          return;
        }
        await createLinkMutation.mutateAsync({
          title: title.trim(),
          description: description.trim() || null,
          externalUrl: externalUrl.trim(),
          sortOrder: 0,
        });
      } else {
        if (!selectedFile) {
          setErrorMsg("Please select a file to upload.");
          return;
        }
        const formData = new FormData();
        formData.append("title", title.trim());
        if (description.trim()) {
          formData.append("description", description.trim());
        }
        formData.append("sortOrder", "0");
        formData.append("file", selectedFile);

        await uploadFileMutation.mutateAsync(formData);
      }

      onOpenChange(false);
      setTitle("");
      setDescription("");
      setExternalUrl("");
      setSelectedFile(null);
    } catch (err: unknown) {
      const msg =
        err && typeof err === "object" && "message" in err
          ? String(err.message)
          : "Failed to add resource.";
      setErrorMsg(msg);
    }
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="max-w-md">
        <form onSubmit={handleSubmit}>
          <DialogHeader>
            <DialogTitle>Add Learning Resource</DialogTitle>
          </DialogHeader>

          {/* Type Toggle Tabs */}
          <div className="flex gap-2 border-b border-neutral-200 pt-1 pb-3 dark:border-zinc-800">
            <button
              type="button"
              onClick={() => {
                setTab("link");
                setErrorMsg(null);
              }}
              className={`flex items-center gap-1.5 rounded-full px-3.5 py-1.5 text-xs font-semibold transition-all ${
                tab === "link"
                  ? "bg-brand text-white shadow-xs"
                  : "bg-neutral-100 text-neutral-600 dark:bg-zinc-800 dark:text-zinc-400"
              }`}
            >
              <Link2 className="size-3.5" />
              <span>External Link</span>
            </button>

            <button
              type="button"
              onClick={() => {
                setTab("file");
                setErrorMsg(null);
              }}
              className={`flex items-center gap-1.5 rounded-full px-3.5 py-1.5 text-xs font-semibold transition-all ${
                tab === "file"
                  ? "bg-brand text-white shadow-xs"
                  : "bg-neutral-100 text-neutral-600 dark:bg-zinc-800 dark:text-zinc-400"
              }`}
            >
              <FileUp className="size-3.5" />
              <span>Upload Document</span>
            </button>
          </div>

          <div className="space-y-4 py-4">
            {errorMsg && (
              <div className="rounded-xl border border-rose-200 bg-rose-50 p-2.5 text-xs font-semibold text-rose-700 dark:border-rose-900/50 dark:bg-rose-950/40 dark:text-rose-300">
                {errorMsg}
              </div>
            )}

            <div>
              <label className="block text-xs font-semibold text-neutral-700 dark:text-zinc-300">
                Resource Title *
              </label>
              <input
                type="text"
                value={title}
                onChange={(e) => setTitle(e.target.value)}
                placeholder="e.g. Official Documentation or Cheatsheet"
                className="mt-1 h-9 w-full rounded-xl border border-neutral-200 bg-white px-3 text-xs dark:border-zinc-800 dark:bg-zinc-900"
                required
              />
            </div>

            <div>
              <label className="block text-xs font-semibold text-neutral-700 dark:text-zinc-300">
                Description
              </label>
              <input
                type="text"
                value={description}
                onChange={(e) => setDescription(e.target.value)}
                placeholder="Brief summary of why this resource is useful"
                className="mt-1 h-9 w-full rounded-xl border border-neutral-200 bg-white px-3 text-xs dark:border-zinc-800 dark:bg-zinc-900"
              />
            </div>

            {tab === "link" ? (
              <div>
                <label className="block text-xs font-semibold text-neutral-700 dark:text-zinc-300">
                  External URL *
                </label>
                <input
                  type="url"
                  value={externalUrl}
                  onChange={(e) => setExternalUrl(e.target.value)}
                  placeholder="https://react.dev"
                  className="mt-1 h-9 w-full rounded-xl border border-neutral-200 bg-white px-3 font-mono text-xs dark:border-zinc-800 dark:bg-zinc-900"
                  required
                />
              </div>
            ) : (
              <div>
                <label className="block text-xs font-semibold text-neutral-700 dark:text-zinc-300">
                  Select File *
                </label>
                <input
                  type="file"
                  onChange={(e) => setSelectedFile(e.target.files?.[0] || null)}
                  className="file:bg-brand/10 file:text-brand hover:file:bg-brand/20 dark:file:bg-brand/20 dark:file:text-brand mt-1 block w-full text-xs text-neutral-600 file:mr-3 file:rounded-full file:border-0 file:px-3 file:py-1.5 file:text-xs file:font-semibold dark:text-zinc-400"
                  required
                />
              </div>
            )}
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
              disabled={isPending}
              className="gap-1.5 rounded-full"
            >
              {isPending && <Loader2 className="size-3.5 animate-spin" />}
              <span>{tab === "link" ? "Add Link" : "Upload File"}</span>
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}
