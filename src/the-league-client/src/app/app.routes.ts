import { Routes } from '@angular/router';
import { authGuard, roleGuard, guestGuard } from './core/guards';

export const routes: Routes = [
  { path: '', redirectTo: '/auth/login', pathMatch: 'full' },
  {
    path: 'auth',
    canActivate: [guestGuard],
    loadChildren: () => import('./features/auth/auth.routes').then(m => m.AUTH_ROUTES)
  },
  {
    path: 'admin',
    canActivate: [authGuard, roleGuard],
    data: { roles: ['SuperAdmin'] },
    loadComponent: () => import('./layouts/admin-layout/admin-layout.component').then(m => m.AdminLayoutComponent),
    loadChildren: () => import('./features/admin/admin.routes').then(m => m.ADMIN_ROUTES)
  },
  {
    path: 'club',
    canActivate: [authGuard, roleGuard],
    data: { roles: ['ClubManager'] },
    loadComponent: () => import('./layouts/club-layout/club-layout.component').then(m => m.ClubLayoutComponent),
    loadChildren: () => import('./features/club/club.routes').then(m => m.CLUB_ROUTES)
  },
  {
    path: 'portal',
    canActivate: [authGuard, roleGuard],
    data: { roles: ['Member'] },
    loadComponent: () => import('./layouts/portal-layout/portal-layout.component').then(m => m.PortalLayoutComponent),
    loadChildren: () => import('./features/portal/portal.routes').then(m => m.PORTAL_ROUTES)
  },
  {
    path: 'match/:id',
    loadComponent: () => import('./features/match-centre/match-centre.component').then(m => m.MatchCentreComponent)
  },
  {
    path: 'onboarding',
    loadComponent: () => import('./features/public/onboarding/onboarding.component').then(m => m.OnboardingComponent)
  },
  {
    path: 'unauthorized',
    loadComponent: () => import('./layouts/unauthorized/unauthorized.component').then(m => m.UnauthorizedComponent)
  },
  { path: '**', redirectTo: '/auth/login' }
];
