export const ROLE_TYPES = {
  Admin: 1,
  SocialMediaManager: 2,
  ContentCreator: 3,
  Approver: 4,
  Individual: 5,
} as const;

export type RoleType = (typeof ROLE_TYPES)[keyof typeof ROLE_TYPES];
export type RoleName = keyof typeof ROLE_TYPES;

export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  accessToken: string;
  refreshToken: string;
  accessTokenExpiresOnUtc: string;
  refreshTokenExpiresOnUtc: string;
  userId: string;
  email: string;
  fullName: string;
  role: number;
  teamId: string | null;
}

export interface AuthenticatedUser {
  userId: string;
  email: string;
  fullName: string;
  role?: number;
  roleName?: string;
  teamId: string | null;
  teamName?: string | null;
  accessTokenExpiresOnUtc?: string;
  refreshTokenExpiresOnUtc?: string;
}

export interface RefreshRequest {
  refreshToken: string;
}

export interface UserProfile {
  id: string;
  email: string;
  fullName: string;
  role: number;
  teamId: string | null;
  teamName?: string | null;
}

export interface RegisterRequest {
  email: string;
  password: string;
  firstName: string;
  lastName: string;
  role: RoleType;
  teamName?: string | null;
  isIndividual: boolean;
  brandSurvey: BrandSurveyOnboardingRequest;
}

export interface AssignRoleRequest {
  userId: string;
  role: RoleName;
}

export interface BrandSurveyOnboardingRequest {
  voice: string;
  tone: string;
  goals: string;
  audience: string;
  competitors: string;
  keywords: string[];
}
