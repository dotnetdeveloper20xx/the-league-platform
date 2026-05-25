import { Component } from '@angular/core';

@Component({
  selector: 'app-club-dashboard',
  standalone: true,
  template: `
    <div class="space-y-8">
      <!-- Page Header -->
      <div class="page-header">
        <div>
          <h1 class="page-title">Club Dashboard</h1>
          <p class="page-subtitle">Welcome back! Here's how your club is performing.</p>
        </div>
        <div class="flex items-center gap-3">
          <span class="live-indicator">Live</span>
          <button class="btn btn-primary btn-sm btn-glow">+ New Session</button>
        </div>
      </div>

      <!-- KPI Stats with Animated Cards -->
      <div class="kpi-grid">
        <div class="stat-animated">
          <div class="stat-figure text-primary">
            <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" class="inline-block w-8 h-8 stroke-current">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0z"></path>
            </svg>
          </div>
          <div class="stat-title">Active Members</div>
          <div class="stat-value">124</div>
          <div class="stat-desc text-success">↑ 8 this month</div>
        </div>

        <div class="stat-animated">
          <div class="stat-figure text-secondary">
            <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" class="inline-block w-8 h-8 stroke-current">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8c-1.657 0-3 .895-3 2s1.343 2 3 2 3 .895 3 2-1.343 2-3 2m0-8c1.11 0 2.08.402 2.599 1M12 8V7m0 1v8m0 0v1m0-1c-1.11 0-2.08-.402-2.599-1M21 12a9 9 0 11-18 0 9 9 0 0118 0z"></path>
            </svg>
          </div>
          <div class="stat-title">Monthly Revenue</div>
          <div class="stat-value">£4,280</div>
          <div class="stat-desc text-success">↑ 12% from last month</div>
        </div>

        <div class="stat-animated">
          <div class="stat-figure text-accent">
            <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" class="inline-block w-8 h-8 stroke-current">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 19v-6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2a2 2 0 002-2zm0 0V9a2 2 0 012-2h2a2 2 0 012 2v10m-6 0a2 2 0 002 2h2a2 2 0 002-2m0 0V5a2 2 0 012-2h2a2 2 0 012 2v14a2 2 0 01-2 2h-2a2 2 0 01-2-2z"></path>
            </svg>
          </div>
          <div class="stat-title">Session Attendance</div>
          <div class="stat-value">78%</div>
          <div class="stat-desc text-warning">↓ 3% from last week</div>
        </div>

        <div class="stat-animated">
          <div class="stat-figure text-error">
            <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" class="inline-block w-8 h-8 stroke-current">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-2.5L13.732 4c-.77-.833-1.964-.833-2.732 0L4.082 16.5c-.77.833.192 2.5 1.732 2.5z"></path>
            </svg>
          </div>
          <div class="stat-title">Outstanding</div>
          <div class="stat-value">£1,450</div>
          <div class="stat-desc text-error">5 overdue invoices</div>
        </div>
      </div>

      <!-- Club Health Insight Card -->
      <div class="card-animated bg-gradient-to-r from-primary/5 via-base-100 to-secondary/5 border-primary/20">
        <div class="card-body">
          <div class="flex items-center justify-between">
            <div class="flex items-center gap-3">
              <div class="w-14 h-14 rounded-full bg-primary/10 flex items-center justify-center">
                <span class="text-2xl">💪</span>
              </div>
              <div>
                <h3 class="font-bold text-lg">Club Health Score</h3>
                <p class="text-base-content/60 text-sm">Based on retention, payments, attendance & engagement</p>
              </div>
            </div>
            <div class="text-right">
              <div class="text-4xl font-black text-primary">82</div>
              <div class="text-xs text-base-content/60">out of 100</div>
            </div>
          </div>

          <!-- Health Metrics -->
          <div class="grid grid-cols-2 md:grid-cols-4 gap-4 mt-4">
            <div class="text-center">
              <div class="radial-progress text-success text-sm" style="--value:92; --size:3rem; --thickness:4px;" role="progressbar">92%</div>
              <p class="text-xs mt-1 text-base-content/60">Retention</p>
            </div>
            <div class="text-center">
              <div class="radial-progress text-info text-sm" style="--value:85; --size:3rem; --thickness:4px;" role="progressbar">85%</div>
              <p class="text-xs mt-1 text-base-content/60">Payments</p>
            </div>
            <div class="text-center">
              <div class="radial-progress text-warning text-sm" style="--value:78; --size:3rem; --thickness:4px;" role="progressbar">78%</div>
              <p class="text-xs mt-1 text-base-content/60">Attendance</p>
            </div>
            <div class="text-center">
              <div class="radial-progress text-accent text-sm" style="--value:73; --size:3rem; --thickness:4px;" role="progressbar">73%</div>
              <p class="text-xs mt-1 text-base-content/60">Engagement</p>
            </div>
          </div>

          <!-- Actionable Insights -->
          <div class="divider my-2"></div>
          <div class="flex flex-wrap gap-2">
            <div class="badge badge-warning badge-outline gap-1">
              <span>⚠️</span> 3 members at risk of leaving
            </div>
            <div class="badge badge-error badge-outline gap-1">
              <span>💰</span> £1,450 outstanding — send reminders
            </div>
            <div class="badge badge-success badge-outline gap-1">
              <span>🎉</span> Retention up 4% this quarter
            </div>
          </div>
        </div>
      </div>

      <!-- Charts & Activity Row -->
      <div class="content-grid">
        <!-- Revenue Chart -->
        <div class="lg:col-span-2 card-animated">
          <div class="card-body">
            <div class="flex items-center justify-between">
              <h2 class="card-title">Revenue Trends</h2>
              <select class="select select-bordered select-sm">
                <option>Last 12 months</option>
                <option>Last 6 months</option>
                <option>This quarter</option>
              </select>
            </div>
            <div class="h-56 flex items-center justify-center border-2 border-dashed border-base-300 rounded-xl mt-4">
              <div class="text-center">
                <div class="text-4xl mb-2">📈</div>
                <p class="text-base-content/50 text-sm">Interactive Chart.js line chart</p>
                <p class="text-xs text-base-content/40 mt-1">Revenue data from 400+ payments</p>
              </div>
            </div>
          </div>
        </div>

        <!-- Activity Feed -->
        <div class="card-animated">
          <div class="card-body">
            <h2 class="card-title">Live Activity</h2>
            <div class="activity-feed">
              @for (activity of recentActivity; track activity.id) {
                <div class="activity-item">
                  <div class="w-8 h-8 rounded-full flex items-center justify-center text-sm" [class]="activity.bgClass">
                    {{ activity.emoji }}
                  </div>
                  <div class="flex-1 min-w-0">
                    <p class="text-sm font-medium truncate">{{ activity.message }}</p>
                    <p class="text-xs text-base-content/50">{{ activity.time }}</p>
                  </div>
                </div>
              }
            </div>
          </div>
        </div>
      </div>

      <!-- Upcoming & Achievements Row -->
      <div class="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <!-- Upcoming Sessions -->
        <div class="card-animated">
          <div class="card-body">
            <div class="flex items-center justify-between">
              <h2 class="card-title">Upcoming Sessions</h2>
              <a class="btn btn-ghost btn-xs">View All →</a>
            </div>
            <div class="space-y-3 mt-2">
              @for (session of upcomingSessions; track session.title) {
                <div class="flex items-center gap-3 p-3 rounded-lg bg-base-200/50 hover:bg-base-200 transition-colors">
                  <div class="w-10 h-10 rounded-lg bg-primary/10 flex items-center justify-center">
                    <span class="text-lg">{{ session.emoji }}</span>
                  </div>
                  <div class="flex-1">
                    <p class="font-medium text-sm">{{ session.title }}</p>
                    <p class="text-xs text-base-content/60">{{ session.time }} · {{ session.venue }}</p>
                  </div>
                  <div class="text-right">
                    <p class="text-sm font-semibold">{{ session.booked }}/{{ session.capacity }}</p>
                    <p class="text-xs text-base-content/50">booked</p>
                  </div>
                </div>
              }
            </div>
          </div>
        </div>

        <!-- Club Achievements -->
        <div class="card-animated">
          <div class="card-body">
            <h2 class="card-title">Recent Achievements 🏆</h2>
            <div class="space-y-3 mt-2">
              <div class="flex items-center gap-3 p-3 rounded-lg bg-accent/5 border border-accent/20">
                <span class="text-2xl">🎯</span>
                <div>
                  <p class="font-medium text-sm">100+ Active Members</p>
                  <p class="text-xs text-base-content/60">Milestone reached this month!</p>
                </div>
                <span class="achievement-badge ml-auto">NEW</span>
              </div>
              <div class="flex items-center gap-3 p-3 rounded-lg bg-success/5 border border-success/20">
                <span class="text-2xl">💰</span>
                <div>
                  <p class="font-medium text-sm">£50K Annual Revenue</p>
                  <p class="text-xs text-base-content/60">Crossed the threshold in March</p>
                </div>
              </div>
              <div class="flex items-center gap-3 p-3 rounded-lg bg-info/5 border border-info/20">
                <span class="text-2xl">📅</span>
                <div>
                  <p class="font-medium text-sm">500 Sessions Hosted</p>
                  <p class="text-xs text-base-content/60">Since joining The League</p>
                </div>
              </div>
              <div class="flex items-center gap-3 p-3 rounded-lg bg-primary/5 border border-primary/20">
                <span class="text-2xl">⭐</span>
                <div>
                  <p class="font-medium text-sm">Top 10% Retention Rate</p>
                  <p class="text-xs text-base-content/60">Better than 90% of clubs on the platform</p>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  `
})
export class ClubDashboardComponent {
  recentActivity = [
    { id: 1, emoji: '👤', bgClass: 'bg-primary/10', message: 'James Wilson joined as a new member', time: '2 minutes ago' },
    { id: 2, emoji: '💳', bgClass: 'bg-success/10', message: '£285 membership payment received', time: '15 minutes ago' },
    { id: 3, emoji: '📅', bgClass: 'bg-info/10', message: 'Senior Nets Practice fully booked (24/24)', time: '1 hour ago' },
    { id: 4, emoji: '🎉', bgClass: 'bg-accent/10', message: 'Annual Awards Dinner — 12 new registrations', time: '2 hours ago' },
    { id: 5, emoji: '⚠️', bgClass: 'bg-warning/10', message: 'Invoice #INV-042 is now overdue', time: '3 hours ago' },
    { id: 6, emoji: '🏆', bgClass: 'bg-secondary/10', message: 'Teddington 1st XI won vs Hampton Hill', time: '5 hours ago' },
    { id: 7, emoji: '📧', bgClass: 'bg-neutral/10', message: 'Bulk campaign sent to 45 members', time: 'Yesterday' },
  ];

  upcomingSessions = [
    { emoji: '🏏', title: 'Senior Nets Practice', time: 'Tue 18:00', venue: 'Indoor Nets', booked: 18, capacity: 24 },
    { emoji: '👶', title: 'Junior Coaching', time: 'Wed 16:00', venue: 'Main Ground', booked: 22, capacity: 30 },
    { emoji: '🏋️', title: 'Weekend Match Prep', time: 'Thu 18:00', venue: 'Practice Nets', booked: 14, capacity: 20 },
    { emoji: '🎯', title: 'MCC Masterclass', time: 'Mon 10:00', venue: 'Indoor School', booked: 11, capacity: 12 },
  ];
}
