/**
 * Auth-related DTOs that mirror the .NET Core backend contracts.
 * These interfaces ensure type-safe HTTP communication.
 */

/** POST /api/Auth/register */
export interface RegisterRequest {
  firstName: string;
  lastName: string;
  phoneNumber: string;
  gender: Gender;
  email: string;
  password: string;
  confirmPassword: string;
}

export enum Gender {
  Masculino = 1,
  Femenino = 2,
}

/** POST /api/auth/login */
export interface LoginRequest {
  email: string;
  password: string;
}

/** POST /api/auth/verify-email */
export interface VerifyEmailRequest {
  email: string;
  code: string;
}

/** POST /api/auth/refresh-token */
export interface RefreshTokenRequest {
  accessToken: string;
  refreshToken: string;
}

/** Response from login and refresh-token endpoints */
export interface TokenResponse {
  accessToken: string;
  refreshToken: string;
}
