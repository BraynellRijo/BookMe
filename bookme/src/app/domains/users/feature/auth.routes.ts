import { Routes } from '@angular/router';

/**
 * Auth feature routes — lazy-loaded under the /auth path.
 *
 * Routes:
 * - /auth/login       → Login page
 * - /auth/register    → Registration page
 * - /auth/verify-email → Email verification page (after registration)
 */
export const AUTH_ROUTES: Routes = [
  {
    path: 'login',
    loadComponent: () =>
      import('./login/login.component').then((m) => m.LoginComponent),
  },
  {
    path: 'register',
    loadComponent: () =>
      import('./register/register.component').then((m) => m.RegisterComponent),
  },
  {
    path: 'verify-email',
    loadComponent: () =>
      import('./verify-email/verify-email.component').then(
        (m) => m.VerifyEmailComponent
      ),
  },
  { path: '', redirectTo: 'login', pathMatch: 'full' },
];
