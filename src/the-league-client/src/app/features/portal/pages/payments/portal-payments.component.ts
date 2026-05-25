import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ApiService } from '../../../../core/services/api.service';
import { SkeletonLoaderComponent } from '../../../../shared/components/skeleton-loader/skeleton-loader.component';
import { EmptyStateComponent } from '../../../../shared/components/empty-state/empty-state.component';
import { StatusBadgeComponent } from '../../../../shared/components/status-badge/status-badge.component';

interface PaymentRecord {
  id: string;
  date: string;
  amount: number;
  currency: string;
  type: string;
  method: string;
  status: string;
}

interface OutstandingInvoice {
  id: string;
  invoiceNumber: string;
  dueDate: string;
  amount: number;
  currency: string;
  status: string;
}

interface BalanceSummary {
  outstanding: number;
  credit: number;
  currency: string;
}

@Component({
  selector: 'app-portal-payments',
  standalone: true,
  imports: [CommonModule, SkeletonLoaderComponent, EmptyStateComponent, StatusBadgeComponent],
  template: `
    <div class="space-y-6">
      <h1 class="text-2xl font-bold">Payments</h1>

      <!-- Balance Summary -->
      <div class="card bg-base-100 shadow-sm">
        <div class="card-body">
          <h2 class="card-title">Balance Summary</h2>
          @if (loading()) {
            <app-skeleton-loader type="text" [count]="2" />
          } @else {
            <div class="stats stats-vertical sm:stats-horizontal w-full">
              <div class="stat">
                <div class="stat-title">Outstanding</div>
                <div class="stat-value text-error">
                  {{ balance()?.currency ?? '£' }}{{ (balance()?.outstanding ?? 0).toFixed(2) }}
                </div>
              </div>
              <div class="stat">
                <div class="stat-title">Credit</div>
                <div class="stat-value text-success">
                  {{ balance()?.currency ?? '£' }}{{ (balance()?.credit ?? 0).toFixed(2) }}
                </div>
              </div>
            </div>
          }
        </div>
      </div>

      <!-- Outstanding Invoices -->
      <div class="card bg-base-100 shadow-sm">
        <div class="card-body">
          <h2 class="card-title">Outstanding Invoices</h2>
          @if (loading()) {
            <app-skeleton-loader type="table-row" [count]="3" />
          } @else if (invoices().length === 0) {
            <app-empty-state
              icon="✅"
              title="All paid up"
              message="You have no outstanding invoices." />
          } @else {
            <div class="overflow-x-auto">
              <table class="table">
                <thead>
                  <tr>
                    <th>Invoice #</th>
                    <th>Due Date</th>
                    <th>Amount</th>
                    <th>Status</th>
                    <th>Actions</th>
                  </tr>
                </thead>
                <tbody>
                  @for (invoice of invoices(); track invoice.id) {
                    <tr>
                      <td class="font-medium">{{ invoice.invoiceNumber }}</td>
                      <td>{{ invoice.dueDate }}</td>
                      <td>{{ invoice.currency }}{{ invoice.amount.toFixed(2) }}</td>
                      <td><app-status-badge [status]="invoice.status" /></td>
                      <td>
                        <button
                          class="btn btn-primary btn-xs"
                          (click)="payInvoice(invoice.id)"
                          [disabled]="paymentInProgress()">
                          Pay
                        </button>
                      </td>
                    </tr>
                  }
                </tbody>
              </table>
            </div>
          }
        </div>
      </div>

      <!-- Payment History -->
      <div class="card bg-base-100 shadow-sm">
        <div class="card-body">
          <h2 class="card-title">Payment History</h2>
          @if (loading()) {
            <app-skeleton-loader type="table-row" [count]="5" />
          } @else if (payments().length === 0) {
            <app-empty-state
              icon="💳"
              title="No payment history"
              message="Your payment history will appear here once you make a payment." />
          } @else {
            <div class="overflow-x-auto">
              <table class="table">
                <thead>
                  <tr>
                    <th>Date</th>
                    <th>Amount</th>
                    <th>Type</th>
                    <th>Method</th>
                    <th>Status</th>
                  </tr>
                </thead>
                <tbody>
                  @for (payment of payments(); track payment.id) {
                    <tr>
                      <td>{{ payment.date }}</td>
                      <td>{{ payment.currency }}{{ payment.amount.toFixed(2) }}</td>
                      <td>{{ payment.type }}</td>
                      <td>{{ payment.method }}</td>
                      <td><app-status-badge [status]="payment.status" /></td>
                    </tr>
                  }
                </tbody>
              </table>
            </div>
          }
        </div>
      </div>
    </div>
  `
})
export class PortalPaymentsComponent implements OnInit {
  private api = inject(ApiService);

  loading = signal(true);
  paymentInProgress = signal(false);
  payments = signal<PaymentRecord[]>([]);
  invoices = signal<OutstandingInvoice[]>([]);
  balance = signal<BalanceSummary | null>(null);

  ngOnInit(): void {
    this.loadPaymentData();
  }

  payInvoice(invoiceId: string): void {
    this.paymentInProgress.set(true);
    this.api.post<any>('portal/payments/pay', { invoiceId }).subscribe({
      next: () => {
        this.paymentInProgress.set(false);
        this.loadPaymentData();
      },
      error: () => {
        this.paymentInProgress.set(false);
      }
    });
  }

  private loadPaymentData(): void {
    this.api.get<any>('portal/payments').subscribe({
      next: (data) => {
        this.payments.set(data.history ?? []);
        this.invoices.set(data.outstandingInvoices ?? []);
        this.balance.set(data.balance ?? null);
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
      }
    });
  }
}
