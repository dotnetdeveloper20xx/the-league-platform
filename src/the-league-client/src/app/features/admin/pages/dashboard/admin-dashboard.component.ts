import { Component } from '@angular/core';

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  template: `
    <div>
      <h1 class="text-2xl font-bold mb-4">Admin Dashboard</h1>
      <p class="text-base-content/70">Platform administration overview</p>
    </div>
  `
})
export class AdminDashboardComponent {}
