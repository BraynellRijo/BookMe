import { CanActivateFn, Router } from '@angular/router';
import { inject } from '@angular/core';

import { TokenService } from '../services/token.service';

/**
 * Role Guard — Protects routes that require specific user roles.
 *
 * Usage:
 * ```typescript
 * {
 *   path: 'host/listings',
 *   canActivate: [authGuard, roleGuard],
 *   data: { roles: ['Host'] },
 *   component: HostListingsComponent
 * }
 * ```
 *
 * Behavior:
 * - Reads the `roles` array from the route's `data` property.
 * - Checks if the current user has at least one of the required roles.
 * - If not → redirects to home page.
 *
 * Note: Always pair with `authGuard` to ensure the user is logged in first.
 */
export const roleGuard: CanActivateFn = (route) => {
  const tokenService = inject(TokenService);
  const router = inject(Router);

  const requiredRoles = route.data?.['roles'] as string[] | undefined;

  if (!requiredRoles || requiredRoles.length === 0) {
    return true;
  }

  const hasRequiredRole = requiredRoles.some((role) => tokenService.hasRole(role));

  if (hasRequiredRole) {
    return true;
  }

  return router.createUrlTree(['/']);
};
