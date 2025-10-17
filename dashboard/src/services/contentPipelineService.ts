import { httpClient } from './httpClient';
import type {
  ContentPipelineApiStatus,
  ContentPipelineItem,
  ContentPipelineItemsResponse,
  ContentPipelinePageRequest,
  ContentPipelineStatus,
  ContentPipelineSummary,
  CreateContentPipelineItemPayload,
  UpdateContentPipelineStatusPayload,
} from '../types/contentPipeline';

const BASE_PATH = '/api/content-pipeline';

interface ContentPipelineSummaryApiResponse {
  status: string;
  count: number;
}

interface ContentPipelineOwnerApiResponse {
  id: string | null;
  name: string | null;
}

interface ContentPipelineChannelApiResponse {
  id: string;
  name: string;
}

interface ContentPipelineItemApiResponse {
  id: string;
  teamId: string;
  title: string;
  status: string;
  channel: ContentPipelineChannelApiResponse;
  owner: ContentPipelineOwnerApiResponse;
  updatedOnUtc: string;
  dueOnUtc?: string | null;
  scheduledOnUtc?: string | null;
}

interface ContentPipelineItemsApiResponse {
  items: ContentPipelineItemApiResponse[];
  page: number;
  pageSize: number;
  totalItems: number;
  totalPages: number;
}

const STATUS_TO_API: Record<ContentPipelineStatus, ContentPipelineApiStatus> = {
  Ideation: 'Draft',
  Drafting: 'Generated',
  InApproval: 'PendingApproval',
  Scheduled: 'Scheduled',
  Published: 'Published',
  NeedsRevision: 'Rejected',
};

const STATUS_FROM_API: Record<ContentPipelineApiStatus, ContentPipelineStatus> = {
  Draft: 'Ideation',
  Generated: 'Drafting',
  PendingApproval: 'InApproval',
  Approved: 'InApproval',
  Scheduled: 'Scheduled',
  Published: 'Published',
  Rejected: 'NeedsRevision',
};

const toApiStatus = (status: ContentPipelineStatus): ContentPipelineApiStatus => STATUS_TO_API[status];

const toUiStatus = (status: string): ContentPipelineStatus => {
  const candidate = status as ContentPipelineApiStatus;
  return STATUS_FROM_API[candidate] ?? 'Ideation';
};

const normalizeChannelId = (id?: string | null) => (id ? id.toLowerCase() : 'unassigned');

const mapOwner = (owner?: ContentPipelineOwnerApiResponse | null) => ({
  id: owner?.id ?? 'unassigned-owner',
  name: owner?.name ?? 'Unassigned owner',
});

const mapChannel = (channel?: ContentPipelineChannelApiResponse | null) => ({
  id: normalizeChannelId(channel?.id),
  name: channel?.name ?? 'Unassigned',
});

const mapItemFromApi = (item: ContentPipelineItemApiResponse): ContentPipelineItem => ({
  id: item.id,
  teamId: item.teamId,
  title: item.title,
  status: toUiStatus(item.status),
  channel: mapChannel(item.channel),
  owner: mapOwner(item.owner),
  updatedOnUtc: item.updatedOnUtc,
  dueOnUtc: item.dueOnUtc ?? null,
  scheduledOnUtc: item.scheduledOnUtc ?? null,
});

const mapChannelToApi = (channel: string | null | undefined): string | null => {
  if (!channel) {
    return null;
  }

  switch (channel.toLowerCase()) {
    case 'linkedin':
      return 'LinkedIn';
    case 'instagram':
      return 'Instagram';
    case 'meta':
      return 'Meta';
    case 'x':
      return 'X';
    case 'tiktok':
      return 'TikTok';
    default:
      return null;
  }
};

const buildQueryString = (params?: ContentPipelinePageRequest) => {
  if (!params) {
    return '';
  }

  const searchParams = new URLSearchParams();

  if (params.status) {
    searchParams.set('status', params.status);
  }

  if (params.page) {
    searchParams.set('page', params.page.toString());
  }

  if (params.pageSize) {
    searchParams.set('pageSize', params.pageSize.toString());
  }

  if (params.teamId) {
    searchParams.set('teamId', params.teamId);
  }

  if (params.ownerId) {
    searchParams.set('ownerId', params.ownerId);
  }

  if (params.search) {
    searchParams.set('search', params.search);
  }

  const serialized = searchParams.toString();
  return serialized ? `?${serialized}` : '';
};

const getSummary = async (teamId?: string) => {
  const query = teamId ? `?teamId=${encodeURIComponent(teamId)}` : '';
  const { data } = await httpClient.get<ContentPipelineSummaryApiResponse[]>(`${BASE_PATH}/summary${query}`);

  const summaryMap = new Map<ContentPipelineStatus, number>();

  data.forEach((entry) => {
    const status = toUiStatus(entry.status);
    summaryMap.set(status, (summaryMap.get(status) ?? 0) + entry.count);
  });

  const summaries: ContentPipelineSummary[] = [];
  summaryMap.forEach((count, status) => {
    summaries.push({ status, count });
  });

  return summaries;
};

const getItems = async (params?: ContentPipelinePageRequest) => {
  const { data } = await httpClient.get<ContentPipelineItemsApiResponse>(`${BASE_PATH}/items${buildQueryString(params)}`);

  const items: ContentPipelineItem[] = data.items.map(mapItemFromApi);

  const response: ContentPipelineItemsResponse = {
    items,
    page: data.page,
    pageSize: data.pageSize,
    totalItems: data.totalItems,
    totalPages: data.totalPages,
  };

  return response;
};

const updateStatus = async (itemId: string, payload: UpdateContentPipelineStatusPayload) => {
  const body = {
    status: toApiStatus(payload.status),
    scheduledOnUtc: payload.scheduledOnUtc ?? null,
  };

  await httpClient.patch<void>(`${BASE_PATH}/items/${itemId}/status`, body);
};

const bulkUpdateStatus = async (itemIds: string[], payload: UpdateContentPipelineStatusPayload) => {
  const body = {
    itemIds,
    status: toApiStatus(payload.status),
    scheduledOnUtc: payload.scheduledOnUtc ?? null,
  };

  await httpClient.patch<void>(`${BASE_PATH}/items/status`, body);
};

const createItem = async (payload: CreateContentPipelineItemPayload) => {
  const body = {
    title: payload.title,
    status: toApiStatus(payload.status),
    channel: mapChannelToApi(payload.channel),
    dueOnUtc: payload.dueOnUtc ?? null,
    teamId: payload.teamId ?? null,
  };

  const { data } = await httpClient.post<ContentPipelineItemApiResponse>(`${BASE_PATH}/items`, body);
  return mapItemFromApi(data);
};

export const contentPipelineService = {
  getSummary,
  getItems,
  updateStatus,
  bulkUpdateStatus,
  createItem,
};
