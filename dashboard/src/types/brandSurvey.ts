export const SurveyQuestionType = {
  Text: 1,
  TextArea: 2,
  SingleChoice: 3,
  MultiChoice: 4,
  Scale: 5,
} as const;

export type SurveyQuestionType = (typeof SurveyQuestionType)[keyof typeof SurveyQuestionType];

export interface BrandSurveySummary {
  id: string;
  teamId: string;
  title: string;
  description: string;
  category: string;
  isActive: boolean;
  questionCount: number;
  createdOnUtc: string;
  modifiedOnUtc?: string | null;
}

export interface BrandSurveyQuestionDetails {
  id: string;
  prompt: string;
  type: SurveyQuestionType;
  order: number;
  options: string[];
}

export interface BrandSurveyDetails {
  id: string;
  teamId: string;
  title: string;
  description: string;
  category: string;
  isActive: boolean;
  questions: BrandSurveyQuestionDetails[];
}

export interface CreateSurveyQuestionPayload {
  prompt: string;
  type: SurveyQuestionType;
  order: number;
  options: string[];
}

export interface CreateBrandSurveyPayload {
  teamId: string;
  title: string;
  description: string;
  category: string;
  questions: CreateSurveyQuestionPayload[];
}
