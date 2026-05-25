import { Component } from '@angular/core';

@Component({
  selector: 'app-club-dashboard',
  standalone: true,
  template: `
    <div class="space-y-6">
      <div>
        <h1 class="text-2xl font-bold">Club Dashboard</h1>
        <p class="text-base-content/70">Club management overview</p>
      </div>

      <!-- KPI Stats -->
      <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
        <div class="stat bg-base-100 shadow rounded-box">
          <div class="stat-figure text-primary">
            <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" class="inline-block w-8 h-8 stroke-current">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0z"></path>
            </svg>
          </div>
          <div class="stat-title">Active Members</div>
          <div class="stat-value text-primary">{{ activeMembers }}</div>
          <div class="stat-desc">+8 this month</div>
        </div>

        <div class="stat bg-base-100 shadow rounded-box">
          <div class="stat-figure text-secondary">
            <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" class="inline-block w-8 h-8 stroke-current">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8c-1.657 0-3 .895-3 2s1.343 2 3 2 3 .895 3 2-1.343 2-3 2m0-8c1.11 0 2.08.402 2.599 1M12 8V7m0 1v8m0 0v1m0-1c-1.11 0-2.08-.402-2.599-1M21 12a9 9 0 11-18 0 9 9 0 0118 0z"></path>
            </svg>
          </div>
          <div class="stat-title">Monthly Revenue</div>
          <div class="stat-value text-secondary">£{{ monthlyRevenue }}</div>
          <div class="stat-desc">+12% from last month</div>
        </div>

        <div class="stat bg-base-100 shadow rounded-box">
          <div class="stat-figure text-accent">
            <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" class="inline-block w-8 h-8 stroke-current">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 19v-6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2a2 2 0 002-2zm0 0V9a2 2 0 012-2h2a2 2 0 012 2v10m-6 0a2 2 0 002 2h2a2 2 0 002-2m0 0V5a2 2 0 012-2h2a2 2 0 012 2v14a2 2 0 01-2 2h-2a2 2 0 01-2-2z"></path>
            </svg>
          </div>
          <div class="stat-title">Session Attendance</div>
          <div class="stat-value text-accent">{{ attendanceRate }}%</div>
          <div class="stat-desc">Average rate this week</div>
        </div>

        <div class="stat bg-base-100 shadow rounded-box">
          <div class="stat-figure text-warning">
            <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" class="inline-block w-8 h-8 stroke-current">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-2.5L13.732 4c-.77-.833-1.964-.833-2.732 0L4.082 16.5c-.77.833.192 2.5 1.732 2.5z"></path>
            </svg>
          </div>
          <div class="stat-title">Outstanding Balance</div>
          <div class="stat-value text-warning">£{{ outstandingBalance }}</div>
          <div class="stat-desc">Across all members</div>
        </div>
      </div>

      <!-- Charts Row -->
      <div class="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <!-- Revenue Trends -->
        <div class="card bg-base-100 shadow">
          <div class="card-body">
            <h2 class="card-title">Revenue Trends</h2>
            <div class="h-48 flex items-center justify-center border-2 border-dashed border-base-300 rounded-lg">
              <p class="text-base-content/50">Line chart — coming soon</p>
            </div>
          </div>
        </div>

        <!-- Membership Growth -->
        <div class="card bg-base-100 shadow">
          <div class="card-body">
            <h2 class="card-title">Membership Growth</h2>
            <div class="h-48 flex items-center justify-center border-2 border-dashed border-base-300 rounded-lg">
              <p class="text-base-content/50">Area chart — coming soon</p>
            </div>
          </div>
        </div>
      </div>

      <!-- Attendance Heatmap & Activity -->
      <div class="grid grid-cols-1 lg:grid-cols-3 gap-6">
        <!-- Attendance Heatmap -->
        <div class="lg:col-span-2 card bg-base-100 shadow">
          <div class="card-body">
            <h2 class="card-title">Attendance Heatmap</h2>
            <div class="h-48 flex items-center justify-center border-2 border-dashed border-base-300 rounded-lg">
              <p class="text-base-content/50">Heatmap visualization — coming soon</p>
            </div>
          </div>
        </div>

        <!-- Recent Activity -->
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
export class ClubDashboardComponent {
  activeMembers = 124;
  monthlyRevenue = '4,280';
  attendanceRate = 78;
  outstandingBalance = '1,450';

  recentActivity = [
    { id: 1, type: 'Member', badgeClass: 'badge-primary', message: 'James Wilson joined', time: '10 minutes ago' },
    { id: 2, type: 'Payment', badgeClass: 'badge-success', message: '£45 membership payment received', time: '30 minutes ago' },
    { id: 3, type: 'Session', badgeClass: 'badge-accent', message: 'Net Practice session fully booked', time: '1 hour ago' },
    { id: 4, type: 'Event', badgeClass: 'badge-info', message: 'Annual Dinner tickets on sale', time: '2 hours ago' },
    { id: 5, type: 'Invoice', badgeClass: 'badge-warning', message: 'Invoice #INV-042 overdue', time: '3 hours ago' },
  ];
}
