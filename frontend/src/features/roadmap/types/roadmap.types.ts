export type RoadmapStatus = "Draft" | "Published" | "Archived";

export type RoadmapLevel =
  | "Beginner"
  | "Intermediate"
  | "Advanced"
  | "AllLevels";

export type RoadmapNodeType = "Topic" | "Group" | "Milestone";

export type RoadmapRelationType = "Required" | "Recommended" | "Optional";

export type RoadmapLineStyle = "Solid" | "Dashed";

export type ResourceType = "Link" | "File";

export interface PageResponse<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalItems: number;
  totalPages: number;
}

export interface CategorySummary {
  id: string;
  name: string;
}

export interface CategoryResponse {
  id: string;
  name: string;
  slug: string;
  description: string | null;
  icon: string | null;
  color: string | null;
  sortOrder: number;
  isActive: boolean;
}

export interface RoadmapSummary {
  id: string;
  title: string;
  slug: string;
  shortDescription: string;
  thumbnailUrl: string | null;
  category: CategorySummary;
  level: RoadmapLevel;
  estimatedDuration: string | null;
  status: RoadmapStatus;
  sortOrder: number;
  nodeCount: number;
  sectionCount?: number;
}

export interface NodePosition {
  x: number;
  y: number;
}

export interface RoadmapNodeDto {
  id: string;
  title: string;
  slug: string;
  description: string | null;
  nodeType: RoadmapNodeType;
  position: NodePosition;
  width: number | null;
  color: string | null;
  icon: string | null;
  sortOrder: number;
  isActive: boolean;
  resourceCount: number;
}

export interface RoadmapEdgeDto {
  id: string;
  sourceNodeId: string;
  targetNodeId: string;
  relationType: RoadmapRelationType;
  lineStyle: RoadmapLineStyle;
  label: string | null;
  sortOrder: number;
  isActive: boolean;
}

export interface RoadmapResponse {
  id: string;
  title: string;
  slug: string;
  shortDescription: string;
  description: string | null;
  thumbnailUrl: string | null;
  category: CategorySummary;
  level: RoadmapLevel;
  estimatedDuration: string | null;
  prerequisites: string | null;
  status: RoadmapStatus;
  sortOrder: number;
  publishedAtUtc: string | null;
  nodes: RoadmapNodeDto[];
  edges: RoadmapEdgeDto[];
}

export interface LearningResourceDto {
  id: string;
  title: string;
  description: string | null;
  resourceType: ResourceType;
  externalUrl: string | null;
  originalFileName: string | null;
  fileSize: number | null;
  contentType: string | null;
  downloadUrl: string | null;
  sortOrder: number;
  isActive: boolean;
}

export interface NodeReferenceDto {
  id: string;
  title: string;
  slug: string;
  relationType: RoadmapRelationType;
}

export interface NodeDetailResponse {
  node: RoadmapNodeDto;
  learningObjectives: string | null;
  estimatedDuration: string | null;
  resources: LearningResourceDto[];
  prerequisites: NodeReferenceDto[];
  nextNodes: NodeReferenceDto[];
}

export interface RoadmapQueryParams {
  search?: string;
  categoryId?: string;
  level?: RoadmapLevel | string;
  status?: RoadmapStatus;
  page?: number;
  pageSize?: number;
}

/* ==========================================================================
   ADMIN REQUEST MODELS
   ========================================================================== */

export interface CreateRoadmapRequest {
  title: string;
  slug: string;
  shortDescription: string;
  description?: string | null;
  categoryId: string;
  level: RoadmapLevel;
  estimatedDuration?: string | null;
  prerequisites?: string | null;
  sortOrder?: number;
}

export type UpdateRoadmapRequest = CreateRoadmapRequest;

export interface UpdateRoadmapStatusRequest {
  status: RoadmapStatus;
}

export interface CreateNodeRequest {
  title: string;
  slug: string;
  description?: string | null;
  learningObjectives?: string | null;
  estimatedDuration?: string | null;
  nodeType: RoadmapNodeType;
  positionX: number;
  positionY: number;
  color?: string | null;
  icon?: string | null;
  sortOrder?: number;
}

export type UpdateNodeRequest = CreateNodeRequest;

export interface UpdateNodeStatusRequest {
  isActive: boolean;
}

export interface NodePositionItem {
  id: string;
  positionX: number;
  positionY: number;
}

export interface UpdateNodePositionsRequest {
  nodes: NodePositionItem[];
}

export interface CreateEdgeRequest {
  sourceNodeId: string;
  targetNodeId: string;
  relationType: RoadmapRelationType;
  lineStyle: RoadmapLineStyle;
  label?: string | null;
  sortOrder?: number;
}

export interface CreateLinkResourceRequest {
  title: string;
  description?: string | null;
  externalUrl: string;
  sortOrder?: number;
}
