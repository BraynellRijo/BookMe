import { HttpInterceptorFn, HttpErrorResponse, HttpRequest, HttpHandlerFn } from '@angular/common/http';
import { inject, Injector } from '@angular/core';
import { BehaviorSubject, catchError, filter, switchMap, take, throwError } from 'rxjs';

import { TokenService } from '../services/token.service';
import { AuthService } from '../services/auth.service';
import { environment } from '../../../environments/environment';

/**
 * Tracks whether a token refresh is currently in progress.
 * Prevents multiple simultaneous refresh calls when several
 * 401 responses arrive at the same time.
 */
let isRefreshing = false;
const refreshTokenSubject = new BehaviorSubject<string | null>(null);

/** Routes that should NOT receive the Authorization header or warn about missing tokens */
const AUTH_WHITELIST = [
  // Auth Flows
  '/Auth/login',
  '/Auth/register',
  '/Auth/verify-email',
  '/Auth/refresh-token',
  
  // Public Modules (AllowAnonymous)
  '/Listings',
  '/Amenities',
  '/Reviews/Listing-Rating',
  '/Reviews/Listing-Reviews',
  '/Availability/Listing-Blocks'
];

/**
 * Functional HTTP Interceptor for JWT authentication.
 *
 * Behavior:
 * 1. If the request targets the API and we have a token → attach Bearer header.
 * 2. If the response is 401 → attempt a silent token refresh.
 * 3. If refresh succeeds → replay the failed request with the new token.
 * 4. If refresh fails → logout the user.
 * 5. Auth whitelist routes are excluded to prevent refresh loops.
 */
export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const tokenService = inject(TokenService);
  const injector = inject(Injector);

  // Skip non-API requests (e.g. assets)
  if (!isApiRequest(req)) {
     return next(req);
  }

  // Check if route is in the auth-less whitelist
  if (isWhitelisted(req)) {
    return next(req);
  }

  const token = tokenService.getAccessToken();
  
  // If we have a token, attach it
  if (token) {
    const authReq = addToken(req, token);
    return next(authReq).pipe(
      catchError((error) => {
        // Handle 401s specifically with refresh token logic
        if (error instanceof HttpErrorResponse && error.status === 401) {
          const authService = injector.get(AuthService);
          return handleUnauthorized(req, next, tokenService, authService);
        }
        return throwError(() => error);
      })
    );
  }

  // Not whitelisted but no token?
  return next(req);
};

//  Helper Functions

/** Clone request with Authorization header. */
function addToken(req: HttpRequest<unknown>, token: string): HttpRequest<unknown> {
  return req.clone({
    setHeaders: { Authorization: `Bearer ${token}` },
  });
}

/** Check if the request URL targets our API. */
function isApiRequest(req: HttpRequest<unknown>): boolean {
  return req.url.startsWith(environment.apiUrl);
}

/** Check if the request URL is in the auth whitelist (no interception needed). */
function isWhitelisted(req: HttpRequest<unknown>): boolean {
  return AUTH_WHITELIST.some((path) => 
    req.url.toLowerCase().includes(path.toLowerCase())
  );
}

/**
 * Handle 401 responses with a token refresh queue.
 *
 * - If no refresh is in progress → start one.
 * - If a refresh is already in progress → wait for it to complete,
 *   then replay the original request with the new token.
 */
function handleUnauthorized(
  req: HttpRequest<unknown>,
  next: HttpHandlerFn,
  tokenService: TokenService,
  authService: AuthService
) {
  if (!isRefreshing) {
    isRefreshing = true;
    refreshTokenSubject.next(null);

    return authService.refreshToken().pipe(
      switchMap((response) => {
        isRefreshing = false;
        refreshTokenSubject.next(response.accessToken);
        return next(addToken(req, response.accessToken));
      }),
      catchError((refreshError) => {
        isRefreshing = false;
        authService.logout();
        return throwError(() => refreshError);
      })
    );
  }

  // Another request triggered while refresh is in progress — queue it
  return refreshTokenSubject.pipe(
    filter((token) => token !== null),
    take(1),
    switchMap((token) => next(addToken(req, token)))
  );
}
