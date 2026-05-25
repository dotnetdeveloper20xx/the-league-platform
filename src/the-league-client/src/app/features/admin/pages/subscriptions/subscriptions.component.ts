import { Component } from '@angular/core';

@Component({
  selector: 'app-subscriptions',
  standalone: true,
  template: `
    <div class="space-y-6">
      <div>
        <h1 class="text-2xl font-bold">Subscriptions</h1>
        <p class="text-base-content/70">Platform subscription overview and management</p>
      </div>

      <!-- Tier Summary -->
      <div class="grid grid-cols-1 md:grid-cols-4 gap-4">
        @for (tier of tiers; track tier.name) {
          <div class="stat bg-base-100 shadow rounded-box">
            <div class="stat-title">{{ tier.name }}</div>
            <div class="stat-value text-lg">{{ tier.count }}</div>
            <div class="stat-desc">{{ tier.price }}/month</div>
          </div>
        }
      </div>

      <div class="card bg-base-100 shadow">
        <div class="card-body">
          <h2 class="card-title">Subscription Management</h2>
          <p class="text-base-content/70">Detailed subscription management coming soon</p>
        </div>
      </div>
    </div>
  `
})
export class SubscriptionsComponent {
  tiers = [
    { name: 'Free', count: 9, price: '£0' },
    { name: 'Starter', count: 15, price: '£29' },
    { name: 'Pro', count: 18, price: '£79' },
    { name: 'Enterprise', count: 5, price: '£199' },
  ];
}
