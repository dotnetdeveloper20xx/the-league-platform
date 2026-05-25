import { Component, ChangeDetectionStrategy, input, output } from '@angular/core';

@Component({
  selector: 'app-empty-state',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="flex flex-col items-center justify-center py-12 text-center">
      @if (icon()) {
        <span class="text-4xl mb-4">{{ icon() }}</span>
      }
      <h3 class="text-lg font-semibold mb-2">{{ title() }}</h3>
      @if (message()) {
        <p class="text-base-content/60 mb-4 max-w-sm">{{ message() }}</p>
      }
      @if (actionLabel()) {
        <button class="btn btn-primary" (click)="actionClick.emit()">
          {{ actionLabel() }}
        </button>
      }
    </div>
  `,
})
export class EmptyStateComponent {
  icon = input('');
  title = input.required<string>();
  message = input('');
  actionLabel = input<string | undefined>(undefined);

  actionClick = output();
}
