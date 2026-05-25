import {
  Component,
  ChangeDetectionStrategy,
  input,
  output,
  computed,
  TemplateRef,
  signal,
} from '@angular/core';
import { NgTemplateOutlet } from '@angular/common';
import { PaginationComponent } from '../pagination/pagination.component';

export interface ColumnDef<T = unknown> {
  key: string;
  label: string;
  sortable?: boolean;
  template?: TemplateRef<{ $implicit: T; row: T }>;
}

export interface SortEvent {
  column: string;
  direction: 'asc' | 'desc';
}

@Component({
  selector: 'app-data-table',
  standalone: true,
  imports: [NgTemplateOutlet, PaginationComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    @if (loading()) {
      <div class="overflow-x-auto">
        <table class="table">
          <thead>
            <tr>
              @for (col of columns(); track col.key) {
                <th>{{ col.label }}</th>
              }
            </tr>
          </thead>
          <tbody>
            @for (row of skeletonRows(); track $index) {
              <tr>
                @for (col of columns(); track col.key) {
                  <td><div class="skeleton h-4 w-full"></div></td>
                }
              </tr>
            }
          </tbody>
        </table>
      </div>
    } @else {
      <div class="overflow-x-auto">
        <table class="table">
          <thead>
            <tr>
              @for (col of columns(); track col.key) {
                <th
                  [class.cursor-pointer]="sortable() && col.sortable !== false"
                  (click)="onSort(col)"
                >
                  {{ col.label }}
                  @if (sortable() && col.sortable !== false && currentSort()?.column === col.key) {
                    <span class="ml-1">
                      {{ currentSort()?.direction === 'asc' ? '▲' : '▼' }}
                    </span>
                  }
                </th>
              }
            </tr>
          </thead>
          <tbody>
            @for (row of paginatedData(); track $index) {
              <tr
                class="hover cursor-pointer"
                (click)="rowClick.emit(row)"
              >
                @for (col of columns(); track col.key) {
                  <td>
                    @if (col.template) {
                      <ng-container
                        [ngTemplateOutlet]="col.template"
                        [ngTemplateOutletContext]="{ $implicit: row, row: row }"
                      />
                    } @else {
                      {{ getCellValue(row, col.key) }}
                    }
                  </td>
                }
              </tr>
            }
          </tbody>
        </table>
      </div>

      @if (totalPages() > 1) {
        <div class="flex justify-end mt-4">
          <app-pagination
            [currentPage]="currentPage()"
            [totalPages]="totalPages()"
            [pageSize]="pageSize()"
            (pageChange)="onPageChange($event)"
          />
        </div>
      }
    }
  `,
})
export class DataTableComponent<T = unknown> {
  data = input<T[]>([]);
  columns = input<ColumnDef<T>[]>([]);
  loading = input(false);
  sortable = input(true);
  pageSize = input(10);

  rowClick = output<T>();
  sortChange = output<SortEvent>();
  pageChange = output<number>();

  currentSort = signal<SortEvent | null>(null);
  currentPage = signal(1);

  skeletonRows = computed(() => Array.from({ length: this.pageSize() }));

  totalPages = computed(() => {
    const dataLength = this.data().length;
    return Math.max(1, Math.ceil(dataLength / this.pageSize()));
  });

  paginatedData = computed(() => {
    const start = (this.currentPage() - 1) * this.pageSize();
    const end = start + this.pageSize();
    return this.data().slice(start, end);
  });

  onSort(col: ColumnDef<T>): void {
    if (!this.sortable() || col.sortable === false) return;

    const current = this.currentSort();
    let direction: 'asc' | 'desc' = 'asc';

    if (current?.column === col.key) {
      direction = current.direction === 'asc' ? 'desc' : 'asc';
    }

    const sortEvent: SortEvent = { column: col.key, direction };
    this.currentSort.set(sortEvent);
    this.sortChange.emit(sortEvent);
  }

  onPageChange(page: number): void {
    this.currentPage.set(page);
    this.pageChange.emit(page);
  }

  getCellValue(row: T, key: string): string {
    const value = (row as Record<string, unknown>)[key];
    return value != null ? String(value) : '';
  }
}
