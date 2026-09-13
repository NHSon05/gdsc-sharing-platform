import React from "react";
import { VisualRoadmapView } from "@/features/roadmap";

export const metadata = {
  title: "Roadmap Sơ đồ — GDSC Sharing Platform",
  description: "Interactive visual roadmap node graph and study guides.",
};

export default async function RoadmapDetailPage({
  params,
}: {
  params: Promise<{ id: string }>;
}) {
  const { id } = await params;
  return <VisualRoadmapView slugOrId={id} />;
}
