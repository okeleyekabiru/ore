import { httpClient } from './httpClient';
import type { ApiSuccess } from '../types/api';
import type { LoginRequest, LoginResponse, RefreshRequest, RegisterRequest, UserProfile } from '../types/auth';

export const login = (credentials: LoginRequest): Promise<ApiSuccess<LoginResponse>> => {
  return httpClient.post<LoginResponse>('/api/auth/login', credentials, { skipAuth: true });
};

export const refresh = (request: RefreshRequest): Promise<ApiSuccess<LoginResponse>> => {
  return httpClient.post<LoginResponse>('/api/auth/refresh', request, { skipAuth: true });
};

export const getProfile = (): Promise<ApiSuccess<UserProfile>> => {
  return httpClient.get<UserProfile>('/api/users/detail');
};

export const register = (payload: RegisterRequest): Promise<ApiSuccess<string>> => {
  return httpClient.post<string>('/api/auth/register', payload, { skipAuth: true });
};
