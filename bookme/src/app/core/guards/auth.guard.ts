import { CanActivateFn, Router } from '@angular/router';
import { inject } from '@angular/core';

import { TokenService } from '../services/token.service';

/**
 * Auth Guard — Protects routes that require authentication.
 *
 * Usage:
 * ```typescript
 * { path: 'dashboard', canActivate: [authGuard], component: DashboardComponent }
 * ```
 *
 * Behavior:
 * - If the user has a valid (non-expired) token → allow access.
 * - Otherwise → redirect to `/auth/login` and preserve the intended URL
 *   so we can redirect back after successful login.
 */
export const authGuard: CanActivateFn = (route, state) => {
  const tokenService = inject(TokenService);
  const router = inject(Router);

  if (tokenService.isAuthenticated()) {
    return true;
  }

  // Store the attempted URL for redirecting after login
  return router.createUrlTree(['/auth/login'], {
    queryParams: { returnUrl: state.url },
  });
};
