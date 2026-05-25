import { Component } from '@angular/core';

@Component({
  selector: 'app-club-settings',
  standalone: true,
  template: `
    <div class="space-y-6">
      <div>
        <h1 class="text-2xl font-bold">Club Settings</h1>
        <p class="text-base-content/70">Configure your club preferences</p>
      </div>

      <div class="card bg-base-100 shadow">
        <div class="card-body space-y-4">
          <h2 class="card-title">General Settings</h2>

          <div class="form-control w-full max-w-md">
            <label class="label"><span class="label-text">Club Name</span></label>
            <input type="text" value="Riverside Cricket Club" class="input input-bordered w-full" disabled />
          </div>

          <div class="form-control w-full max-w-md">
            <label class="label"><span class="label-text">Contact Email</span></label>
            <input type="email" value="info@riverside.cc" class="input input-bordered w-full" disabled />
          </div>

          <div class="form-control w-full max-w-md">
            <label class="label"><span class="label-text">Sport Type</span></label>
            <input type="text" value="Cricket" class="input input-bordered w-full" disabled />
          </div>

          <div class="divider"></div>
          <p class="text-base-content/70">Full settings management coming soon</p>
        </div>
      </div>
    </div>
  `
})
export class ClubSettingsComponent {}
