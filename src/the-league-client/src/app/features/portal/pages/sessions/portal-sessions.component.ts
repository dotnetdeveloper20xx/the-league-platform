import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ApiService } from '../../../../core/services/api.service';
import { SkeletonLoaderComponent } from '../../../../shared/components/skeleton-loader/skeleton-loader.component';
import { EmptyStateComponent } from '../../../../shared/components/empty-state/empty-state.component';

interface AvailableSession {
  id: string;
  title: string;
  date: string;
  time: string;
  venue: string;
  capacity: number;
  bookedCount: number;
  fee: number;
  currency: string;
}

interface MyBooking {
  id: string;
  sessionId: string;
  sessionTitle: string;
  date: string;
  time: string;
  venue: string;
  status: string;
  canCancel: boolean;
}

@Component({
  selector: 'app-portal-sessions',
  standalone: true,
  imports: [CommonModule, SkeletonLoaderComponent, EmptyStateComponent],
  template: `
    <div class="space-y-6">
      <h1 class="text-2xl font-bold">Sessions</h1>

      <!-- Tabs -->
      <div role="tablist" class="tabs tabs-bordered">
        <button
          role="tab"
          class="tab"
          [class.tab-active]="activeTab() === 'browse'"
          (click)="activeTab.set('browse')">
          Browse Sessions
        </button>
        <button
          role="tab"
          class="tab"
          [class.tab-active]="activeTab() === 'my-bookings'"
          (click)="activeTab.set('my-bookings')">
          My Bookings
        </button>
      </div>

      <!-- Browse Sessions Tab -->
      @if (activeTab() === 'browse') {
        @if (loadingSessions()) {
          <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
            <app-skeleton-loader type="card" [count]="6" />
          </div>
        } @else if (sessions().length === 0) {
          <app-empty-state
            icon="🏃"
            title="No sessions available"
            message="There are no upcoming sessions at the moment. Check back later." />
        } @else {
          <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
            @for (session of sessions(); track session.id) {
              <div class="card bg-base-100 shadow-sm">
                <div class="card-body">
                  <h3 class="card-title text-base">{{ session.title }}</h3>
                  <div class="space-y-1 text-sm text-base-content/70">
                    <p>📅 {{ session.date }}</p>
                    <p>🕐 {{ session.time }}</p>
                    <p>📍 {{ session.venue }}</p>
                    <p>👥 {{ session.bookedCount }}/{{ session.capacity }}</p>
                    <p class="font-semibold text-base-content">
                      {{ session.fee > 0 ? session.currency + session.fee.toFixed(2) : 'Free' }}
                    </p>
                  </div>
                  <div class="card-actions justify-end mt-2">
                    @if (session.bookedCount < session.capacity) {
                      <button
                        class="btn btn-primary btn-sm"
                        (click)="bookSession(session.id)"
                        [disabled]="bookingInProgress()">
                        Book
                      </button>
                    } @else {
                      <button
                        class="btn btn-warning btn-sm"
                        (click)="joinWaitlist(session.id)"
                        [disabled]="bookingInProgress()">
                        Join Waitlist
                      </button>
                    }
                  </div>
                </div>
              </div>
            }
          </div>
        }
      }

      <!-- My Bookings Tab -->
      @if (activeTab() === 'my-bookings') {
        @if (loadingBookings()) {
          <app-skeleton-loader type="table-row" [count]="5" />
        } @else if (myBookings().length === 0) {
          <app-empty-state
            icon="📋"
            title="No bookings yet"
            message="You haven't booked any sessions. Browse available sessions to get started."
            actionLabel="Browse Sessions"
            (actionClick)="activeTab.set('browse')" />
        } @else {
          <div class="overflow-x-auto">
            <table class="table">
              <thead>
                <tr>
                  <th>Session</th>
                  <th>Date</th>
                  <th>Time</th>
                  <th>Venue</th>
                  <th>Status</th>
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody>
                @for (booking of myBookings(); track booking.id) {
                  <tr>
                    <td class="font-medium">{{ booking.sessionTitle }}</td>
                    <td>{{ booking.date }}</td>
                    <td>{{ booking.time }}</td>
                    <td>{{ booking.venue }}</td>
                    <td>
                      <span class="badge" [class]="getStatusClass(booking.status)">
                        {{ booking.status }}
                      </span>
                    </td>
                    <td>
                      @if (booking.canCancel) {
                        <button
                          class="btn btn-error btn-xs"
                          (click)="cancelBooking(booking.id)">
                          Cancel
                        </button>
                      }
                    </td>
                  </tr>
                }
              </tbody>
            </table>
          </div>
        }
      }
    </div>
  `
})
export class PortalSessionsComponent implements OnInit {
  private api = inject(ApiService);

  activeTab = signal<'browse' | 'my-bookings'>('browse');
  loadingSessions = signal(true);
  loadingBookings = signal(true);
  bookingInProgress = signal(false);
  sessions = signal<AvailableSession[]>([]);
  myBookings = signal<MyBooking[]>([]);

  ngOnInit(): void {
    this.loadSessions();
    this.loadMyBookings();
  }

  bookSession(sessionId: string): void {
    this.bookingInProgress.set(true);
    this.api.post<any>('portal/sessions/book', { sessionId }).subscribe({
      next: () => {
        this.bookingInProgress.set(false);
        this.loadSessions();
        this.loadMyBookings();
      },
      error: () => {
        this.bookingInProgress.set(false);
      }
    });
  }

  joinWaitlist(sessionId: string): void {
    this.bookingInProgress.set(true);
    this.api.post<any>('portal/sessions/waitlist', { sessionId }).subscribe({
      next: () => {
        this.bookingInProgress.set(false);
      },
      error: () => {
        this.bookingInProgress.set(false);
      }
    });
  }

  cancelBooking(bookingId: string): void {
    this.api.post<any>('portal/sessions/cancel', { bookingId }).subscribe({
      next: () => {
        this.loadMyBookings();
      }
    });
  }

  getStatusClass(status: string): string {
    const map: Record<string, string> = {
      confirmed: 'badge-success',
      waitlisted: 'badge-warning',
      cancelled: 'badge-error',
      attended: 'badge-info',
    };
    return map[status.toLowerCase()] ?? 'badge-neutral';
  }

  private loadSessions(): void {
    this.api.get<AvailableSession[]>('portal/sessions/available').subscribe({
      next: (data) => {
        this.sessions.set(data ?? []);
        this.loadingSessions.set(false);
      },
      error: () => {
        this.loadingSessions.set(false);
      }
    });
  }

  private loadMyBookings(): void {
    this.api.get<MyBooking[]>('portal/sessions/my-bookings').subscribe({
      next: (data) => {
        this.myBookings.set(data ?? []);
        this.loadingBookings.set(false);
      },
      error: () => {
        this.loadingBookings.set(false);
      }
    });
  }
}
