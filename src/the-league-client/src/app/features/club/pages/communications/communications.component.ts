import { Component } from '@angular/core';

@Component({
  selector: 'app-communications',
  standalone: true,
  template: `
    <div class="space-y-6">
      <div class="flex items-center justify-between">
        <div>
          <h1 class="text-2xl font-bold">Communications</h1>
          <p class="text-base-content/70">Email campaigns, templates, and messaging</p>
        </div>
        <button class="btn btn-primary">New Campaign</button>
      </div>

      <div class="card bg-base-100 shadow">
        <div class="card-body">
          <h2 class="card-title">Recent Campaigns</h2>
          <p class="text-base-content/70 mt-2">Communication management coming soon</p>
          <div class="mt-4 grid grid-cols-1 md:grid-cols-3 gap-4">
            <div class="stat bg-base-200 rounded-box">
              <div class="stat-title">Emails Sent</div>
              <div class="stat-value text-lg">342</div>
              <div class="stat-desc">This month</div>
            </div>
            <div class="stat bg-base-200 rounded-box">
              <div class="stat-title">SMS Sent</div>
              <div class="stat-value text-lg">89</div>
              <div class="stat-desc">This month</div>
            </div>
            <div class="stat bg-base-200 rounded-box">
              <div class="stat-title">Open Rate</div>
              <div class="stat-value text-lg">67%</div>
              <div class="stat-desc">Average</div>
            </div>
          </div>
        </div>
      </div>
    </div>
  `
})
export class CommunicationsComponent {}
