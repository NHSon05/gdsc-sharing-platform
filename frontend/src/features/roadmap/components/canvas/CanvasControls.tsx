"use client";

import React from "react";
import { Button } from "@/components/ui/button";
import {
  ZoomIn,
  ZoomOut,
  Maximize2,
  RotateCcw,
  Info,
  Edit3,
  Plus,
  GitMerge,
  Save,
  Loader2,
} from "lucide-react";

interface CanvasControlsProps {
  zoom: number;
  onZoomIn: () => void;
  onZoomOut: () => void;
  onFitView: () => void;
  onResetView: () => void;
  isLegendOpen: boolean;
  onToggleLegend: () => void;
  // Admin props
  isAdmin: boolean;
  isEditMode: boolean;
  onToggleEditMode: () => void;
  hasUnsavedChanges: boolean;
  isSavingPositions: boolean;
  onSavePositions: () => void;
  onOpenAddNode: () => void;
  onOpenConnectNodes: () => void;
}

export function CanvasControls({
  zoom,
  onZoomIn,
  onZoomOut,
  onFitView,
  onResetView,
  isLegendOpen,
  onToggleLegend,
  isAdmin,
  isEditMode,
  onToggleEditMode,
  hasUnsavedChanges,
  isSavingPositions,
  onSavePositions,
  onOpenAddNode,
  onOpenConnectNodes,
}: CanvasControlsProps) {
  const zoomPercent = Math.round(zoom * 100);

  return (
    <div className="absolute bottom-6 left-6 z-30 flex flex-wrap items-center gap-2">
      {/* Zoom and Navigation Controls pill */}
      <div className="flex items-center gap-1 rounded-2xl border border-neutral-200/90 bg-white/90 p-1 shadow-lg backdrop-blur-md dark:border-zinc-800 dark:bg-zinc-900/90">
        <button
          type="button"
          onClick={onZoomOut}
          title="Zoom Out (-)"
          className="flex size-8 items-center justify-center rounded-xl text-neutral-600 transition-colors hover:bg-neutral-100 hover:text-neutral-900 dark:text-zinc-400 dark:hover:bg-zinc-800 dark:hover:text-zinc-100"
        >
          <ZoomOut className="size-4" />
        </button>

        <span className="w-12 text-center text-xs font-semibold text-neutral-700 dark:text-zinc-300 select-none">
          {zoomPercent}%
        </span>

        <button
          type="button"
          onClick={onZoomIn}
          title="Zoom In (+)"
          className="flex size-8 items-center justify-center rounded-xl text-neutral-600 transition-colors hover:bg-neutral-100 hover:text-neutral-900 dark:text-zinc-400 dark:hover:bg-zinc-800 dark:hover:text-zinc-100"
        >
          <ZoomIn className="size-4" />
        </button>

        <div className="h-4 w-px bg-neutral-200 dark:bg-zinc-700" />

        <button
          type="button"
          onClick={onFitView}
          title="Fit All Nodes into Screen"
          className="flex size-8 items-center justify-center rounded-xl text-neutral-600 transition-colors hover:bg-neutral-100 hover:text-neutral-900 dark:text-zinc-400 dark:hover:bg-zinc-800 dark:hover:text-zinc-100"
        >
          <Maximize2 className="size-4" />
        </button>

        <button
          type="button"
          onClick={onResetView}
          title="Reset to Center"
          className="flex size-8 items-center justify-center rounded-xl text-neutral-600 transition-colors hover:bg-neutral-100 hover:text-neutral-900 dark:text-zinc-400 dark:hover:bg-zinc-800 dark:hover:text-zinc-100"
        >
          <RotateCcw className="size-3.5" />
        </button>

        <div className="h-4 w-px bg-neutral-200 dark:bg-zinc-700" />

        <button
          type="button"
          onClick={onToggleLegend}
          title="Toggle Legend"
          className={`flex size-8 items-center justify-center rounded-xl transition-colors ${
            isLegendOpen
              ? "bg-brand text-white shadow-xs"
              : "text-neutral-600 hover:bg-neutral-100 hover:text-neutral-900 dark:text-zinc-400 dark:hover:bg-zinc-800 dark:hover:text-zinc-100"
          }`}
        >
          <Info className="size-4" />
        </button>
      </div>

      {/* Admin Authoring Strip */}
      {isAdmin && (
        <div className="flex items-center gap-1.5 rounded-2xl border border-neutral-200/90 bg-white/90 p-1 shadow-lg backdrop-blur-md dark:border-zinc-800 dark:bg-zinc-900/90">
          <Button
            variant={isEditMode ? "brand" : "subtle"}
            size="sm"
            onClick={onToggleEditMode}
            className="gap-1.5 rounded-xl px-3 text-xs"
          >
            <Edit3 className="size-3.5" />
            <span>{isEditMode ? "Editing Mode" : "Edit Layout"}</span>
          </Button>

          {isEditMode && (
            <>
              <Button
                variant="outline"
                size="sm"
                onClick={onOpenAddNode}
                className="gap-1.5 rounded-xl px-3 text-xs"
              >
                <Plus className="size-3.5" />
                <span>Add Node</span>
              </Button>

              <Button
                variant="outline"
                size="sm"
                onClick={onOpenConnectNodes}
                className="gap-1.5 rounded-xl px-3 text-xs"
              >
                <GitMerge className="size-3.5" />
                <span>Connect</span>
              </Button>

              {hasUnsavedChanges && (
                <Button
                  variant="brand"
                  size="sm"
                  onClick={onSavePositions}
                  disabled={isSavingPositions}
                  className="gap-1.5 rounded-xl bg-emerald-600 px-3 text-xs text-white hover:bg-emerald-700 animate-pulse"
                >
                  {isSavingPositions ? (
                    <Loader2 className="size-3.5 animate-spin" />
                  ) : (
                    <Save className="size-3.5" />
                  )}
                  <span>Save Positions</span>
                </Button>
              )}
            </>
          )}
        </div>
      )}
    </div>
  );
}
