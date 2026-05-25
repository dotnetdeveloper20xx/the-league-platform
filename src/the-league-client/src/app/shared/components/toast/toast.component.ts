import { Component, ChangeDetectionStrategy, inject } from '@angular/core';
import { NotificationService } from '../../../core/services/notification.service';

@Component({
  selector: 'app-toast',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="toast toast-end toast-top z-50">
      @for (toast of notificationService.toasts(); track toast.id) {
        <div
          class="alert cursor-pointer shadow-lg"
          [class.alert-success]="toast.type === 'success'"
          [class.alert-error]="toast.type === 'error'"
          [class.alert-warning]="toast.type === 'warning'"
          [class.alert-info]="toast.type === 'info'"
          (click)="notificationService.dismiss(toast.id)"
        >
          <span>{{ toast.message }}</span>
        </div>
      }
    </div>
  `,
})
export class ToastComponent {
  protected readonly notificationService = inject(NotificationService);
}
