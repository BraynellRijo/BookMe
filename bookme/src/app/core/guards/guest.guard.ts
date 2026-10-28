import { CanActivateFn, Router } from '@angular/router';
import { inject } from '@angular/core';

import { TokenService } from '../services/token.service';

/**
 * Guest Guard — Prevents authenticated users from accessing auth pages.
 *
 * Usage:
 * ```typescript
 * {
 *   path: 'auth',
 *   canActivate: [guestGuard],
 *   loadChildren: () => import('./auth.routes').then(m => m.AUTH_ROUTES)
 * }
 * ```
 *
 * Behavior:
 * - If the user is NOT authenticated → allow (they can reach login/register).
 * - If the user IS authenticated → redirect to home page.
 */
export const guestGuard: CanActivateFn = () => {
  const tokenService = inject(TokenService);
  const router = inject(Router);

  if (!tokenService.isAuthenticated()) {
    return true;
  }

  // Already logged in — redirect to home
  return router.createUrlTree(['/']);
};
