/**
 * User-related interfaces.
 * JwtPayload represents the decoded JWT token payload from the backend.
 */

/** Custom JWT payload structure matching the .NET Core Identity claims */
export interface JwtPayload {
  sub: string;
  email: string;
  firstName: string;
  lastName: string;
  roles: string | string[];
  // Token expiration as a Unix timestamp (seconds)
  exp: number;
  // Token issued-at as a Unix timestamp (seconds)
  iat: number;
}

// Normalized user profile derived from the decoded JWT
export interface UserProfile {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  roles: string[];
}
