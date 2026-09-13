"use client";

import React from "react";
import { X, Check } from "lucide-react";

interface CanvasLegendProps {
  isOpen: boolean;
  onClose: () => void;
}

export function CanvasLegend({ isOpen, onClose }: CanvasLegendProps) {
  if (!isOpen) return null;

  return (
    <div className="absolute top-20 left-6 z-30 w-72 rounded-2xl border border-neutral-300/90 bg-white/95 p-4 shadow-xl backdrop-blur-md dark:border-zinc-800 dark:bg-zinc-900/95 animate-in fade-in slide-in-from-top-2 duration-200">
      <div className="flex items-center justify-between border-b border-neutral-200/80 pb-2.5 dark:border-zinc-800/80">
        <h4 className="text-xs font-bold uppercase tracking-wider text-neutral-800 dark:text-zinc-200">
          Roadmap Legend
        </h4>
        <button
          type="button"
          onClick={onClose}
          className="text-neutral-400 hover:text-neutral-600 dark:text-zinc-500 dark:hover:text-zinc-300"
        >
          <X className="size-4" />
        </button>
      </div>

      <div className="mt-3 space-y-3.5 text-xs">
        {/* Recommendation Badges */}
        <div>
          <span className="font-semibold text-neutral-500 dark:text-zinc-400">
            Recommendation Badges
          </span>
          <div className="mt-2 space-y-2">
            <div className="flex items-center gap-2">
              <div className="flex size-4 shrink-0 items-center justify-center rounded-full bg-[#8B5CF6] text-white shadow-xs">
                <Check className="size-2.5 stroke-[3]" />
              </div>
              <span className="text-neutral-800 font-medium dark:text-zinc-200">
                Personal Recommendation
              </span>
            </div>

            <div className="flex items-center gap-2">
              <div className="flex size-4 shrink-0 items-center justify-center rounded-full bg-[#10B981] text-white shadow-xs">
                <Check className="size-2.5 stroke-[3]" />
              </div>
              <span className="text-neutral-800 font-medium dark:text-zinc-200">
                Alternative Option
              </span>
            </div>

            <div className="flex items-center gap-2">
              <div className="flex size-4 shrink-0 items-center justify-center rounded-full bg-[#6B7280] text-white shadow-xs">
                <Check className="size-2.5 stroke-[3]" />
              </div>
              <span className="text-neutral-800 font-medium dark:text-zinc-200">
                Order not strict on roadmap
              </span>
            </div>
          </div>
        </div>

        {/* Node Styling */}
        <div className="border-t border-neutral-200/80 pt-2.5 dark:border-zinc-800/80">
          <span className="font-semibold text-neutral-500 dark:text-zinc-400">
            Node Boxes
          </span>
          <div className="mt-2 space-y-2">
            <div className="flex items-center gap-2">
              <div className="h-4 w-7 rounded-xs border border-black bg-[#FFE600] shadow-xs" />
              <span className="text-neutral-800 font-medium dark:text-zinc-200">
                Main Core Technology
              </span>
            </div>

            <div className="flex items-center gap-2">
              <div className="h-4 w-7 rounded-xs border border-black bg-[#FFF4C2] shadow-xs" />
              <span className="text-neutral-800 font-medium dark:text-zinc-200">
                Sub-topic / Tool / Library
              </span>
            </div>

            <div className="flex items-center gap-2">
              <div className="h-4 w-7 rounded-xs bg-[#2563EB] shadow-xs" />
              <span className="text-neutral-800 font-medium dark:text-zinc-200">
                Specialty Track / Roadmap
              </span>
            </div>
          </div>
        </div>

        {/* Path / Edge types */}
        <div className="border-t border-neutral-200/80 pt-2.5 dark:border-zinc-800/80">
          <span className="font-semibold text-neutral-500 dark:text-zinc-400">
            Path Connections
          </span>
          <div className="mt-2 space-y-2">
            <div className="flex items-center gap-2.5">
              <svg width="32" height="6" className="shrink-0">
                <line
                  x1="0"
                  y1="3"
                  x2="32"
                  y2="3"
                  stroke="#2563EB"
                  strokeWidth="2.5"
                />
              </svg>
              <span className="text-neutral-700 dark:text-zinc-300">
                <strong>Solid Blue:</strong> Core Progression
              </span>
            </div>

            <div className="flex items-center gap-2.5">
              <svg width="32" height="6" className="shrink-0">
                <line
                  x1="0"
                  y1="3"
                  x2="32"
                  y2="3"
                  stroke="#2563EB"
                  strokeWidth="2"
                  strokeDasharray="4 3"
                />
              </svg>
              <span className="text-neutral-700 dark:text-zinc-300">
                <strong>Dotted Blue:</strong> Branch / Recommended Options
              </span>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
