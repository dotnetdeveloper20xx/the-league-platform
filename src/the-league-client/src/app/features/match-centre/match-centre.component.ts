import { Component, inject, signal, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { SignalRService } from '../../core/services/signalr.service';
import { SkeletonLoaderComponent } from '../../shared/components/skeleton-loader/skeleton-loader.component';
import * as signalR from '@microsoft/signalr';

interface MatchData {
  id: string;
  homeTeam: string;
  awayTeam: string;
  homeScore: string;
  awayScore: string;
  status: string;
  venue: string;
  startTime: string;
  sportType: string;
}

interface MatchEvent {
  id: string;
  type: string;
  team: string;
  player: string;
  minute: number;
  description: string;
  timestamp: string;
}

@Component({
  selector: 'app-match-centre',
  standalone: true,
  imports: [CommonModule, SkeletonLoaderComponent],
  template: `
    <div class="min-h-screen bg-base-200 p-4">
      <div class="max-w-4xl mx-auto space-y-6">
        <!-- Connection Status -->
        @if (!connected()) {
          <div class="alert alert-warning">
            <span>Connecting to live feed...</span>
            <span class="loading loading-dots loading-sm"></span>
          </div>
        }

        <!-- Scoreboard -->
        <div class="card bg-base-100 shadow-lg">
          <div class="card-body text-center">
            @if (loading()) {
              <app-skeleton-loader type="card" [count]="1" />
            } @else if (match()) {
              <div class="badge badge-lg mb-2" [class]="getStatusBadgeClass()">
                {{ match()!.status }}
              </div>
              <div class="grid grid-cols-3 items-center gap-4">
                <div class="text-right">
                  <p class="text-xl font-bold">{{ match()!.homeTeam }}</p>
                </div>
                <div>
                  <p class="text-4xl font-bold">
                    {{ match()!.homeScore }} - {{ match()!.awayScore }}
                  </p>
                </div>
                <div class="text-left">
                  <p class="text-xl font-bold">{{ match()!.awayTeam }}</p>
                </div>
              </div>
              <div class="text-sm text-base-content/60 mt-2">
                <p>📍 {{ match()!.venue }}</p>
                <p>🕐 {{ match()!.startTime }}</p>
              </div>
            }
          </div>
        </div>

        <!-- Live Commentary -->
        <div class="card bg-base-100 shadow">
          <div class="card-body">
            <h2 class="card-title">Live Commentary</h2>
            @if (events().length === 0) {
              <p class="text-base-content/60 text-center py-8">
                {{ match()?.status === 'InProgress' ? 'Waiting for match events...' : 'Match has not started yet.' }}
              </p>
            } @else {
              <div class="space-y-3 max-h-96 overflow-y-auto">
                @for (event of events(); track event.id) {
                  <div class="flex items-start gap-3 p-3 rounded-lg bg-base-200">
                    <div class="badge badge-sm badge-primary">{{ event.minute }}'</div>
                    <div>
                      <p class="font-medium text-sm">
                        <span class="text-primary">{{ event.type }}</span>
                        — {{ event.team }}
                      </p>
                      <p class="text-sm text-base-content/70">{{ event.description }}</p>
                      <p class="text-xs text-base-content/50">{{ event.timestamp }}</p>
                    </div>
                  </div>
                }
              </div>
            }
          </div>
        </div>
      </div>
    </div>
  `
})
export class MatchCentreComponent implements OnInit, OnDestroy {
  private route = inject(ActivatedRoute);
  private signalRService = inject(SignalRService);

  loading = signal(true);
  connected = signal(false);
  match = signal<MatchData | null>(null);
  events = signal<MatchEvent[]>([]);

  private matchId = '';
  private connection: signalR.HubConnection | null = null;

  ngOnInit(): void {
    this.matchId = this.route.snapshot.params['id'] ?? '';
    this.loadMatchData();
    this.connectToLiveFeed();
  }

  ngOnDestroy(): void {
    if (this.matchId) {
      this.signalRService.disconnectMatch(this.matchId);
    }
  }

  getStatusBadgeClass(): string {
    const status = this.match()?.status ?? '';
    const map: Record<string, string> = {
      'InProgress': 'badge-error animate-pulse',
      'Completed': 'badge-success',
      'Scheduled': 'badge-info',
      'Confirmed': 'badge-warning'
    };
    return map[status] ?? 'badge-neutral';
  }

  private loadMatchData(): void {
    // Mock data for now - will be connected to API
    setTimeout(() => {
      this.match.set({
        id: this.matchId,
        homeTeam: 'Teddington CC',
        awayTeam: 'Richmond HC',
        homeScore: '0',
        awayScore: '0',
        status: 'Scheduled',
        venue: 'Teddington Cricket Ground',
        startTime: '14:00',
        sportType: 'Cricket'
      });
      this.loading.set(false);
    }, 500);
  }

  private async connectToLiveFeed(): Promise<void> {
    try {
      this.connection = await this.signalRService.connectMatch(this.matchId);
      this.connected.set(true);

      this.connection.on('MatchEvent', (eventType: string, payload: any) => {
        if (payload.score) {
          this.match.update(m => m ? { ...m, homeScore: payload.score.home, awayScore: payload.score.away, status: 'InProgress' } : m);
        }
        if (payload.event) {
          this.events.update(events => [payload.event, ...events].slice(0, 100));
        }
      });
    } catch {
      this.connected.set(false);
    }
  }
}
