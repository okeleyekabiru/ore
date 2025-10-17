export const ContentPipelineStatus = {
  Ideation: 'Ideation',
  Drafting: 'Drafting',
  InApproval: 'InApproval',
  Scheduled: 'Scheduled',
  Published: 'Published',
  NeedsRevision: 'NeedsRevision',
} as const;

export type ContentPipelineStatus = (typeof ContentPipelineStatus)[keyof typeof ContentPipelineStatus];

export type ContentPipelineApiStatus =
  | 'Draft'
  | 'Generated'
  | 'PendingApproval'
  | 'Approved'
  | 'Scheduled'
  | 'Published'
  | 'Rejected';

export interface ContentPipelineSummary {
  status: ContentPipelineStatus;
  count: number;
}

export interface ContentPipelineItemOwner {
  id: string;
  name: string;
}

export interface ContentPipelineItemChannel {
  id: string;
  name: string;
}

export interface ContentPipelineItem {
  id: string;
  teamId: string;
  title: string;
  status: ContentPipelineStatus;
  channel: ContentPipelineItemChannel;
  owner: ContentPipelineItemOwner;
  updatedOnUtc: string;
  dueOnUtc?: string | null;
  scheduledOnUtc?: string | null;
}

export interface ContentPipelinePageRequest {
  status?: ContentPipelineStatus;
  page?: number;
  pageSize?: number;
  teamId?: string;
  ownerId?: string;
  search?: string;
}

export interface ContentPipelineItemsResponse {
  items: ContentPipelineItem[];
  page: number;
  pageSize: number;
  totalItems: number;
  totalPages: number;
}

export interface UpdateContentPipelineStatusPayload {
  status: ContentPipelineStatus;
  scheduledOnUtc?: string | null;
}

export interface CreateContentPipelineItemPayload {
  title: string;
  status: ContentPipelineStatus;
  channel: string;
  dueOnUtc?: string | null;
  teamId?: string | null;
}
