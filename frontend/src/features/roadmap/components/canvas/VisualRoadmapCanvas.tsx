"use client";

import React, { useState, useRef, useEffect, useCallback, useMemo } from "react";
import type {
  RoadmapResponse,
  RoadmapNodeDto,
} from "../../types/roadmap.types";
import { RoadmapNodeCard } from "./RoadmapNodeCard";
import { RoadmapEdgePath } from "./RoadmapEdgePath";
import { CanvasControls } from "./CanvasControls";
import { CanvasLegend } from "./CanvasLegend";
import { Search, X } from "lucide-react";

interface VisualRoadmapCanvasProps {
  roadmap: RoadmapResponse;
  isAdmin: boolean;
  selectedNodeId: string | null;
  onSelectNode: (node: RoadmapNodeDto | null) => void;
  onSavePositions: (
    updatedNodes: { id: string; positionX: number; positionY: number }[]
  ) => Promise<void>;
  isSavingPositions: boolean;
  onOpenAddNode: () => void;
  onOpenConnectNodes: () => void;
  onDeleteEdge: (edgeId: string) => void;
}

export function VisualRoadmapCanvas({
  roadmap,
  isAdmin,
  selectedNodeId,
  onSelectNode,
  onSavePositions,
  isSavingPositions,
  onOpenAddNode,
  onOpenConnectNodes,
  onDeleteEdge,
}: VisualRoadmapCanvasProps) {
  const containerRef = useRef<HTMLDivElement>(null);

  // Local position overrides for node drag & drop
  const [positionOverrides, setPositionOverrides] = useState<
    Record<string, { x: number; y: number }>
  >({});

  // Canvas viewport coordinates
  const [pan, setPan] = useState<{ x: number; y: number }>({ x: 80, y: 80 });
  const [zoom, setZoom] = useState<number>(1.0);

  // Search & Navigation
  const [nodeSearch, setNodeSearch] = useState("");
  const [isLegendOpen, setIsLegendOpen] = useState(false);
  const [isEditMode, setIsEditMode] = useState(false);

  // Drag interaction states
  const [isPanning, setIsPanning] = useState(false);
  const panStartRef = useRef<{ x: number; y: number }>({ x: 0, y: 0 });

  const draggingNodeRef = useRef<{
    id: string;
    startMouseX: number;
    startMouseY: number;
    startNodeX: number;
    startNodeY: number;
  } | null>(null);

  // Merge server nodes with local dragged position overrides
  const nodes = useMemo(() => {
    return roadmap.nodes.map((node) => {
      const override = positionOverrides[node.id];
      return override ? { ...node, position: override } : node;
    });
  }, [roadmap.nodes, positionOverrides]);

  const hasUnsavedChanges = Object.keys(positionOverrides).length > 0;

  // Compute node lookup map for quick edge rendering
  const nodeMap = useMemo(() => {
    const map = new Map<string, RoadmapNodeDto>();
    nodes.forEach((n) => map.set(n.id, n));
    return map;
  }, [nodes]);

  // Highlighted nodes/edges based on search or selection
  const matchingNodeIds = useMemo(() => {
    if (!nodeSearch.trim()) return new Set<string>();
    const query = nodeSearch.trim().toLowerCase();
    const matches = new Set<string>();
    nodes.forEach((n) => {
      if (n.title.toLowerCase().includes(query)) {
        matches.add(n.id);
      }
    });
    return matches;
  }, [nodes, nodeSearch]);

  const selectedNodeConnectedIds = useMemo(() => {
    const set = new Set<string>();
    if (!selectedNodeId) return set;
    roadmap.edges.forEach((edge) => {
      if (edge.sourceNodeId === selectedNodeId) {
        set.add(edge.targetNodeId);
      }
      if (edge.targetNodeId === selectedNodeId) {
        set.add(edge.sourceNodeId);
      }
    });
    return set;
  }, [roadmap.edges, selectedNodeId]);

  // Fit View algorithm
  const handleFitView = useCallback(() => {
    if (!containerRef.current || nodes.length === 0) return;
    const rect = containerRef.current.getBoundingClientRect();
    const padding = 80;

    let minX = Infinity;
    let minY = Infinity;
    let maxX = -Infinity;
    let maxY = -Infinity;

    nodes.forEach((n) => {
      const width = n.nodeType === "Group" ? 220 : n.width ? Number(n.width) : 190;
      const height = n.nodeType === "Milestone" ? 72 : 50;
      minX = Math.min(minX, n.position.x);
      minY = Math.min(minY, n.position.y);
      maxX = Math.max(maxX, n.position.x + width);
      maxY = Math.max(maxY, n.position.y + height);
    });

    const graphWidth = maxX - minX;
    const graphHeight = maxY - minY;

    if (graphWidth <= 0 || graphHeight <= 0) return;

    const availableWidth = rect.width - padding * 2;
    const availableHeight = rect.height - padding * 2;

    const scaleX = availableWidth / graphWidth;
    const scaleY = availableHeight / graphHeight;
    let newZoom = Math.min(scaleX, scaleY);

    if (newZoom < 0.65) {
      // Long vertical roadmap like roadmap.sh: keep text legible and focus top-center
      newZoom = 0.82;
      const newPanX = (rect.width - graphWidth * newZoom) / 2 - minX * newZoom;
      const newPanY = 40 - minY * newZoom;
      setZoom(newZoom);
      setPan({ x: Math.round(newPanX), y: Math.round(newPanY) });
      return;
    }

    newZoom = Math.min(Math.max(newZoom, 0.4), 1.2);
    const newPanX = (rect.width - graphWidth * newZoom) / 2 - minX * newZoom;
    const newPanY = (rect.height - graphHeight * newZoom) / 2 - minY * newZoom;

    setZoom(newZoom);
    setPan({ x: Math.round(newPanX), y: Math.round(newPanY) });
  }, [nodes]);

  // Initial fit view on mount
  useEffect(() => {
    const timer = setTimeout(() => {
      handleFitView();
    }, 150);
    return () => clearTimeout(timer);
  }, [handleFitView]);

  // Reset View to start/center
  const handleResetView = () => {
    if (nodes.length > 0) {
      const startNode = [...nodes].sort((a, b) => a.sortOrder - b.sortOrder)[0];
      if (startNode && containerRef.current) {
        const rect = containerRef.current.getBoundingClientRect();
        setZoom(1.0);
        setPan({
          x: Math.round(rect.width / 2 - startNode.position.x - 100),
          y: Math.round(rect.height / 2 - startNode.position.y - 30),
        });
        return;
      }
    }
    setPan({ x: 80, y: 80 });
    setZoom(1.0);
  };

  // Zoom helpers
  const handleZoomIn = () => {
    setZoom((prev) => Math.min(prev + 0.15, 2.2));
  };

  const handleZoomOut = () => {
    setZoom((prev) => Math.max(prev - 0.15, 0.35));
  };

  // Jump to searched node
  const handleJumpToNode = (targetNode: RoadmapNodeDto) => {
    if (!containerRef.current) return;
    const rect = containerRef.current.getBoundingClientRect();
    const targetZoom = Math.max(zoom, 0.9);
    setZoom(targetZoom);
    setPan({
      x: Math.round(rect.width / 2 - targetNode.position.x * targetZoom - 100 * targetZoom),
      y: Math.round(rect.height / 2 - targetNode.position.y * targetZoom - 30 * targetZoom),
    });
    onSelectNode(targetNode);
  };

  // Search submit
  const handleSearchSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (!nodeSearch.trim()) return;
    const query = nodeSearch.trim().toLowerCase();
    const match = nodes.find((n) => n.title.toLowerCase().includes(query));
    if (match) {
      handleJumpToNode(match);
    }
  };

  // Mouse wheel: handles zoom with cursor pivot and touchpad pan
  const handleWheel = (e: React.WheelEvent) => {
    e.preventDefault();
    if (!containerRef.current) return;

    if (e.ctrlKey || e.metaKey) {
      const zoomFactor = -e.deltaY * 0.002;
      const newZoom = Math.min(Math.max(zoom + zoomFactor, 0.35), 2.2);

      const rect = containerRef.current.getBoundingClientRect();
      const mouseX = e.clientX - rect.left;
      const mouseY = e.clientY - rect.top;

      const newPanX = mouseX - (mouseX - pan.x) * (newZoom / zoom);
      const newPanY = mouseY - (mouseY - pan.y) * (newZoom / zoom);

      setZoom(newZoom);
      setPan({ x: Math.round(newPanX), y: Math.round(newPanY) });
    } else {
      setPan((prev) => ({
        x: Math.round(prev.x - e.deltaX),
        y: Math.round(prev.y - e.deltaY),
      }));
    }
  };

  // Canvas Pan Dragging
  const handleCanvasMouseDown = (e: React.MouseEvent) => {
    if (e.button !== 0) return;
    setIsPanning(true);
    panStartRef.current = {
      x: e.clientX - pan.x,
      y: e.clientY - pan.y,
    };
  };

  // Node Dragging Start
  const handleNodeDragStart = (e: React.MouseEvent, node: RoadmapNodeDto) => {
    if (!isEditMode) return;
    draggingNodeRef.current = {
      id: node.id,
      startMouseX: e.clientX,
      startMouseY: e.clientY,
      startNodeX: node.position.x,
      startNodeY: node.position.y,
    };
  };

  // Global mouse move & mouse up for smooth dragging
  useEffect(() => {
    const handleMouseMove = (e: MouseEvent) => {
      if (draggingNodeRef.current) {
        const { id, startMouseX, startMouseY, startNodeX, startNodeY } =
          draggingNodeRef.current;
        const dx = (e.clientX - startMouseX) / zoom;
        const dy = (e.clientY - startMouseY) / zoom;

        // Snap to grid of 10px
        const newX = Math.round((startNodeX + dx) / 10) * 10;
        const newY = Math.round((startNodeY + dy) / 10) * 10;

        setPositionOverrides((prev) => ({
          ...prev,
          [id]: { x: newX, y: newY },
        }));
        return;
      }

      if (isPanning) {
        setPan({
          x: Math.round(e.clientX - panStartRef.current.x),
          y: Math.round(e.clientY - panStartRef.current.y),
        });
      }
    };

    const handleMouseUp = () => {
      setIsPanning(false);
      draggingNodeRef.current = null;
    };

    window.addEventListener("mousemove", handleMouseMove);
    window.addEventListener("mouseup", handleMouseUp);

    return () => {
      window.removeEventListener("mousemove", handleMouseMove);
      window.removeEventListener("mouseup", handleMouseUp);
    };
  }, [isPanning, zoom]);

  // Handle Save Positions
  const handleSavePositions = async () => {
    const ids = Object.keys(positionOverrides);
    if (ids.length === 0) return;
    const payload = ids.map((id) => ({
      id,
      positionX: positionOverrides[id].x,
      positionY: positionOverrides[id].y,
    }));

    await onSavePositions(payload);
    setPositionOverrides({});
  };

  return (
    <div
      ref={containerRef}
      onMouseDown={handleCanvasMouseDown}
      onWheel={handleWheel}
      className={`relative h-full w-full overflow-hidden select-none ${
        isPanning
          ? "cursor-grabbing"
          : isEditMode
          ? "cursor-default"
          : "cursor-grab"
      }`}
      style={{
        backgroundColor: "var(--background)",
        backgroundImage: `radial-gradient(circle, rgba(148, 163, 184, 0.2) 1px, transparent 1px)`,
        backgroundSize: `${24 * zoom}px ${24 * zoom}px`,
        backgroundPosition: `${pan.x}px ${pan.y}px`,
      }}
    >
      {/* Search Bar inside Canvas */}
      <div className="absolute top-4 left-4 z-30 flex items-center gap-2 max-w-sm">
        <form onSubmit={handleSearchSubmit} className="relative flex-1">
          <Search className="absolute left-3 top-1/2 size-3.5 -translate-y-1/2 text-neutral-400 dark:text-zinc-500" />
          <input
            type="text"
            value={nodeSearch}
            onChange={(e) => setNodeSearch(e.target.value)}
            placeholder="Search topic or milestone..."
            className="h-9 w-64 rounded-xl border border-neutral-200/90 bg-white/90 pl-8 pr-8 text-xs text-neutral-900 shadow-md backdrop-blur-md transition-all placeholder:text-neutral-400 focus:w-72 focus:border-brand focus:outline-none focus:ring-2 focus:ring-brand/20 dark:border-zinc-800 dark:bg-zinc-900/90 dark:text-zinc-100 dark:placeholder:text-zinc-500"
          />
          {nodeSearch && (
            <button
              type="button"
              onClick={() => setNodeSearch("")}
              className="absolute right-2.5 top-1/2 -translate-y-1/2 text-neutral-400 hover:text-neutral-600 dark:text-zinc-500 dark:hover:text-zinc-300"
            >
              <X className="size-3.5" />
            </button>
          )}
        </form>

        {/* Quick jump button */}
        {matchingNodeIds.size > 0 && (
          <button
            type="button"
            onClick={() => {
              const firstMatch = nodes.find((n) => matchingNodeIds.has(n.id));
              if (firstMatch) handleJumpToNode(firstMatch);
            }}
            className="rounded-full bg-brand px-2.5 py-1 text-[11px] font-semibold text-white shadow-xs hover:bg-brand-hover transition-colors"
            title="Jump to matching node"
          >
            {matchingNodeIds.size} {matchingNodeIds.size === 1 ? "match" : "matches"}
          </button>
        )}
      </div>

      {/* Canvas Controls Floating Bar */}
      <CanvasControls
        zoom={zoom}
        onZoomIn={handleZoomIn}
        onZoomOut={handleZoomOut}
        onFitView={handleFitView}
        onResetView={handleResetView}
        isLegendOpen={isLegendOpen}
        onToggleLegend={() => setIsLegendOpen(!isLegendOpen)}
        isAdmin={isAdmin}
        isEditMode={isEditMode}
        onToggleEditMode={() => setIsEditMode(!isEditMode)}
        hasUnsavedChanges={hasUnsavedChanges}
        isSavingPositions={isSavingPositions}
        onSavePositions={handleSavePositions}
        onOpenAddNode={onOpenAddNode}
        onOpenConnectNodes={onOpenConnectNodes}
      />

      {/* Floating Legend */}
      <CanvasLegend
        isOpen={isLegendOpen}
        onClose={() => setIsLegendOpen(false)}
      />

      {/* The Interactive Transformed World Layer */}
      <div
        style={{
          transform: `translate(${pan.x}px, ${pan.y}px) scale(${zoom})`,
          transformOrigin: "0 0",
          width: "10000px",
          height: "10000px",
          position: "absolute",
          top: 0,
          left: 0,
        }}
      >
        {/* SVG Layer for Directed Arrows */}
        <svg
          className="absolute inset-0 pointer-events-none"
          style={{ width: "100%", height: "100%", overflow: "visible" }}
        >
          <defs>
            {/* Default arrow */}
            <marker
              id="arrowhead-default"
              viewBox="0 0 10 10"
              refX="8"
              refY="5"
              markerWidth="6"
              markerHeight="6"
              orient="auto-start-reverse"
            >
              <path d="M 0 1.5 L 8 5 L 0 8.5 z" fill="#2563EB" />
            </marker>

            {/* Dashed / Optional arrow */}
            <marker
              id="arrowhead-dashed"
              viewBox="0 0 10 10"
              refX="8"
              refY="5"
              markerWidth="6"
              markerHeight="6"
              orient="auto-start-reverse"
            >
              <path d="M 0 1.5 L 8 5 L 0 8.5 z" fill="#2563EB" />
            </marker>

            {/* Highlighted arrow */}
            <marker
              id="arrowhead-highlight"
              viewBox="0 0 10 10"
              refX="8"
              refY="5"
              markerWidth="7"
              markerHeight="7"
              orient="auto-start-reverse"
            >
              <path d="M 0 1.5 L 8 5 L 0 8.5 z" fill="#1D4ED8" />
            </marker>
          </defs>

          {/* Render all edges */}
          {roadmap.edges.map((edge) => {
            const isHighlighted =
              edge.sourceNodeId === selectedNodeId ||
              edge.targetNodeId === selectedNodeId;

            return (
              <RoadmapEdgePath
                key={edge.id}
                edge={edge}
                sourceNode={nodeMap.get(edge.sourceNodeId)}
                targetNode={nodeMap.get(edge.targetNodeId)}
                isHighlighted={isHighlighted}
                isEditMode={isEditMode}
                onDeleteEdge={onDeleteEdge}
              />
            );
          })}
        </svg>

        {/* HTML Nodes Layer */}
        {nodes.map((node) => {
          const isSelected = selectedNodeId === node.id;
          const isHighlighted =
            matchingNodeIds.has(node.id) ||
            selectedNodeConnectedIds.has(node.id);

          return (
            <RoadmapNodeCard
              key={node.id}
              node={node}
              isSelected={isSelected}
              isHighlighted={isHighlighted}
              isEditMode={isEditMode}
              onClick={(clickedNode) => onSelectNode(clickedNode)}
              onDragStart={handleNodeDragStart}
            />
          );
        })}
      </div>
    </div>
  );
}
