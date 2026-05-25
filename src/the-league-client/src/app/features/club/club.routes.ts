import { Routes } from '@angular/router';

export const CLUB_ROUTES: Routes = [
  { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
  {
    path: 'dashboard',
    loadComponent: () => import('./pages/dashboard/club-dashboard.component').then(m => m.ClubDashboardComponent)
  }
];
