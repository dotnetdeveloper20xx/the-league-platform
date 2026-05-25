import { Component } from '@angular/core';

@Component({
  selector: 'app-shop-management',
  standalone: true,
  template: `
    <div class="space-y-6">
      <div class="flex items-center justify-between">
        <div>
          <h1 class="text-2xl font-bold">Shop</h1>
          <p class="text-base-content/70">Manage merchandise and orders</p>
        </div>
        <button class="btn btn-primary">Add Product</button>
      </div>

      <div class="card bg-base-100 shadow">
        <div class="card-body">
          <h2 class="card-title">Products</h2>
          <p class="text-base-content/70 mt-2">Shop management coming soon</p>
          <div class="mt-4 grid grid-cols-1 md:grid-cols-3 gap-4">
            <div class="stat bg-base-200 rounded-box">
              <div class="stat-title">Products</div>
              <div class="stat-value text-lg">12</div>
              <div class="stat-desc">Active listings</div>
            </div>
            <div class="stat bg-base-200 rounded-box">
              <div class="stat-title">Orders</div>
              <div class="stat-value text-lg">28</div>
              <div class="stat-desc">This month</div>
            </div>
            <div class="stat bg-base-200 rounded-box">
              <div class="stat-title">Revenue</div>
              <div class="stat-value text-lg">£840</div>
              <div class="stat-desc">This month</div>
            </div>
          </div>
        </div>
      </div>
    </div>
  `
})
export class ShopManagementComponent {}
