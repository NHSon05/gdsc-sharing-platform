/* ==========================================================================
   SHARING FEATURE ENUMS & VALUE TYPES
   ========================================================================== */

export type SharingContentStatus =
  "Draft" | "PendingReview" | "Published" | "Rejected" | "Archived";

export type SharingScheduleStatus =
  "Draft" | "Scheduled" | "InProgress" | "Completed" | "Cancelled";

export type SharingAuthorRole = "Owner" | "Contributor";

export type SharingType =
  "Workshop" | "TechTalk" | "PanelDiscussion" | "InternalSharing";

export type DeliveryMode = "Offline" | "Online" | "Hybrid";

export type AudienceScope = "AllMembers" | "SelectedAudience";

export type PresenterRole = "Host" | "Speaker" | "Facilitator" | "Support";

export type ResourceType =
  "Link" | "Document" | "Slide" | "CodeSample" | "Recording";

/* ==========================================================================
   RESPONSE DTOs
   ========================================================================== */

export interface AuthorResponse {
  userId: string;
  fullName: string;
  role: SharingAuthorRole;
  sortOrder: number;
}

export interface TagResponse {
  id: string;
  name: string;
  slug: string;
  color?: string | null;
  isActive: boolean;
}

export interface PresenterResponse {
  userId: string;
  fullName: string;
  role: PresenterRole;
  sortOrder: number;
}

export interface ContentReference {
  id: string;
  title: string;
  slug: string;
  sortOrder: number;
}

export interface ScheduleReference {
  id: string;
  title: string;
  startsAtUtc: string;
  status: SharingScheduleStatus;
}

export interface ContentSummary {
  id: string;
  title: string;
  slug: string;
  summary: string;
  coverImageUrl?: string | null;
  status: SharingContentStatus;
  publishedAtUtc?: string | null;
  version: number;
  authors: AuthorResponse[];
  tags: TagResponse[];
}

export interface ResourceResponse {
  id: string;
  sharingContentId: string;
  title: string;
  description?: string | null;
  resourceType: ResourceType;
  externalUrl?: string | null;
  originalFileName?: string | null;
  fileSize?: number | null;
  contentType?: string | null;
  sortOrder: number;
  isActive: boolean;
}

export interface ResourceMutationResponse {
  resource: ResourceResponse;
  version: number;
}

export interface ContentResponse {
  content: ContentSummary;
  bodyMarkdown: string;
  reviewNote?: string | null;
  submittedAtUtc?: string | null;
  reviewedAtUtc?: string | null;
  reviewedByUserId?: string | null;
  authors: AuthorResponse[];
  tags: TagResponse[];
  resources: ResourceResponse[];
  schedules: ScheduleReference[];
}

export interface ScheduleResponse {
  id: string;
  title: string;
  description?: string | null;
  sharingType: SharingType;
  deliveryMode: DeliveryMode;
  startsAtUtc: string;
  endsAtUtc: string;
  timeZoneId: string;
  location?: string | null;
  meetingUrl?: string | null;
  status: SharingScheduleStatus;
  audienceScope: AudienceScope;
  cancellationReason?: string | null;
  createdByUserId: string;
  version: number;
  presenters: PresenterResponse[];
  contents: ContentReference[];
  generationIds: string[];
  departmentIds: string[];
}

export interface SharingPage<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
}

/* ==========================================================================
   REQUEST DTOs
   ========================================================================== */

export interface ContentRequest {
  title: string;
  slug: string;
  summary: string;
  bodyMarkdown: string;
  tagIds: string[];
  contributorUserIds: string[];
  coverImageUrl?: string | null;
}

export interface ReviewRequest {
  reviewNote: string;
}

export interface CancelScheduleRequest {
  reason: string;
}

export interface TagRequest {
  name: string;
  slug: string;
  color?: string | null;
}

export interface TagStatusRequest {
  isActive?: boolean | null;
}

export interface ResourceRequest {
  title: string;
  description?: string | null;
  externalUrl?: string | null;
  sortOrder?: number;
}

export interface ReorderResourcesRequest {
  ids: string[];
}

export interface PresenterRequest {
  userId: string;
  role: PresenterRole;
  sortOrder?: number;
}

export interface PresentersRequest {
  presenters: PresenterRequest[];
}

export interface ScheduleContentsRequest {
  contentIds: string[];
}

export interface AudienceRequest {
  audienceScope: AudienceScope;
  generationIds: string[];
  departmentIds: string[];
}

export interface ScheduleRequest {
  title: string;
  sharingType: SharingType;
  deliveryMode: DeliveryMode;
  startsAtLocal: string; // ISO format string: YYYY-MM-DDTHH:mm:ss without timezone offset
  endsAtLocal: string;
  timeZoneId: string; // e.g. "Asia/Ho_Chi_Minh"
  audienceScope: AudienceScope;
  presenters: PresenterRequest[];
  contentIds: string[];
  generationIds: string[];
  departmentIds: string[];
  description?: string | null;
  location?: string | null;
  meetingUrl?: string | null;
}

/* ==========================================================================
   QUERY PARAMS
   ========================================================================== */

export interface ContentQueryParams {
  search?: string;
  tagId?: string;
  authorId?: string;
  status?: SharingContentStatus;
  page?: number;
  pageSize?: number;
  sort?: "newest" | "oldest" | "title";
}

export interface ScheduleQueryParams {
  from?: string;
  to?: string;
  status?: SharingScheduleStatus;
  deliveryMode?: DeliveryMode;
  sharingType?: SharingType;
  presenterId?: string;
  page?: number;
  pageSize?: number;
}
