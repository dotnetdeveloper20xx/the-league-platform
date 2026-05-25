import { Injectable, signal } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class OfflineService {
  private readonly _isOnline = signal(navigator.onLine);
  readonly isOnline = this._isOnline.asReadonly();

  constructor() {
    window.addEventListener('online', () => {
      this._isOnline.set(true);
      this.syncQueuedActions();
    });
    window.addEventListener('offline', () => this._isOnline.set(false));
  }

  async queueAction(action: { type: string; payload: any }): Promise<void> {
    const queue = this.getQueue();
    queue.push({ ...action, timestamp: Date.now() });
    localStorage.setItem('offline_queue', JSON.stringify(queue));
  }

  private getQueue(): any[] {
    const raw = localStorage.getItem('offline_queue');
    return raw ? JSON.parse(raw) : [];
  }

  private async syncQueuedActions(): Promise<void> {
    const queue = this.getQueue();
    if (queue.length === 0) return;
    // TODO: Process queued actions and sync with server
    localStorage.removeItem('offline_queue');
  }
}
