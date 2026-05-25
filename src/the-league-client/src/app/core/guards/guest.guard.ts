import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

export const guestGuard: CanActivateFn = () => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (!authService.isAuthenticated()) {
    return true;
  }

  // Redirect authenticated users to their portal
  const role = authService.userRole();
  switch (role) {
    case 'SuperAdmin': return router.createUrlTree(['/admin']);
    case 'ClubManager': return router.createUrlTree(['/club']);
    default: return router.createUrlTree(['/portal']);
  }
};
