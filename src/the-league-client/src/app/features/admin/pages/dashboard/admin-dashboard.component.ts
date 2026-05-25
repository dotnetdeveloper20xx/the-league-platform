import { Component } from '@angular/core';

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  template: `
    <div class="space-y-6">
      <div>
        <h1 class="text-2xl font-bold">Platform Overview</h1>
        <p class="text-base-content/70">SuperAdmin Dashboard</p>
      </div>

      <!-- KPI Stats -->
      <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
        <div class="stat bg-base-100 shadow rounded-box">
          <div class="stat-figure text-primary">
            <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" class="inline-block w-8 h-8 stroke-current">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 21V5a2 2 0 00-2-2H7a2 2 0 00-2 2v16m14 0h2m-2 0h-5m-9 0H3m2 0h5M9 7h1m-1 4h1m4-4h1m-1 4h1m-5 10v-5a1 1 0 011-1h2a1 1 0 011 1v5m-4 0h4"></path>
            </svg>
          </div>
          <div class="stat-title">Total Clubs</div>
          <div class="stat-value text-primary">{{ totalClubs }}</div>
          <div class="stat-desc">+12 this month</div>
        </div>

        <div class="stat bg-base-100 shadow rounded-box">
          <div class="stat-figure text-secondary">
            <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" class="inline-block w-8 h-8 stroke-current">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0z"></path>
            </svg>
          </div>
          <div class="stat-title">Total Members</div>
          <div class="stat-value text-secondary">{{ totalMembers }}</div>
          <div class="stat-desc">Across all clubs</div>
        </div>

        <div class="stat bg-base-100 shadow rounded-box">
          <div class="stat-figure text-accent">
            <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" class="inline-block w-8 h-8 stroke-current">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8c-1.657 0-3 .895-3 2s1.343 2 3 2 3 .895 3 2-1.343 2-3 2m0-8c1.11 0 2.08.402 2.599 1M12 8V7m0 1v8m0 0v1m0-1c-1.11 0-2.08-.402-2.599-1M21 12a9 9 0 11-18 0 9 9 0 0118 0z"></path>
            </svg>
          </div>
          <div class="stat-title">Platform Revenue</div>
          <div class="stat-value text-accent">£{{ platformRevenue }}</div>
          <div class="stat-desc">This month</div>
        </div>

        <div class="stat bg-base-100 shadow rounded-box">
          <div class="stat-figure text-info">
            <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" class="inline-block w-8 h-8 stroke-current">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m5.618-4.016A11.955 11.955 0 0112 2.944a11.955 11.955 0 01-8.618 3.04A12.02 12.02 0 003 9c0 5.591 3.824 10.29 9 11.622 5.176-1.332 9-6.03 9-11.622 0-1.042-.133-2.052-.382-3.016z"></path>
            </svg>
          </div>
          <div class="stat-title">Active Subscriptions</div>
          <div class="stat-value text-info">{{ activeSubscriptions }}</div>
          <div class="stat-desc">Paid tiers</div>
        </div>
      </div>

      <!-- Chart Placeholder & Activity Feed -->
      <div class="grid grid-cols-1 lg:grid-cols-3 gap-6">
        <!-- Chart Area -->
        <div class="lg:col-span-2 card bg-base-100 shadow">
          <div class="card-body">
            <h2 class="card-title">Revenue Trends</h2>
            <div class="h-64 flex items-center justify-center border-2 border-dashed border-base-300 rounded-lg">
              <p class="text-base-content/50">Chart.js integration coming soon</p>
            </div>
          </div>
        </div>

        <!-- Recent Activity Feed -->
        <div class="card bg-base-100 shadow">
          <div class="card-body">
            <h2 class="card-title">Recent Activity</h2>
            <ul class="space-y-3">
              @for (activity of recentActivity; track activity.id) {
                <li class="flex items-start gap-3 p-2 rounded-lg hover:bg-base-200">
                  <div class="badge badge-sm" [class]="activity.badgeClass">{{ activity.type }}</div>
                  <div>
                    <p class="text-sm font-medium">{{ activity.message }}</p>
                    <p class="text-xs text-base-content/50">{{ activity.time }}</p>
                  </div>
                </li>
              }
            </ul>
          </div>
        </div>
      </div>
    </div>
  `
})
export class AdminDashboardComponent {
  totalClubs = 47;
  totalMembers = '3,842';
  platformRevenue = '12,450';
  activeSubscriptions = 38;

  recentActivity = [
    { id: 1, type: 'Club', badgeClass: 'badge-primary', message: 'Riverside CC registered', time: '2 minutes ago' },
    { id: 2, type: 'Sub', badgeClass: 'badge-secondary', message: 'Oakfield FC upgraded to Pro', time: '15 minutes ago' },
    { id: 3, type: 'User', badgeClass: 'badge-accent', message: 'New admin user created', time: '1 hour ago' },
    { id: 4, type: 'Pay', badgeClass: 'badge-info', message: '£199 subscription payment received', time: '2 hours ago' },
    { id: 5, type: 'Alert', badgeClass: 'badge-warning', message: 'Dunning retry failed for Valley HC', time: '3 hours ago' },
  ];
}
