import { Component } from '@angular/core';

@Component({
  selector: 'app-club-reports',
  standalone: true,
  template: `
    <div class="space-y-6">
      <div>
        <h1 class="text-2xl font-bold">Reports</h1>
        <p class="text-base-content/70">Club reporting and analytics</p>
      </div>

      <div class="card bg-base-100 shadow">
        <div class="card-body">
          <h2 class="card-title">Available Reports</h2>
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
export class ClubReportsComponent {
  reports = [
    { name: 'Membership Report', description: 'Active members, renewals, and churn' },
    { name: 'Financial Summary', description: 'Revenue, expenses, and outstanding balances' },
    { name: 'Attendance Report', description: 'Session attendance rates and trends' },
    { name: 'Event Report', description: 'Event participation and ticket sales' },
    { name: 'Equipment Report', description: 'Inventory status and loan history' },
    { name: 'Communication Report', description: 'Email and SMS delivery metrics' },
  ];
}
