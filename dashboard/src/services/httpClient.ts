import { API_BASE_URL } from '../config/env';
import { authStorage } from './authStorage';
import type { ApiEnvelope, ApiSuccess } from '../types/api';
import { ApiError } from '../types/api';

type HttpMethod = 'GET' | 'POST' | 'PUT' | 'PATCH' | 'DELETE';

type RequestOptions = {
  method?: HttpMethod;
  body?: unknown;
  headers?: Record<string, string>;
  signal?: AbortSignal;
  skipAuth?: boolean;
};

type RequestConfig = RequestOptions & { path: string };

const buildUrl = (path: string) => {
  if (path.startsWith('http://') || path.startsWith('https://')) {
    return path;
  }

  if (path.startsWith('/')) {
    return `${API_BASE_URL}${path}`;
  }

  return `${API_BASE_URL}/${path}`;
};

const normalizeBody = (body: unknown) => {
  if (!body) {
    return undefined;
  }

  if (body instanceof FormData || body instanceof URLSearchParams) {
    return body;
  }

  return JSON.stringify(body);
};

const isApiEnvelope = <T>(payload: unknown): payload is ApiEnvelope<T> => {
  if (typeof payload !== 'object' || payload === null) {
    return false;
  }

  return 'success' in payload && 'errors' in payload;
};

const parsePayload = async (response: Response) => {
  const text = await response.text();

  if (!text) {
    return undefined;
  }

  try {
    return JSON.parse(text) as unknown;
  } catch (error) {
    throw new ApiError('Invalid JSON response received from API.', response.status);
  }
};

const buildHeaders = (options?: RequestOptions) => {
  const headers: Record<string, string> = {
    Accept: 'application/json',
    ...(options?.headers ?? {}),
  };

  if (options?.body && !(options.body instanceof FormData) && !(options.body instanceof URLSearchParams)) {
    headers['Content-Type'] = 'application/json';
  }

  if (!options?.skipAuth) {
    const accessToken = authStorage.getAccessToken();
    if (accessToken) {
      headers.Authorization = `Bearer ${accessToken}`;
    }
  }

  return headers;
};

const request = async <T>({ path, method = 'GET', body, signal, skipAuth, headers }: RequestConfig): Promise<ApiSuccess<T>> => {
  const response = await fetch(buildUrl(path), {
    method,
    body: normalizeBody(body),
    headers: buildHeaders({ body, headers, skipAuth }),
    signal,
  });

  const payload = await parsePayload(response);

  if (!response.ok) {
    if (isApiEnvelope(payload)) {
      throw new ApiError(payload.message ?? 'Request failed.', response.status, payload.errors);
    }

    throw new ApiError(response.statusText || 'Request failed.', response.status);
  }

  if (isApiEnvelope<T>(payload)) {
    if (!payload.success) {
      throw new ApiError(payload.message ?? 'Request failed.', response.status, payload.errors);
    }

    return {
      data: (payload.data as T | undefined) as T,
      message: payload.message ?? undefined,
    };
  }

  return {
    data: (payload as T | undefined) as T,
    message: undefined,
  };
};

export const httpClient = {
  get<T>(path: string, options?: Omit<RequestOptions, 'method' | 'body'>) {
    return request<T>({ path, ...options, method: 'GET' });
  },
  post<T>(path: string, body?: unknown, options?: Omit<RequestOptions, 'method'>) {
    return request<T>({ path, body, ...options, method: 'POST' });
  },
  put<T>(path: string, body?: unknown, options?: Omit<RequestOptions, 'method'>) {
    return request<T>({ path, body, ...options, method: 'PUT' });
  },
  patch<T>(path: string, body?: unknown, options?: Omit<RequestOptions, 'method'>) {
    return request<T>({ path, body, ...options, method: 'PATCH' });
  },
  delete<T>(path: string, options?: Omit<RequestOptions, 'method' | 'body'>) {
    return request<T>({ path, ...options, method: 'DELETE' });
  },
};
