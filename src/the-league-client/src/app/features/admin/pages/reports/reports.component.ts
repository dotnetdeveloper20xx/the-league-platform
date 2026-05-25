import { Component } from '@angular/core';

@Component({
  selector: 'app-admin-reports',
  standalone: true,
  template: `
    <div class="space-y-6">
      <div>
        <h1 class="text-2xl font-bold">Reports</h1>
        <p class="text-base-content/70">Platform-wide reporting and analytics</p>
      </div>

      <div class="card bg-base-100 shadow">
        <div class="card-body">
          <h2 class="card-title">Report Generation</h2>
          <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4 mt-4">
            @for (report of reports; track report.name) {
              <div class="card bg-base-200">
                <div class="card-body">
                  <h3 class="font-semibold">{{ report.name }}</h3>
                  <p class="text-sm text-base-content/70">{{ report.description }}</p>
                  <div class="card-actions justify-end mt-2">
                    <button class="btn btn-sm btn-outline">Generate</button>
                  </div>
                </div>
              </div>
            }
          </div>
        </div>
      </div>
    </div>
  `
})
export class ReportsComponent {
  reports = [
    { name: 'Revenue Report', description: 'Monthly platform revenue breakdown by tier' },
    { name: 'Club Growth', description: 'New club registrations over time' },
    { name: 'Churn Analysis', description: 'Subscription cancellations and downgrades' },
    { name: 'Usage Metrics', description: 'Platform feature usage across clubs' },
    { name: 'Payment Failures', description: 'Failed payments and dunning outcomes' },
    { name: 'User Activity', description: 'Active users and engagement metrics' },
  ];
}
