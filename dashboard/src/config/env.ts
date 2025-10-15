const sanitizeBaseUrl = (value: string | undefined) => {
  if (!value) {
    return undefined;
  }

  return value.endsWith('/') ? value.slice(0, -1) : value;
};

const defaultApiBaseUrl = 'http://localhost:5000';

export const API_BASE_URL = sanitizeBaseUrl(import.meta.env.VITE_API_BASE_URL) ?? defaultApiBaseUrl;
