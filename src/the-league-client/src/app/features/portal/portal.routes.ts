import { Routes } from '@angular/router';

export const PORTAL_ROUTES: Routes = [
  { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
  {
    path: 'dashboard',
    loadComponent: () => import('./pages/dashboard/portal-dashboard.component').then(m => m.PortalDashboardComponent)
  },
  {
    path: 'sessions',
    loadComponent: () => import('./pages/sessions/portal-sessions.component').then(m => m.PortalSessionsComponent)
  },
  {
    path: 'events',
    loadComponent: () => import('./pages/events/portal-events.component').then(m => m.PortalEventsComponent)
  },
  {
    path: 'payments',
    loadComponent: () => import('./pages/payments/portal-payments.component').then(m => m.PortalPaymentsComponent)
  },
  {
    path: 'family',
    loadComponent: () => import('./pages/family/family.component').then(m => m.FamilyComponent)
  },
  {
    path: 'profile',
    loadComponent: () => import('./pages/profile/profile.component').then(m => m.ProfileComponent)
  },
  {
    path: 'settings',
    loadComponent: () => import('./pages/settings/portal-settings.component').then(m => m.PortalSettingsComponent)
  }
];
