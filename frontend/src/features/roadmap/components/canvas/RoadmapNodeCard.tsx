"use client";

import React from "react";
import type { RoadmapNodeDto } from "../../types/roadmap.types";
import { Flag, Layers, BookOpen, GripVertical, Check } from "lucide-react";

interface RoadmapNodeCardProps {
  node: RoadmapNodeDto;
  isSelected: boolean;
  isHighlighted: boolean;
  isEditMode: boolean;
  onClick: (node: RoadmapNodeDto) => void;
  onDragStart?: (e: React.MouseEvent, node: RoadmapNodeDto) => void;
}

function renderBadge(icon?: string | null) {
  if (!icon) return null;

  switch (icon) {
    case "check-purple":
      return (
        <div
          className="flex size-4 shrink-0 items-center justify-center rounded-full bg-[#8B5CF6] text-white shadow-xs"
          title="Personal Recommendation"
        >
          <Check className="size-2.5 stroke-[3]" />
        </div>
      );
    case "check-green":
      return (
        <div
          className="flex size-4 shrink-0 items-center justify-center rounded-full bg-[#10B981] text-white shadow-xs"
          title="Alternative Option"
        >
          <Check className="size-2.5 stroke-[3]" />
        </div>
      );
    case "check-gray":
      return (
        <div
          className="flex size-4 shrink-0 items-center justify-center rounded-full bg-[#6B7280] text-white shadow-xs"
          title="Order not strict on roadmap"
        >
          <Check className="size-2.5 stroke-[3]" />
        </div>
      );
    default:
      return null;
  }
}

