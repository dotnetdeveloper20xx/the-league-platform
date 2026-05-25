import { Component } from '@angular/core';

@Component({
  selector: 'app-admin-settings',
  standalone: true,
  template: `
    <div class="space-y-6">
      <div>
        <h1 class="text-2xl font-bold">Platform Settings</h1>
        <p class="text-base-content/70">Configure global platform settings</p>
      </div>

      <div class="card bg-base-100 shadow">
        <div class="card-body space-y-4">
          <h2 class="card-title">General Settings</h2>

          <div class="form-control w-full max-w-md">
            <label class="label"><span class="label-text">Platform Name</span></label>
            <input type="text" value="The League" class="input input-bordered w-full" disabled />
          </div>

          <div class="form-control w-full max-w-md">
            <label class="label"><span class="label-text">Transaction Fee (%)</span></label>
            <input type="number" value="1.5" class="input input-bordered w-full" disabled />
            <label class="label"><span class="label-text-alt">Applied to all Stripe payments (1-2%)</span></label>
          </div>

          <div class="form-control w-full max-w-md">
            <label class="label"><span class="label-text">Support Email</span></label>
            <input type="email" value="support@theleague.io" class="input input-bordered w-full" disabled />
          </div>

          <div class="divider"></div>
          <p class="text-base-content/70">Full settings management coming soon</p>
        </div>
      </div>
    </div>
  `
})
export class SettingsComponent {}
