import { Component } from '@angular/core';

@Component({
  selector: 'app-invoices-list',
  standalone: true,
  template: `
    <div class="space-y-6">
      <div class="flex items-center justify-between">
        <div>
          <h1 class="text-2xl font-bold">Invoices</h1>
          <p class="text-base-content/70">Generate and manage member invoices</p>
        </div>
        <button class="btn btn-primary">Create Invoice</button>
      </div>

      <div class="card bg-base-100 shadow">
        <div class="card-body">
          <div class="overflow-x-auto">
            <table class="table table-zebra">
              <thead>
                <tr>
                  <th>Invoice #</th>
                  <th>Member</th>
                  <th>Amount</th>
                  <th>Due Date</th>
                  <th>Status</th>
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody>
                @for (invoice of invoices; track invoice.id) {
                  <tr>
                    <td class="font-mono text-sm">{{ invoice.number }}</td>
                    <td class="font-medium">{{ invoice.member }}</td>
                    <td>£{{ invoice.amount }}</td>
                    <td>{{ invoice.dueDate }}</td>
                    <td>
                      <span class="badge" [class]="invoice.statusClass">{{ invoice.status }}</span>
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
export class InvoicesListComponent {
  invoices = [
    { id: 1, number: 'INV-001', member: 'James Wilson', amount: '120.00', dueDate: '2025-02-01', status: 'Sent', statusClass: 'badge-info' },
    { id: 2, number: 'INV-002', member: 'Emma Thompson', amount: '60.00', dueDate: '2025-01-25', status: 'Paid', statusClass: 'badge-success' },
    { id: 3, number: 'INV-003', member: 'Oliver Brown', amount: '45.00', dueDate: '2025-01-10', status: 'Overdue', statusClass: 'badge-error' },
  ];
}
