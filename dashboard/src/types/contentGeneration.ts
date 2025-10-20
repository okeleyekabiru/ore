export interface GenerateContentPayload {
  brandId: string;
  topic: string;
  tone: string;
  platform: string;
}

export interface GeneratedContentSuggestion {
  caption: string;
  hashtags: string[];
  imageIdea: string;
}

export const PLATFORM_CHOICES = [
  { value: 'LinkedIn', label: 'LinkedIn' },
  { value: 'Instagram', label: 'Instagram' },
  { value: 'Meta', label: 'Meta (Facebook)' },
  { value: 'X', label: 'X (Twitter)' },
  { value: 'TikTok', label: 'TikTok' },
];
