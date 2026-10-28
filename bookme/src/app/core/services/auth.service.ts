import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, tap } from 'rxjs';

import { environment } from '../../../environments/environment';
import { TokenService } from './token.service';
import {
  LoginRequest,
  RegisterRequest,
  VerifyEmailRequest,
  RefreshTokenRequest,
  TokenResponse,
} from '../models/auth.interface';

/**
 * AuthService — Handles all authentication-related API calls.
 *
 * Delegates token management entirely to TokenService.
 * Exposes the reactive user state via TokenService signals.
 *
 * Endpoints consumed:
 * - POST /api/Auth/register
 * - POST /api/Auth/verify-email
 * - POST /api/Auth/login
 * - POST /api/Auth/refresh-token
 * - PATCH /api/Auth/become-host
 */



@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);
  private readonly tokenService = inject(TokenService);

  private readonly baseUrl = `${environment.apiUrl}/Auth`;

  // ─── Expose TokenService signals for convenience ────────────────

  /** Reactive signal: the current user profile (or null). */
  readonly currentUser = this.tokenService.currentUser;

  /** Reactive signal: whether the user is authenticated. */
  readonly isAuthenticated = this.tokenService.isAuthenticated;

  /** Reactive signal: whether the user is a Host. */
  readonly isHost = this.tokenService.isHost;

  // ─── API Calls ──────────────────────────────────────────────────

  /**
   * Register a new user account.
   * After registration, the user must verify their email before logging in.
   */
  register(data: any): Observable<unknown> {
    return this.http.post(`${this.baseUrl}/register`, data);
  }

  /**
   * Verify the user's email with the code sent by the backend.
   * Must be called BEFORE login — unverified users get 403.
   */
  verifyEmail(data: VerifyEmailRequest): Observable<unknown> {
    return this.http.post(`${this.baseUrl}/verify-email`, data);
  }

  /**
   * Authenticate and receive JWT tokens.
   * Tokens are persisted automatically via TokenService.
   */
  login(data: LoginRequest): Observable<TokenResponse> {
    return this.http
      .post<TokenResponse>(`${this.baseUrl}/login`, data)
      .pipe(tap((response) => this.tokenService.setTokens(response)));
  }

  /**
   * Refresh an expired access token using the stored refresh token.
   * Called automatically by the auth interceptor on 401 responses.
   */
  refreshToken(): Observable<TokenResponse> {
    const accessToken = this.tokenService.getAccessToken() ?? '';
    const refreshToken = this.tokenService.getRefreshToken() ?? '';

    const request: RefreshTokenRequest = { accessToken, refreshToken };

    return this.http
      .post<TokenResponse>(`${this.baseUrl}/refresh-token`, request)
      .pipe(tap((response) => this.tokenService.setTokens(response)));
  }

  /**
   * Upgrade the current user's role to "Host".
   * The backend returns a new token with the updated roles claim.
   */
  becomeHost(): Observable<TokenResponse> {
    return this.http
      .patch<TokenResponse>(`${this.baseUrl}/become-host`, {})
      .pipe(tap((response) => this.tokenService.setTokens(response)));
  }

  /**
   * Log the user out: clear tokens and redirect to login.
   */
  logout(): void {
    this.tokenService.clearTokens();
    this.router.navigate(['/auth/login']);
  }
}
