import { Component } from '@angular/core';

@Component({
  selector: 'app-payments-list',
  standalone: true,
  template: `
    <div class="space-y-6">
      <div class="flex items-center justify-between">
        <div>
          <h1 class="text-2xl font-bold">Payments</h1>
          <p class="text-base-content/70">Track and manage payment transactions</p>
        </div>
        <button class="btn btn-primary">Record Payment</button>
      </div>

      <div class="card bg-base-100 shadow">
        <div class="card-body">
          <div class="overflow-x-auto">
            <table class="table table-zebra">
              <thead>
                <tr>
                  <th>Date</th>
                  <th>Member</th>
                  <th>Amount</th>
                  <th>Method</th>
                  <th>Status</th>
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody>
                @for (payment of payments; track payment.id) {
                  <tr>
                    <td>{{ payment.date }}</td>
                    <td class="font-medium">{{ payment.member }}</td>
                    <td>£{{ payment.amount }}</td>
                    <td>{{ payment.method }}</td>
                    <td>
                      <span class="badge" [class]="payment.statusClass">{{ payment.status }}</span>
                    </td>
                    <td>
                      <button class="btn btn-ghost btn-xs">View</button>
                    </td>
                  </tr>
                }
              </tbody>
            </table>
          </div>
        </div>
      </div>
    </div>
  `
})
export class PaymentsListComponent {
  payments = [
    { id: 1, date: '2025-01-18', member: 'James Wilson', amount: '120.00', method: 'Stripe', status: 'Completed', statusClass: 'badge-success' },
    { id: 2, date: '2025-01-17', member: 'Emma Thompson', amount: '60.00', method: 'Direct Debit', status: 'Completed', statusClass: 'badge-success' },
    { id: 3, date: '2025-01-16', member: 'Oliver Brown', amount: '45.00', method: 'Cash', status: 'Pending', statusClass: 'badge-warning' },
    { id: 4, date: '2025-01-15', member: 'Sophie Davis', amount: '120.00', method: 'Stripe', status: 'Failed', statusClass: 'badge-error' },
  ];
}
