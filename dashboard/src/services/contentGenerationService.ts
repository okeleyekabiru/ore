import { httpClient } from './httpClient';
import type { GenerateContentPayload, GeneratedContentSuggestion } from '../types/contentGeneration';

export const contentGenerationService = {
  generateContent(payload: GenerateContentPayload) {
    return httpClient.post<GeneratedContentSuggestion>('/api/content/generate', payload);
  },
};
