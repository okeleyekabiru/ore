import { httpClient } from './httpClient';
import type {
  ContentPipelineItemsResponse,
  ContentPipelinePageRequest,
  ContentPipelineSummary,
  UpdateContentPipelineStatusPayload,
} from '../types/contentPipeline';

const BASE_PATH = '/api/content-pipeline';

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
  const { data } = await httpClient.get<ContentPipelineSummary[]>(`${BASE_PATH}/summary${query}`);
  return data;
};

const getItems = async (params?: ContentPipelinePageRequest) => {
  const { data } = await httpClient.get<ContentPipelineItemsResponse>(`${BASE_PATH}/items${buildQueryString(params)}`);
  return data;
};

const updateStatus = async (itemId: string, payload: UpdateContentPipelineStatusPayload) => {
  await httpClient.patch<void>(`${BASE_PATH}/items/${itemId}/status`, payload);
};

const bulkUpdateStatus = async (itemIds: string[], payload: UpdateContentPipelineStatusPayload) => {
  await httpClient.patch<void>(`${BASE_PATH}/items/status`, { itemIds, ...payload });
};

export const contentPipelineService = {
  getSummary,
  getItems,
  updateStatus,
  bulkUpdateStatus,
};
