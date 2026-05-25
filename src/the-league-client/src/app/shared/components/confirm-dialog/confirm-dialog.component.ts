import { Component, ChangeDetectionStrategy, input, output } from '@angular/core';

@Component({
  selector: 'app-confirm-dialog',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <dialog class="modal" [class.modal-open]="isOpen()">
      <div class="modal-box">
        <h3 class="font-bold text-lg mb-2">{{ title() }}</h3>
        <p class="py-4">{{ message() }}</p>
        <div class="modal-action">
          <button class="btn" (click)="cancelled.emit()">
            {{ cancelLabel() }}
          </button>
          <button
            class="btn"
            [class.btn-error]="variant() === 'danger'"
            [class.btn-warning]="variant() === 'warning'"
            [class.btn-info]="variant() === 'info'"
            (click)="confirmed.emit()"
          >
            {{ confirmLabel() }}
          </button>
        </div>
      </div>
      <form method="dialog" class="modal-backdrop">
        <button (click)="cancelled.emit()">close</button>
      </form>
    </dialog>
  `,
})
export class ConfirmDialogComponent {
  isOpen = input(false);
  title = input('Confirm');
  message = input('Are you sure?');
  confirmLabel = input('Confirm');
  cancelLabel = input('Cancel');
  variant = input<'danger' | 'warning' | 'info'>('danger');

  confirmed = output();
  cancelled = output();
}
