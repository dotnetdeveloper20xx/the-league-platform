import { Component, signal } from '@angular/core';
import { RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-portal-layout',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  templateUrl: './portal-layout.component.html'
})
export class PortalLayoutComponent {
  mobileMenuOpen = signal(false);

  readonly menuItems = [
    { label: 'Dashboard', route: '/portal/dashboard' },
    { label: 'Sessions', route: '/portal/sessions' },
    { label: 'Events', route: '/portal/events' },
    { label: 'Payments', route: '/portal/payments' },
    { label: 'Family', route: '/portal/family' },
    { label: 'Profile', route: '/portal/profile' },
    { label: 'Settings', route: '/portal/settings' }
  ];

  constructor(public authService: AuthService) {}

  toggleMobileMenu(): void {
    this.mobileMenuOpen.update(v => !v);
  }

  logout(): void {
    this.authService.logout();
  }
}
