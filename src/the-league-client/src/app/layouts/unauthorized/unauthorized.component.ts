import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-unauthorized',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './unauthorized.component.html'
})
export class UnauthorizedComponent {
  constructor(public authService: AuthService) {}

  get redirectLink(): string {
    const role = this.authService.userRole();
    switch (role) {
      case 'SuperAdmin': return '/admin';
      case 'ClubManager': return '/club';
      case 'Member':
      case 'Coach':
      case 'Staff':
        return '/portal';
      default: return '/auth/login';
    }
  }

  get redirectLabel(): string {
    if (this.authService.isAuthenticated()) {
      return 'Go to Dashboard';
    }
    return 'Go to Login';
  }
}
