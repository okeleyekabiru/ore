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
