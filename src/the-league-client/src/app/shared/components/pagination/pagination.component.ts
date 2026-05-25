import { Component, ChangeDetectionStrategy, input, output, computed } from '@angular/core';

@Component({
  selector: 'app-pagination',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="join">
      <button
        class="join-item btn btn-sm"
        [disabled]="currentPage() <= 1"
        (click)="goToPage(currentPage() - 1)"
      >
        «
      </button>

      @for (page of visiblePages(); track page) {
        @if (page === -1) {
          <button class="join-item btn btn-sm btn-disabled">…</button>
        } @else {
          <button
            class="join-item btn btn-sm"
            [class.btn-active]="page === currentPage()"
            (click)="goToPage(page)"
          >
            {{ page }}
          </button>
        }
      }

      <button
        class="join-item btn btn-sm"
        [disabled]="currentPage() >= totalPages()"
        (click)="goToPage(currentPage() + 1)"
      >
        »
      </button>
    </div>
  `,
})
export class PaginationComponent {
  currentPage = input(1);
  totalPages = input(1);
  pageSize = input(20);

  pageChange = output<number>();

  visiblePages = computed(() => {
    const total = this.totalPages();
    const current = this.currentPage();

    if (total <= 7) {
      return Array.from({ length: total }, (_, i) => i + 1);
    }

    const pages: number[] = [1];

    if (current > 3) {
      pages.push(-1); // ellipsis
    }

    const start = Math.max(2, current - 1);
    const end = Math.min(total - 1, current + 1);

    for (let i = start; i <= end; i++) {
      pages.push(i);
    }

    if (current < total - 2) {
      pages.push(-1); // ellipsis
    }

    pages.push(total);
    return pages;
  });

  goToPage(page: number): void {
    if (page >= 1 && page <= this.totalPages()) {
      this.pageChange.emit(page);
    }
  }
}
