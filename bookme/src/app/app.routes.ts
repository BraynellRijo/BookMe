import { Routes } from '@angular/router';
import { guestGuard } from './core/guards/guest.guard';
import { authGuard } from './core/guards/auth.guard';
import { roleGuard } from './core/guards/role.guard';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () => import('./domains/properties/pages/home/home.component').then(m => m.HomeComponent)
  },
  {
    path: 'properties/:id',
    loadComponent: () => import('./domains/properties/pages/property-detail/property-detail.component').then(m => m.PropertyDetailComponent)
  },
  {
    path: 'auth',
    canActivate: [guestGuard],
    loadChildren: () =>
      import('./domains/users/feature/auth.routes').then((m) => m.AUTH_ROUTES),
  },
  // Future: protected routes will go here with authGuard / roleGuard
  {
    path: 'profile',
    loadComponent: () => import('./domains/users/feature/profile/profile.component').then(m => m.ProfileComponent)
  },
  // Host Routing (Root /host is public, children require Host role)
  {
    path: 'host',
    children: [
      {
        path: '', // maps to /host
        loadComponent: () => import('./domains/properties/pages/host/host.component').then(m => m.HostComponent)
      },
      {
        path: 'dashboard',
        canActivate: [authGuard, roleGuard],
        data: { roles: ['Host'] },
        loadComponent: () => import('./domains/properties/pages/host-dashboard/host-dashboard').then(m => m.HostDashboardComponent)
      },
      {
        path: 'create',
        canActivate: [authGuard, roleGuard],
        data: { roles: ['Host'] },
        loadComponent: () => import('./domains/properties/pages/host-listing-create/host-listing-create').then(m => m.HostListingCreateComponent)
      },
      {
        path: 'edit/:id',
        canActivate: [authGuard, roleGuard],
        data: { roles: ['Host'] },
        loadComponent: () => import('./domains/properties/pages/host-edit-listing/host-edit-listing').then(m => m.HostEditListingComponent)
      }
    ]
  },
  {
    path: 'reservations',
    loadComponent: () => import('./domains/booking/pages/my-trips/my-trips').then(m => m.MyTripsComponent)
  },


  // Default redirect (fallback)
  { path: '**', redirectTo: '' },
];
