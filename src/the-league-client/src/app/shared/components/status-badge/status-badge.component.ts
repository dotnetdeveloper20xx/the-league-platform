import { Component, ChangeDetectionStrategy, input, computed } from '@angular/core';

const DEFAULT_COLOR_MAP: Record<string, string> = {
  active: 'badge-success',
  pending: 'badge-warning',
  expired: 'badge-error',
  cancelled: 'badge-neutral',
};

@Component({
  selector: 'app-status-badge',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <span class="badge" [class]="badgeClass()">
      {{ status() }}
    </span>
  `,
})
export class StatusBadgeComponent {
  status = input.required<string>();
  colorMap = input<Record<string, string>>({});

  badgeClass = computed(() => {
    const map = { ...DEFAULT_COLOR_MAP, ...this.colorMap() };
    return map[this.status().toLowerCase()] ?? 'badge-neutral';
  });
}
