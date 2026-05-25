import { Component, ChangeDetectionStrategy, input, output } from '@angular/core';

@Component({
  selector: 'app-modal',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <dialog
      class="modal"
      [class.modal-open]="isOpen()"
      (click)="onBackdropClick($event)"
    >
      <div
        class="modal-box"
        [class.max-w-sm]="size() === 'sm'"
        [class.max-w-lg]="size() === 'md'"
        [class.max-w-3xl]="size() === 'lg'"
      >
        @if (title()) {
          <h3 class="font-bold text-lg mb-4">{{ title() }}</h3>
        }
        <ng-content />
        <div class="modal-action">
          <ng-content select="[modal-actions]" />
        </div>
      </div>
      <form method="dialog" class="modal-backdrop">
        <button (click)="closed.emit()">close</button>
      </form>
    </dialog>
  `,
})
export class ModalComponent {
  isOpen = input(false);
  title = input('');
  size = input<'sm' | 'md' | 'lg'>('md');

  closed = output();

  onBackdropClick(event: MouseEvent): void {
    if ((event.target as HTMLElement).tagName === 'DIALOG') {
      this.closed.emit();
    }
  }
}
