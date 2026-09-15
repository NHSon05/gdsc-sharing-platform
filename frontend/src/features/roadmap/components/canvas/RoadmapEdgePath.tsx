"use client";

import React from "react";
import type { RoadmapEdgeDto, RoadmapNodeDto } from "../../types/roadmap.types";

interface RoadmapEdgePathProps {
  edge: RoadmapEdgeDto;
  sourceNode?: RoadmapNodeDto;
  targetNode?: RoadmapNodeDto;
  isHighlighted: boolean;
  isEditMode: boolean;
  onDeleteEdge?: (edgeId: string) => void;
}

function getNodeDimensions(node: RoadmapNodeDto) {
  const width = node.width
    ? Number(node.width)
    : node.nodeType === "Group"
      ? 220
      : 180;
  const color = (node.color || "").toUpperCase();
  const isTopic = !node.color || color === "#FFE600";
  const isBlue = color === "#2563EB" || color === "#3B82F6";
  const height =
    node.nodeType === "Milestone" ? 72 : isTopic ? 44 : isBlue ? 40 : 38;
  return { width, height };
}

export function RoadmapEdgePath({
  edge,
  sourceNode,
  targetNode,
  isHighlighted,
  isEditMode,
  onDeleteEdge,
}: RoadmapEdgePathProps) {
  if (!sourceNode || !targetNode) return null;

  const sDim = getNodeDimensions(sourceNode);
  const tDim = getNodeDimensions(targetNode);

  const sCenter = {
    x: sourceNode.position.x + sDim.width / 2,
    y: sourceNode.position.y + sDim.height / 2,
  };
  const tCenter = {
    x: targetNode.position.x + tDim.width / 2,
    y: targetNode.position.y + tDim.height / 2,
  };

  const dx = tCenter.x - sCenter.x;
  const dy = tCenter.y - sCenter.y;

  let startX = 0;
  let startY = 0;
  let endX = 0;
  let endY = 0;
  let pathData = "";

  // Check if nodes are vertically aligned (e.g. spine topic sequence)
  const isVerticallyAligned = Math.abs(dx) <= 36;
  const isHorizontalBranch = !isVerticallyAligned;

  if (isVerticallyAligned) {
    // 1. Straight vertical spine connector
    const alignedX = Math.round((sCenter.x + tCenter.x) / 2);
    if (dy >= 0) {
      startX = alignedX;
      startY = sourceNode.position.y + sDim.height;
      endX = alignedX;
      endY = targetNode.position.y;
    } else {
      startX = alignedX;
      startY = sourceNode.position.y;
      endX = alignedX;
      endY = targetNode.position.y + tDim.height;
    }
    // Crisp straight line down the spine axis
    pathData = `M ${startX} ${startY} L ${endX} ${endY}`;
  } else if (isHorizontalBranch) {
    // 2. Horizontal branch connector (Spine to side boxes or column-to-column)
    if (dx > 0) {
      // Target is to the right
      startX = sourceNode.position.x + sDim.width;
      startY = sCenter.y;
      endX = targetNode.position.x;
      endY = tCenter.y;
      const span = Math.max(endX - startX, 20);
      const offset = Math.min(Math.max(span * 0.45, 20), 80);
      const c1x = startX + offset;
      const c1y = startY;
      const c2x = endX - offset;
      const c2y = endY;
      pathData = `M ${startX} ${startY} C ${c1x} ${c1y}, ${c2x} ${c2y}, ${endX} ${endY}`;
    } else {
      // Target is to the left
      startX = sourceNode.position.x;
      startY = sCenter.y;
      endX = targetNode.position.x + tDim.width;
      endY = tCenter.y;
      const span = Math.max(startX - endX, 20);
      const offset = Math.min(Math.max(span * 0.45, 20), 80);
      const c1x = startX - offset;
      const c1y = startY;
      const c2x = endX + offset;
      const c2y = endY;
      pathData = `M ${startX} ${startY} C ${c1x} ${c1y}, ${c2x} ${c2y}, ${endX} ${endY}`;
    }
  }
  const midX = (startX + endX) / 2;
  const midY = (startY + endY) / 2;

  const isDashed =
    edge.lineStyle === "Dashed" ||
    edge.relationType === "Recommended" ||
    edge.relationType === "Optional";

  const strokeColor = isHighlighted ? "#1D4ED8" : "#2563EB";
  const strokeWidth = isHighlighted ? 3.5 : isDashed ? 2 : 2.5;
  const markerId = isHighlighted ? "arrowhead-highlight" : "arrowhead-default";

  return (
    <g className="transition-all duration-200">
      {/* Invisible wider hit-area for easier interaction */}
      <path
        d={pathData}
        fill="none"
        stroke="transparent"
        strokeWidth={14}
        className="cursor-pointer"
      />

      {/* Rendered SVG Path */}
      <path
        d={pathData}
        fill="none"
        stroke={strokeColor}
        strokeWidth={strokeWidth}
        strokeDasharray={isDashed ? "6 5" : undefined}
        markerEnd={`url(#${markerId})`}
        className="transition-colors duration-200"
      />

      {/* Optional Label */}
      {edge.label && (
        <g transform={`translate(${midX}, ${midY})`}>
          <rect
            x={-35}
            y={-10}
            width={70}
            height={20}
            rx={10}
            fill="white"
            className="dark:fill-zinc-800"
            stroke="#CBD5E1"
            strokeWidth={1}
          />
          <text
            x={0}
            y={3}
            textAnchor="middle"
            className="fill-neutral-600 text-[10px] font-semibold select-none dark:fill-zinc-300"
          >
            {edge.label}
          </text>
        </g>
      )}

      {/* Admin Delete Edge Button in Edit Mode */}
      {isEditMode && onDeleteEdge && (
        <g
          transform={`translate(${midX}, ${midY})`}
          className="group cursor-pointer"
          onClick={(e) => {
            e.stopPropagation();
            onDeleteEdge(edge.id);
          }}
        >
          <circle
            r={10}
            className="fill-rose-500 shadow-sm transition-all hover:fill-rose-600"
          />
          <text
            x={0}
            y={3.5}
            textAnchor="middle"
            className="pointer-events-none fill-white text-[11px] font-bold select-none"
          >
            ×
          </text>
        </g>
      )}
    </g>
  );
}
