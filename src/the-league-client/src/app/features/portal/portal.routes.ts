import { Routes } from '@angular/router';

export const PORTAL_ROUTES: Routes = [
  { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
  {
    path: 'dashboard',
    loadComponent: () => import('./pages/dashboard/portal-dashboard.component').then(m => m.PortalDashboardComponent)
  }
];
