"use client";

import React, { useState } from "react";
import Link from "next/link";
import { useRoadmapDetailQuery } from "../hooks/use-roadmap-detail-query";
import {
  useSaveNodePositionsMutation,
  useSetNodeStatusMutation,
  useDeleteEdgeMutation,
  useDeleteResourceMutation,
} from "../hooks/use-roadmap-admin-mutations";
import { VisualRoadmapCanvas } from "./canvas/VisualRoadmapCanvas";
import { NodeDetailDrawer } from "./drawer/NodeDetailDrawer";
import { CreateNodeDialog } from "./modals/CreateNodeDialog";
import { CreateEdgeDialog } from "./modals/CreateEdgeDialog";
import { AddResourceDialog } from "./modals/AddResourceDialog";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { useSessionStore } from "@/core/session/session.store";
import { selectCurrentUser } from "@/core/session/session.selectors";
import { isAdmin } from "@/features/auth/utils/rbac";
import type { RoadmapNodeDto } from "../types/roadmap.types";
import {
  ArrowLeft,
  Compass,
  Clock,
  Layers,
  Archive,
  FileCheck,
  RefreshCw,
} from "lucide-react";

interface VisualRoadmapViewProps {
  slugOrId: string;
}

export function VisualRoadmapView({ slugOrId }: VisualRoadmapViewProps) {
  const user = useSessionStore(selectCurrentUser);
  const userIsAdmin = isAdmin(user);

  const {
    data: roadmap,
    isLoading,
    isError,
    refetch,
  } = useRoadmapDetailQuery(slugOrId, userIsAdmin);

  // Active selected node for the drawer
  const [selectedNode, setSelectedNode] = useState<RoadmapNodeDto | null>(null);

  // Admin dialog open states
  const [isAddNodeOpen, setIsAddNodeOpen] = useState(false);
  const [isConnectOpen, setIsConnectOpen] = useState(false);
  const [resourceNodeId, setResourceNodeId] = useState<string | null>(null);

  // Mutations
  const roadmapId = roadmap?.id ?? "";
  const savePositionsMutation = useSaveNodePositionsMutation(roadmapId);
  const setNodeStatusMutation = useSetNodeStatusMutation(
    roadmapId,
    selectedNode?.id ?? ""
  );
  const deleteEdgeMutation = useDeleteEdgeMutation(roadmapId);
  const deleteResourceMutation = useDeleteResourceMutation(
    roadmapId,
    selectedNode?.id ?? ""
  );

  const handleSelectNodeById = (nodeId: string) => {
    if (!roadmap) return;
    const target = roadmap.nodes.find((n) => n.id === nodeId);
    if (target) {
      setSelectedNode(target);
    }
  };

  const handleSavePositions = async (
    updatedNodes: { id: string; positionX: number; positionY: number }[]
  ) => {
    if (!roadmap) return;
    await savePositionsMutation.mutateAsync({ nodes: updatedNodes });
  };

  const handleToggleNodeStatus = async (
    nodeId: string,
    currentActive: boolean
  ) => {
    await setNodeStatusMutation.mutateAsync({ isActive: !currentActive });
    if (selectedNode && selectedNode.id === nodeId) {
      setSelectedNode((prev) =>
        prev ? { ...prev, isActive: !currentActive } : null
      );
    }
  };

  const handleDeleteEdge = async (edgeId: string) => {
    if (window.confirm("Are you sure you want to remove this connection?")) {
      await deleteEdgeMutation.mutateAsync(edgeId);
    }
  };

  const handleDeleteResource = async (resourceId: string) => {
    if (window.confirm("Are you sure you want to remove this resource?")) {
      await deleteResourceMutation.mutateAsync(resourceId);
    }
  };

  if (isLoading) {
    return (
      <div className="flex h-[calc(100dvh-4rem)] w-full flex-col items-center justify-center gap-3">
        <div className="border-brand size-8 animate-spin rounded-full border-3 border-t-transparent" />
        <p className="text-xs font-semibold text-neutral-500 dark:text-zinc-400">
          Loading roadmap graph...
        </p>
      </div>
    );
  }

  if (isError || !roadmap) {
    return (
      <div className="mx-auto flex h-[calc(100dvh-4rem)] max-w-md flex-col items-center justify-center p-6 text-center">
        <div className="flex size-14 items-center justify-center rounded-2xl bg-rose-50 text-rose-600 dark:bg-rose-950/40 dark:text-rose-400">
          <Compass className="size-7" />
        </div>
        <h2 className="mt-4 text-xl font-bold text-neutral-900 dark:text-white">
          Roadmap Not Found
        </h2>
        <p className="mt-1.5 text-xs text-neutral-500 dark:text-zinc-400">
          The requested roadmap might be private, unpublished, or the link is
          invalid.
        </p>
        <div className="mt-6 flex items-center gap-3">
          <Link href="/roadmaps">
            <Button variant="outline" size="sm" className="rounded-full">
              Back to Roadmaps
            </Button>
          </Link>
          <Button
            variant="brand"
            size="sm"
            onClick={() => refetch()}
            className="gap-1.5 rounded-full"
          >
            <RefreshCw className="size-3.5" />
            <span>Try Again</span>
          </Button>
        </div>
      </div>
    );
  }

  return (
    <div className="relative flex h-[calc(100dvh-4rem)] w-full flex-col overflow-hidden">
      {/* Top Header Bar */}
      <header className="z-20 flex h-14 shrink-0 items-center justify-between border-b border-neutral-200/80 bg-white/90 px-4 backdrop-blur-md sm:px-6 dark:border-zinc-800/80 dark:bg-zinc-900/90">
        <div className="flex min-w-0 items-center gap-3">
          <Link
            href="/roadmaps"
            className="flex size-8 items-center justify-center rounded-xl border border-neutral-200 bg-white text-neutral-600 hover:bg-neutral-100 hover:text-neutral-900 dark:border-zinc-800 dark:bg-zinc-900 dark:text-zinc-400 dark:hover:bg-zinc-800 dark:hover:text-zinc-200"
            title="Back to all roadmaps"
          >
            <ArrowLeft className="size-4" />
          </Link>

          <div className="flex min-w-0 items-center gap-2">
            <h1 className="truncate text-sm font-bold text-neutral-900 sm:text-base dark:text-white">
              {roadmap.title}
            </h1>

            <Badge
              variant="outline"
              className="border-brand/30 bg-brand/10 text-brand hidden text-[10px] sm:inline-flex"
            >
              {roadmap.category.name}
            </Badge>

            <span className="hidden text-xs text-neutral-400 sm:inline-block">
              •
            </span>

            <span className="hidden text-xs font-medium text-neutral-500 sm:inline-block dark:text-zinc-400">
              {roadmap.level}
            </span>

            {roadmap.status === "Archived" && (
              <Badge
                variant="outline"
                className="gap-1 border-amber-300 bg-amber-50 text-[10px] text-amber-800 dark:border-amber-800 dark:bg-amber-950/30 dark:text-amber-300"
              >
                <Archive className="size-2.5" />
                Archived
              </Badge>
            )}

            {roadmap.status === "Draft" && (
              <Badge
                variant="outline"
                className="gap-1 border-neutral-300 bg-neutral-100 text-[10px] text-neutral-700 dark:border-zinc-700 dark:bg-zinc-800 dark:text-zinc-300"
              >
                <FileCheck className="size-2.5" />
                Draft
              </Badge>
            )}
          </div>
        </div>

        {/* Right meta details */}
        <div className="flex items-center gap-4 text-xs text-neutral-500 dark:text-zinc-400">
          <div className="hidden items-center gap-1.5 md:flex">
            <Layers className="text-brand size-3.5" />
            <span>{roadmap.nodes.length} nodes</span>
          </div>

          {roadmap.estimatedDuration && (
            <div className="hidden items-center gap-1.5 md:flex">
              <Clock className="size-3.5 text-amber-500" />
              <span>{roadmap.estimatedDuration}</span>
            </div>
          )}
        </div>
      </header>

      {/* Main Canvas Area */}
      <div className="relative h-full w-full flex-1 overflow-hidden">
        <VisualRoadmapCanvas
          roadmap={roadmap}
          isAdmin={userIsAdmin}
          selectedNodeId={selectedNode?.id ?? null}
          onSelectNode={(node) => setSelectedNode(node)}
          onSavePositions={handleSavePositions}
          isSavingPositions={savePositionsMutation.isPending}
          onOpenAddNode={() => setIsAddNodeOpen(true)}
          onOpenConnectNodes={() => setIsConnectOpen(true)}
          onDeleteEdge={handleDeleteEdge}
        />

        {/* Slide-over Node Detail Drawer */}
        <NodeDetailDrawer
          roadmapId={roadmap.id}
          node={selectedNode}
          isOpen={selectedNode !== null}
          onClose={() => setSelectedNode(null)}
          onSelectNodeById={handleSelectNodeById}
          isAdmin={userIsAdmin}
          onOpenAddResource={(nId) => setResourceNodeId(nId)}
          onToggleNodeStatus={handleToggleNodeStatus}
          onDeleteResource={handleDeleteResource}
        />
      </div>

      {/* Admin Modals */}
      {userIsAdmin && (
        <>
          <CreateNodeDialog
            open={isAddNodeOpen}
            onOpenChange={setIsAddNodeOpen}
            roadmapId={roadmap.id}
          />

          <CreateEdgeDialog
            open={isConnectOpen}
            onOpenChange={setIsConnectOpen}
            roadmapId={roadmap.id}
            nodes={roadmap.nodes}
          />

          {resourceNodeId && (
            <AddResourceDialog
              open={Boolean(resourceNodeId)}
              onOpenChange={(open) => {
                if (!open) setResourceNodeId(null);
              }}
              roadmapId={roadmap.id}
              nodeId={resourceNodeId}
            />
          )}
        </>
      )}
    </div>
  );
}
