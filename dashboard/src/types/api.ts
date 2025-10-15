export interface ApiEnvelope<T> {
  data?: T;
  success: boolean;
  message?: string | null;
  errors: string[];
}

export interface ApiSuccess<T> {
  data: T;
  message?: string;
}

export class ApiError extends Error {
  status: number;
  errors: string[];

  constructor(message: string, status: number, errors?: string[]) {
    super(message);
    this.name = 'ApiError';
    this.status = status;
    this.errors = errors ?? [];
  }
}
