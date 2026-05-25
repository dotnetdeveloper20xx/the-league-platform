import { Routes } from '@angular/router';

export const ADMIN_ROUTES: Routes = [
  { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
  {
    path: 'dashboard',
    loadComponent: () => import('./pages/dashboard/admin-dashboard.component').then(m => m.AdminDashboardComponent)
  },
  {
    path: 'clubs',
    loadComponent: () => import('./pages/clubs/clubs-list.component').then(m => m.ClubsListComponent)
  },
  {
    path: 'users',
    loadComponent: () => import('./pages/users/users-list.component').then(m => m.UsersListComponent)
  },
  {
    path: 'subscriptions',
    loadComponent: () => import('./pages/subscriptions/subscriptions.component').then(m => m.SubscriptionsComponent)
  },
  {
    path: 'reports',
    loadComponent: () => import('./pages/reports/reports.component').then(m => m.ReportsComponent)
  },
  {
    path: 'settings',
    loadComponent: () => import('./pages/settings/settings.component').then(m => m.SettingsComponent)
  }
];
