import { Component, ChangeDetectionStrategy, input } from '@angular/core';

@Component({
  selector: 'app-loading-spinner',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <span
      class="loading loading-spinner"
      [class.loading-sm]="size() === 'sm'"
      [class.loading-md]="size() === 'md'"
      [class.loading-lg]="size() === 'lg'"
    ></span>
  `,
})
export class LoadingSpinnerComponent {
  size = input<'sm' | 'md' | 'lg'>('md');
}
