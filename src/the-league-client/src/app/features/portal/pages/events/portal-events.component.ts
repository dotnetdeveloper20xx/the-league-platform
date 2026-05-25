import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ApiService } from '../../../../core/services/api.service';
import { SkeletonLoaderComponent } from '../../../../shared/components/skeleton-loader/skeleton-loader.component';
import { EmptyStateComponent } from '../../../../shared/components/empty-state/empty-state.component';
import { StatusBadgeComponent } from '../../../../shared/components/status-badge/status-badge.component';

interface ClubEvent {
  id: string;
  title: string;
  type: string;
  date: string;
  venue: string;
  price: number | null;
  currency: string;
  isTicketed: boolean;
  isFull: boolean;
}

interface MyEvent {
  id: string;
  eventId: string;
  title: string;
  date: string;
  type: string;
  registrationStatus: string;
}

@Component({
  selector: 'app-portal-events',
  standalone: true,
  imports: [CommonModule, SkeletonLoaderComponent, EmptyStateComponent, StatusBadgeComponent],
  template: `
    <div class="space-y-6">
      <h1 class="text-2xl font-bold">Events</h1>

      <!-- Tabs -->
      <div role="tablist" class="tabs tabs-bordered">
        <button
          role="tab"
          class="tab"
          [class.tab-active]="activeTab() === 'browse'"
          (click)="activeTab.set('browse')">
          Upcoming Events
        </button>
        <button
          role="tab"
          class="tab"
          [class.tab-active]="activeTab() === 'my-events'"
          (click)="activeTab.set('my-events')">
          My Events
        </button>
      </div>

      <!-- Browse Events Tab -->
      @if (activeTab() === 'browse') {
        @if (loadingEvents()) {
          <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
            <app-skeleton-loader type="card" [count]="6" />
          </div>
        } @else if (events().length === 0) {
          <app-empty-state
            icon="🎉"
            title="No upcoming events"
            message="There are no upcoming events at the moment. Check back soon!" />
        } @else {
          <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
            @for (event of events(); track event.id) {
              <div class="card bg-base-100 shadow-sm">
                <div class="card-body">
                  <h3 class="card-title text-base">{{ event.title }}</h3>
                  <app-status-badge [status]="event.type" />
                  <div class="space-y-1 text-sm text-base-content/70 mt-2">
                    <p>📅 {{ event.date }}</p>
                    <p>📍 {{ event.venue }}</p>
                    <p class="font-semibold text-base-content">
                      {{ event.price !== null && event.price > 0 ? event.currency + event.price.toFixed(2) : 'Free' }}
                    </p>
                  </div>
                  <div class="card-actions justify-end mt-2">
                    @if (event.isFull) {
                      <button class="btn btn-disabled btn-sm" disabled>Full</button>
                    } @else if (event.isTicketed) {
                      <button
                        class="btn btn-primary btn-sm"
                        (click)="registerForEvent(event.id)"
                        [disabled]="actionInProgress()">
                        Register
                      </button>
                    } @else {
                      <button
                        class="btn btn-secondary btn-sm"
                        (click)="rsvpEvent(event.id)"
                        [disabled]="actionInProgress()">
                        RSVP
                      </button>
                    }
                  </div>
                </div>
              </div>
            }
          </div>
        }
      }

      <!-- My Events Tab -->
      @if (activeTab() === 'my-events') {
        @if (loadingMyEvents()) {
          <app-skeleton-loader type="table-row" [count]="5" />
        } @else if (myEvents().length === 0) {
          <app-empty-state
            icon="📋"
            title="No registered events"
            message="You haven't registered for any events yet. Browse upcoming events to find something interesting."
            actionLabel="Browse Events"
            (actionClick)="activeTab.set('browse')" />
        } @else {
          <div class="overflow-x-auto">
            <table class="table">
              <thead>
                <tr>
                  <th>Event</th>
                  <th>Date</th>
                  <th>Type</th>
                  <th>Status</th>
                </tr>
              </thead>
              <tbody>
                @for (event of myEvents(); track event.id) {
                  <tr>
                    <td class="font-medium">{{ event.title }}</td>
                    <td>{{ event.date }}</td>
                    <td>{{ event.type }}</td>
                    <td><app-status-badge [status]="event.registrationStatus" /></td>
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
export class PortalEventsComponent implements OnInit {
  private api = inject(ApiService);

  activeTab = signal<'browse' | 'my-events'>('browse');
  loadingEvents = signal(true);
  loadingMyEvents = signal(true);
  actionInProgress = signal(false);
  events = signal<ClubEvent[]>([]);
  myEvents = signal<MyEvent[]>([]);

  ngOnInit(): void {
    this.loadEvents();
    this.loadMyEvents();
  }

  registerForEvent(eventId: string): void {
    this.actionInProgress.set(true);
    this.api.post<any>('portal/events/register', { eventId }).subscribe({
      next: () => {
        this.actionInProgress.set(false);
        this.loadEvents();
        this.loadMyEvents();
      },
      error: () => {
        this.actionInProgress.set(false);
      }
    });
  }

  rsvpEvent(eventId: string): void {
    this.actionInProgress.set(true);
    this.api.post<any>('portal/events/rsvp', { eventId, response: 'Attending' }).subscribe({
      next: () => {
        this.actionInProgress.set(false);
        this.loadEvents();
        this.loadMyEvents();
      },
      error: () => {
        this.actionInProgress.set(false);
      }
    });
  }

  private loadEvents(): void {
    this.api.get<ClubEvent[]>('portal/events/upcoming').subscribe({
      next: (data) => {
        this.events.set(data ?? []);
        this.loadingEvents.set(false);
      },
      error: () => {
        this.loadingEvents.set(false);
      }
    });
  }

  private loadMyEvents(): void {
    this.api.get<MyEvent[]>('portal/events/my-events').subscribe({
      next: (data) => {
        this.myEvents.set(data ?? []);
        this.loadingMyEvents.set(false);
      },
      error: () => {
        this.loadingMyEvents.set(false);
      }
    });
  }
}
