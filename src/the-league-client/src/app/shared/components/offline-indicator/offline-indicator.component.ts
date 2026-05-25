import { Component, inject } from '@angular/core';
import { OfflineService } from '../../../core/services/offline.service';

@Component({
  selector: 'app-offline-indicator',
  standalone: true,
  template: `
    @if (!offlineService.isOnline()) {
      <div class="fixed top-0 left-0 right-0 z-50 bg-warning text-warning-content text-center py-1 text-sm font-medium">
        ⚠️ You are offline. Some features may be limited.
      </div>
    }
  `
})
export class OfflineIndicatorComponent {
  offlineService = inject(OfflineService);
}
