import { httpClient } from './httpClient';
import type { ApiSuccess } from '../types/api';
import type { BrandSurveySummary, BrandSurveyDetails } from '../types/brandSurvey';

interface ListBrandSurveysOptions {
  includeInactive?: boolean;
  teamId?: string;
  signal?: AbortSignal;
}

export const listBrandSurveys = (options?: ListBrandSurveysOptions): Promise<ApiSuccess<BrandSurveySummary[]>> => {
  const searchParams = new URLSearchParams();

  if (options?.teamId) {
    searchParams.set('teamId', options.teamId);
  }

  if (options?.includeInactive) {
    searchParams.set('includeInactive', 'true');
  }

  const queryString = searchParams.toString();

  return httpClient.get<BrandSurveySummary[]>(`/api/brand-surveys${queryString ? `?${queryString}` : ''}`, {
    signal: options?.signal,
  });
};

export const getBrandSurvey = (
  surveyId: string,
  options?: { signal?: AbortSignal },
): Promise<ApiSuccess<BrandSurveyDetails>> => {
  return httpClient.get<BrandSurveyDetails>(`/api/brand-surveys/${surveyId}`, {
    signal: options?.signal,
  });
};
