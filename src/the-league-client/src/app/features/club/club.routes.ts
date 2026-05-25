import { Routes } from '@angular/router';

export const CLUB_ROUTES: Routes = [
  { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
  {
    path: 'dashboard',
    loadComponent: () => import('./pages/dashboard/club-dashboard.component').then(m => m.ClubDashboardComponent)
  },
  {
    path: 'members',
    loadComponent: () => import('./pages/members/members-list.component').then(m => m.MembersListComponent)
  },
  {
    path: 'memberships',
    loadComponent: () => import('./pages/memberships/memberships.component').then(m => m.MembershipsComponent)
  },
  {
    path: 'sessions',
    loadComponent: () => import('./pages/sessions/sessions-list.component').then(m => m.SessionsListComponent)
  },
  {
    path: 'events',
    loadComponent: () => import('./pages/events/events-list.component').then(m => m.EventsListComponent)
  },
  {
    path: 'competitions',
    loadComponent: () => import('./pages/competitions/competitions-list.component').then(m => m.CompetitionsListComponent)
  },
  {
    path: 'payments',
    loadComponent: () => import('./pages/payments/payments-list.component').then(m => m.PaymentsListComponent)
  },
  {
    path: 'invoices',
    loadComponent: () => import('./pages/invoices/invoices-list.component').then(m => m.InvoicesListComponent)
  },
  {
    path: 'facilities',
    loadComponent: () => import('./pages/facilities/facilities-list.component').then(m => m.FacilitiesListComponent)
  },
  {
    path: 'equipment',
    loadComponent: () => import('./pages/equipment/equipment-list.component').then(m => m.EquipmentListComponent)
  },
  {
    path: 'programs',
    loadComponent: () => import('./pages/programs/programs-list.component').then(m => m.ProgramsListComponent)
  },
  {
    path: 'communications',
    loadComponent: () => import('./pages/communications/communications.component').then(m => m.CommunicationsComponent)
  },
  {
    path: 'shop',
    loadComponent: () => import('./pages/shop/shop-management.component').then(m => m.ShopManagementComponent)
  },
  {
    path: 'reports',
    loadComponent: () => import('./pages/reports/reports.component').then(m => m.ClubReportsComponent)
  },
  {
    path: 'settings',
    loadComponent: () => import('./pages/settings/settings.component').then(m => m.ClubSettingsComponent)
  }
];
