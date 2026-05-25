import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { ApiService } from '../../../../core/services/api.service';
import { SkeletonLoaderComponent } from '../../../../shared/components/skeleton-loader/skeleton-loader.component';
import { StatusBadgeComponent } from '../../../../shared/components/status-badge/status-badge.component';
import { EmptyStateComponent } from '../../../../shared/components/empty-state/empty-state.component';

interface MembershipStatus {
  type: string;
  expiryDate: string;
  status: string;
}

interface UpcomingBooking {
  id: string;
  sessionTitle: string;
  date: string;
  time: string;
  venue: string;
}

interface UpcomingEvent {
  id: string;
  title: string;
  date: string;
  type: string;
}

interface BalanceSummary {
  outstanding: number;
  currency: string;
}

@Component({
  selector: 'app-portal-dashboard',
  standalone: true,
  imports: [CommonModule, RouterLink, SkeletonLoaderComponent, StatusBadgeComponent, EmptyStateComponent],
  template: `
    <div class="space-y-6">
      <h1 class="text-2xl font-bold">My Dashboard</h1>

      <!-- Stats Row -->
      <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
        <!-- Membership Status Card -->
        <div class="card bg-base-100 shadow-sm">
          <div class="card-body">
            <h2 class="card-title text-sm font-medium text-base-content/70">Membership</h2>
            @if (loading()) {
              <app-skeleton-loader type="text" [count]="2" />
            } @else if (membership()) {
              <p class="text-lg font-semibold">{{ membership()!.type }}</p>
              <div class="flex items-center gap-2">
                <app-status-badge [status]="membership()!.status" />
              </div>
              <p class="text-xs text-base-content/60">Expires: {{ membership()!.expiryDate }}</p>
            } @else {
              <p class="text-base-content/60">No active membership</p>
            }
          </div>
        </div>

        <!-- Outstanding Balance Card -->
        <div class="card bg-base-100 shadow-sm">
          <div class="card-body">
            <h2 class="card-title text-sm font-medium text-base-content/70">Balance</h2>
            @if (loading()) {
              <app-skeleton-loader type="text" [count]="1" />
            } @else {
              <p class="text-2xl font-bold" [class.text-error]="(balance()?.outstanding ?? 0) > 0">
                {{ balance()?.currency ?? '£' }}{{ (balance()?.outstanding ?? 0).toFixed(2) }}
              </p>
              @if ((balance()?.outstanding ?? 0) > 0) {
                <div class="card-actions justify-end">
                  <a routerLink="/portal/payments" class="btn btn-primary btn-sm">Pay Now</a>
                </div>
              }
            }
          </div>
        </div>

        <!-- Bookings Count -->
        <div class="card bg-base-100 shadow-sm">
          <div class="card-body">
            <h2 class="card-title text-sm font-medium text-base-content/70">Upcoming Bookings</h2>
            @if (loading()) {
              <app-skeleton-loader type="text" [count]="1" />
            } @else {
              <p class="text-2xl font-bold">{{ bookings().length }}</p>
              <p class="text-xs text-base-content/60">Next 7 days</p>
            }
          </div>
        </div>

        <!-- Events Count -->
        <div class="card bg-base-100 shadow-sm">
          <div class="card-body">
            <h2 class="card-title text-sm font-medium text-base-content/70">Upcoming Events</h2>
            @if (loading()) {
              <app-skeleton-loader type="text" [count]="1" />
            } @else {
              <p class="text-2xl font-bold">{{ events().length }}</p>
              <p class="text-xs text-base-content/60">Next 30 days</p>
            }
          </div>
        </div>
      </div>

      <!-- Content Row -->
      <div class="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <!-- Upcoming Bookings -->
        <div class="card bg-base-100 shadow-sm">
          <div class="card-body">
            <div class="flex items-center justify-between">
              <h2 class="card-title">Upcoming Bookings</h2>
              <a routerLink="/portal/sessions" class="btn btn-ghost btn-sm">View All</a>
            </div>
            @if (loading()) {
              <app-skeleton-loader type="table-row" [count]="3" />
            } @else if (bookings().length === 0) {
              <app-empty-state
                icon="📅"
                title="No upcoming bookings"
                message="Browse available sessions to book your next activity"
                actionLabel="Browse Sessions"
                (actionClick)="navigateToSessions()" />
            } @else {
              <div class="overflow-x-auto">
                <table class="table table-sm">
                  <thead>
                    <tr>
                      <th>Session</th>
                      <th>Date</th>
                      <th>Time</th>
                      <th>Venue</th>
                    </tr>
                  </thead>
                  <tbody>
                    @for (booking of bookings(); track booking.id) {
                      <tr>
                        <td class="font-medium">{{ booking.sessionTitle }}</td>
                        <td>{{ booking.date }}</td>
                        <td>{{ booking.time }}</td>
                        <td>{{ booking.venue }}</td>
                      </tr>
                    }
                  </tbody>
                </table>
              </div>
            }
          </div>
        </div>

        <!-- Upcoming Events -->
        <div class="card bg-base-100 shadow-sm">
          <div class="card-body">
            <div class="flex items-center justify-between">
              <h2 class="card-title">Upcoming Events</h2>
              <a routerLink="/portal/events" class="btn btn-ghost btn-sm">View All</a>
            </div>
            @if (loading()) {
              <app-skeleton-loader type="table-row" [count]="3" />
            } @else if (events().length === 0) {
              <app-empty-state
                icon="🎉"
                title="No upcoming events"
                message="Check back soon for new events and activities" />
            } @else {
              <div class="overflow-x-auto">
                <table class="table table-sm">
                  <thead>
                    <tr>
                      <th>Event</th>
                      <th>Date</th>
                      <th>Type</th>
                    </tr>
                  </thead>
                  <tbody>
                    @for (event of events(); track event.id) {
                      <tr>
                        <td class="font-medium">{{ event.title }}</td>
                        <td>{{ event.date }}</td>
                        <td><app-status-badge [status]="event.type" /></td>
                      </tr>
                    }
                  </tbody>
                </table>
              </div>
            }
          </div>
        </div>
      </div>
    </div>
  `
})
export class PortalDashboardComponent implements OnInit {
  private api = inject(ApiService);

  loading = signal(true);
  membership = signal<MembershipStatus | null>(null);
  bookings = signal<UpcomingBooking[]>([]);
  events = signal<UpcomingEvent[]>([]);
  balance = signal<BalanceSummary | null>(null);

  ngOnInit(): void {
    this.loadDashboardData();
  }

  navigateToSessions(): void {
    window.location.href = '/portal/sessions';
  }

  private loadDashboardData(): void {
    this.api.get<any>('portal/dashboard').subscribe({
      next: (data) => {
        this.membership.set(data.membership ?? null);
        this.bookings.set(data.upcomingBookings ?? []);
        this.events.set(data.upcomingEvents ?? []);
        this.balance.set(data.balance ?? null);
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
      }
    });
  }
}