export function RoadmapNodeCard({
  node,
  isSelected,
  isHighlighted,
  isEditMode,
  onClick,
  onDragStart,
}: RoadmapNodeCardProps) {
  const nodeWidth = node.width ? Number(node.width) : 190;
  const nodeColor = (node.color || "").toUpperCase();
  const badge = renderBadge(node.icon);

  // 1. Milestone Node
  if (node.nodeType === "Milestone") {
    return (
      <div
        id={`node-${node.id}`}
        style={{
          position: "absolute",
          left: `${node.position.x}px`,
          top: `${node.position.y}px`,
          width: `${nodeWidth}px`,
          cursor: isEditMode ? "grab" : "pointer",
        }}
        onClick={(e) => {
          e.stopPropagation();
          onClick(node);
        }}
        onMouseDown={(e) => {
          if (isEditMode && onDragStart) {
            e.stopPropagation();
            onDragStart(e, node);
          }
        }}
        className={`group relative flex items-center gap-2.5 rounded-xl border-2 border-black bg-amber-400 px-3.5 py-3 shadow-sm transition-all duration-150 select-none ${
          !node.isActive ? "ring-dashed opacity-50 ring-1 ring-neutral-400" : ""
        } ${
          isSelected
            ? "z-20 scale-105 shadow-md ring-4 ring-blue-500/60"
            : isHighlighted
              ? "z-10 ring-2 ring-blue-500/40"
              : "hover:-translate-y-0.5 hover:shadow-md"
        }`}
      >
        {isEditMode && (
          <GripVertical className="size-4 shrink-0 cursor-grab text-black/60 active:cursor-grabbing" />
        )}
        <div className="flex size-7 shrink-0 items-center justify-center rounded-lg bg-black text-white shadow-xs">
          <Flag className="size-3.5 fill-white" />
        </div>
        <div className="min-w-0 flex-1">
          <h4 className="truncate text-xs font-black text-black">
            {node.title}
          </h4>
        </div>
      </div>
    );
  }

  // 2. Group Node
  if (node.nodeType === "Group") {
    return (
      <div
        id={`node-${node.id}`}
        style={{
          position: "absolute",
          left: `${node.position.x}px`,
          top: `${node.position.y}px`,
          width: `${Math.max(nodeWidth, 160)}px`,
          cursor: isEditMode ? "grab" : "pointer",
        }}
        onClick={(e) => {
          e.stopPropagation();
          onClick(node);
        }}
        onMouseDown={(e) => {
          if (isEditMode && onDragStart) {
            e.stopPropagation();
            onDragStart(e, node);
          }
        }}
        className={`group relative rounded-xl border-2 border-neutral-900 bg-white/95 p-3 shadow-xs transition-all duration-150 select-none dark:bg-zinc-900 ${
          !node.isActive ? "opacity-50" : ""
        } ${
          isSelected
            ? "z-20 scale-105 shadow-md ring-4 ring-blue-500/60"
            : isHighlighted
              ? "z-10 ring-2 ring-blue-500/40"
              : "hover:border-black hover:shadow-sm"
        }`}
      >
        <div className="flex items-center justify-between gap-1.5">
          <div className="flex items-center gap-1.5">
            <Layers className="size-3.5 text-neutral-600 dark:text-zinc-400" />
            <span className="text-[11px] font-bold tracking-tight text-neutral-800 uppercase dark:text-zinc-200">
              {node.title}
            </span>
          </div>
          {badge}
        </div>
      </div>
    );
  }

  // 3. Blue Specialty Roadmap Node (e.g. Prompt Engineering, AI Agents Roadmap, TypeScript)
  if (nodeColor === "#2563EB" || nodeColor === "#3B82F6") {
    return (
      <div
        id={`node-${node.id}`}
        style={{
          position: "absolute",
          left: `${node.position.x}px`,
          top: `${node.position.y}px`,
          width: `${nodeWidth}px`,
          cursor: isEditMode ? "grab" : "pointer",
        }}
        onClick={(e) => {
          e.stopPropagation();
          onClick(node);
        }}
        onMouseDown={(e) => {
          if (isEditMode && onDragStart) {
            e.stopPropagation();
            onDragStart(e, node);
          }
        }}
        className={`group relative flex h-[40px] items-center justify-center rounded-md border border-blue-700 bg-[#2563EB] px-3 text-center text-white shadow-xs transition-all duration-150 select-none ${
          !node.isActive ? "ring-dashed opacity-50 ring-1 ring-neutral-400" : ""
        } ${
          isSelected
            ? "z-20 scale-105 shadow-md ring-4 ring-amber-400/70"
            : isHighlighted
              ? "z-10 ring-2 ring-amber-400/50"
              : "hover:-translate-y-0.5 hover:shadow-md"
        }`}
      >
        {isEditMode && (
          <GripVertical className="absolute left-1.5 size-3.5 shrink-0 cursor-grab text-white/60 active:cursor-grabbing" />
        )}
        <span className="truncate text-xs font-bold tracking-tight text-white">
          {node.title}
        </span>
      </div>
    );
  }

  // 4. Sub-Topic Pale Yellow / Cream Node (e.g. How does the internet work?, npm, React, Vite, etc.)
  if (
    nodeColor === "#FFF4C2" ||
    nodeColor === "#FFEBB2" ||
    nodeColor === "#FFF2B2"
  ) {
    return (
      <div
        id={`node-${node.id}`}
        style={{
          position: "absolute",
          left: `${node.position.x}px`,
          top: `${node.position.y}px`,
          width: `${nodeWidth}px`,
          cursor: isEditMode ? "grab" : "pointer",
        }}
        onClick={(e) => {
          e.stopPropagation();
          onClick(node);
        }}
        onMouseDown={(e) => {
          if (isEditMode && onDragStart) {
            e.stopPropagation();
            onDragStart(e, node);
          }
        }}
        className={`group relative flex h-[38px] items-center justify-between gap-2 rounded-md border-[1.5px] border-black bg-[#FFF4C2] px-2.5 shadow-2xs transition-all duration-150 select-none ${
          !node.isActive ? "ring-dashed opacity-50 ring-1 ring-neutral-400" : ""
        } ${
          isSelected
            ? "z-20 scale-105 shadow-md ring-4 ring-blue-500/60"
            : isHighlighted
              ? "z-10 ring-2 ring-blue-500/40"
              : "hover:-translate-y-0.5 hover:shadow-md"
        }`}
      >
        {isEditMode && (
          <GripVertical className="size-3 shrink-0 cursor-grab text-black/50 active:cursor-grabbing" />
        )}

        <span className="flex-1 truncate text-center text-[12px] leading-tight font-semibold text-black">
          {node.title}
        </span>

        {/* Status / Recommendation Badge */}
        {badge && <div className="shrink-0">{badge}</div>}

        {/* Resources Indicator if available */}
        {node.resourceCount > 0 && !badge && (
          <span
            className="flex shrink-0 items-center gap-0.5 rounded-full bg-black/10 px-1.5 py-0.5 text-[9px] font-bold text-black"
            title={`${node.resourceCount} resources`}
          >
            <BookOpen className="size-2.5 text-black" />
            <span>{node.resourceCount}</span>
          </span>
        )}
      </div>
    );
  }

  // 5. Main Spine Core Yellow Topic Node (Default roadmap.sh Main Box: Internet, HTML, CSS, JavaScript...)
  return (
    <div
      id={`node-${node.id}`}
      style={{
        position: "absolute",
        left: `${node.position.x}px`,
        top: `${node.position.y}px`,
        width: `${nodeWidth}px`,
        cursor: isEditMode ? "grab" : "pointer",
      }}
      onClick={(e) => {
        e.stopPropagation();
        onClick(node);
      }}
      onMouseDown={(e) => {
        if (isEditMode && onDragStart) {
          e.stopPropagation();
          onDragStart(e, node);
        }
      }}
      className={`group relative flex h-[44px] items-center justify-center rounded-md border-2 border-black bg-[#FFE600] px-3 text-center shadow-xs transition-all duration-150 select-none ${
        !node.isActive ? "ring-dashed opacity-50 ring-1 ring-neutral-400" : ""
      } ${
        isSelected
          ? "z-20 scale-105 shadow-md ring-4 ring-blue-500/60"
          : isHighlighted
            ? "z-10 ring-2 ring-blue-500/40"
            : "hover:-translate-y-0.5 hover:shadow-md"
      }`}
    >
      {isEditMode && (
        <GripVertical className="absolute left-2 size-3.5 shrink-0 cursor-grab text-black/60 active:cursor-grabbing" />
      )}

      <span className="truncate text-xs font-black tracking-tight text-black uppercase sm:text-[13px]">
        {node.title}
      </span>

      {/* Right-aligned badge if present (e.g. on GraphQL, PWAs, Mobile Apps) */}
      {badge && <div className="absolute right-2 shrink-0">{badge}</div>}

      {/* Resource Count Badge */}
      {node.resourceCount > 0 && (
        <span
          className="absolute -top-2 -right-2 flex size-4.5 items-center justify-center rounded-full bg-black text-[9px] font-bold text-white shadow-xs"
          title={`${node.resourceCount} resources`}
        >
          {node.resourceCount}
        </span>
      )}
    </div>
  );
}
