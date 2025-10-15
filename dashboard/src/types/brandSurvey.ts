export interface BrandSurveySummary {
  id: string;
  teamId: string;
  title: string;
  description: string;
  isActive: boolean;
  questionCount: number;
  createdOnUtc: string;
  modifiedOnUtc?: string | null;
}

export interface BrandSurveyQuestionDetails {
  id: string;
  prompt: string;
  type: string;
  order: number;
  options: string[];
}

export interface BrandSurveyDetails {
  id: string;
  teamId: string;
  title: string;
  description: string;
  isActive: boolean;
  questions: BrandSurveyQuestionDetails[];
}
