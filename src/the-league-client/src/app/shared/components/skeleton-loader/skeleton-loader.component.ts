import { Component, ChangeDetectionStrategy, input, computed } from '@angular/core';

@Component({
  selector: 'app-skeleton-loader',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    @for (item of items(); track $index) {
      @switch (type()) {
        @case ('text') {
          <div class="skeleton h-4 w-full mb-2"></div>
        }
        @case ('card') {
          <div class="skeleton h-32 w-full rounded-lg mb-4"></div>
        }
        @case ('table-row') {
          <div class="flex gap-4 mb-2">
            <div class="skeleton h-4 w-1/4"></div>
            <div class="skeleton h-4 w-1/3"></div>
            <div class="skeleton h-4 w-1/4"></div>
            <div class="skeleton h-4 w-1/6"></div>
          </div>
        }
        @case ('avatar') {
          <div class="skeleton h-12 w-12 rounded-full mb-2"></div>
        }
      }
    }
  `,
})
export class SkeletonLoaderComponent {
  type = input<'text' | 'card' | 'table-row' | 'avatar'>('text');
  count = input(3);

  items = computed(() => Array.from({ length: this.count() }));
}
