import { Injectable, signal, computed } from '@angular/core';
import { JwtPayload, UserProfile } from '../models/user.interface';
import { TokenResponse } from '../models/auth.interface';

const ACCESS_TOKEN_KEY = 'rentme_access_token';
const REFRESH_TOKEN_KEY = 'rentme_refresh_token';

/**
 * TokenService — Manages JWT token storage, decoding, and role extraction.
 *
 * Responsibilities:
 * - Store/retrieve/clear tokens from localStorage.
 * - Decode the JWT payload WITHOUT external libraries (using atob + JSON.parse).
 * - Expose reactive signals for the current user state.
 * - Provide role-checking utilities used by guards and components.
 */
@Injectable({ providedIn: 'root' })
export class TokenService {
  /**
   * Reactive signal holding the decoded JWT payload.
   * Null when no valid token is stored.
   */
  private readonly _currentUser = signal<UserProfile | null>(this._loadUserFromStorage());

  /** Public readonly signal for the current user profile */
  readonly currentUser = this._currentUser.asReadonly();

  /** Computed signal: true when a valid (non-expired) user exists */
  readonly isAuthenticated = computed(() => {
    const user = this._currentUser();
    if (!user) return false;
    return !this.isTokenExpired();
  });

  /** Computed signal: true when the current user is authenticated and has the 'Host' role */
  readonly isHost = computed(() => this.isAuthenticated() && this.hasRole('Host'));
  
  /** Computed signal: true when the current user is authenticated and has the 'Guest' role */
  readonly isGuest = computed(() => this.isAuthenticated() && this.hasRole('Guest'));

  // ─── Token Storage ──────────────────────────────────────────────

  /** Persist both tokens and update the reactive user signal. */
  setTokens(response: any): void {
    const accessToken = response.accessToken || response.access_token || response.accesstoken;
    const refreshToken = response.refreshToken || response.refresh_token || response.refreshtoken;

    if (!accessToken) {
      console.error('❌ TokenService: No se encontró accessToken en la respuesta del servidor', response);
      return;
    }

    localStorage.setItem(ACCESS_TOKEN_KEY, accessToken);
    if (refreshToken) localStorage.setItem(REFRESH_TOKEN_KEY, refreshToken);
    
    const profile = this._buildUserProfile();
    this._currentUser.set(profile);
  }

  /** Retrieve the stored access token. */
  getAccessToken(): string | null {
    return localStorage.getItem(ACCESS_TOKEN_KEY);
  }

  /** Retrieve the stored refresh token. */
  getRefreshToken(): string | null {
    return localStorage.getItem(REFRESH_TOKEN_KEY);
  }

  /** Clear all tokens and reset the user signal (logout). */
  clearTokens(): void {
    localStorage.removeItem(ACCESS_TOKEN_KEY);
    localStorage.removeItem(REFRESH_TOKEN_KEY);
    this._currentUser.set(null);
  }

  // ─── JWT Decoding ───────────────────────────────────────────────

  /**
   * Decode the JWT payload from the stored access token.
   * Uses native `atob()` — no external libraries needed.
   *
   * @returns The decoded payload or null if no/invalid token.
   */
  decodeToken(): JwtPayload | null {
    const token = this.getAccessToken();
    if (!token) return null;

    try {
      const parts = token.split('.');
      if (parts.length !== 3) {
        console.error('⚠️ TokenService: El token JWT no tiene 3 partes');
        return null;
      }

      const payload = parts[1]
        .replace(/-/g, '+')
        .replace(/_/g, '/');

      const decoded = atob(payload);
      const parsed = JSON.parse(decoded) as JwtPayload;
      return parsed;
    } catch (e) {
      console.error('❌ TokenService: Error decodificando el payload del JWT', e);
      return null;
    }
  }

  /**
   * Check if the current access token has expired.
   * Compares the `exp` claim (Unix seconds) against the current time.
   */
  isTokenExpired(): boolean {
    const payload = this.decodeToken();
    if (!payload?.exp) return true;

    // Add a 30-second buffer to account for clock skew
    const now = Math.floor(Date.now() / 1000);
    return payload.exp < now + 30;
  }

  // ─── Role Utilities ─────────────────────────────────────────────

  /**
   * Extract the user's roles from the JWT payload.
   * The backend may return roles as a single string or an array.
   * Supports standard 'roles' and .NET 'role' claims.
   */
  getUserRoles(): string[] {
    const payload = this.decodeToken() as any;
    if (!payload) return [];

    const roles = payload.roles || 
                  payload.role || 
                  payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'];

    if (!roles) return [];
    return Array.isArray(roles) ? roles : [roles];
  }

  /** Check if the current user has a specific role. */
  hasRole(role: string): boolean {
    return this.getUserRoles().some(
      (r) => r.toLowerCase() === role.toLowerCase()
    );
  }

  // ─── Private Helpers ────────────────────────────────────────────

  /** Build a normalized UserProfile from the decoded JWT. */
  private _buildUserProfile(): UserProfile | null {
    const payload = this.decodeToken() as any;
    if (!payload) return null;

    // Handle standard and Microsoft-specific claim names
    return {
      id: payload.sub || payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'],
      email: payload.email || payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress'],
      firstName: payload.firstName || payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname'] || payload.name,
      lastName: payload.lastName || payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname'],
      roles: this.getUserRoles(),
    };
  }

  /** Load the user from storage on service initialization (app startup). */
  private _loadUserFromStorage(): UserProfile | null {
    const token = localStorage.getItem(ACCESS_TOKEN_KEY);
    if (!token) return null;

    if (this.isTokenExpired()) {
      // Token exists but is expired. Don't auto-login.
      return null;
    }

    // Build profile mapped correctly with ASP.NET conventions
    return this._buildUserProfile();
  }
}
