import { Component } from '@angular/core';

@Component({
  selector: 'app-memberships',
  standalone: true,
  template: `
    <div class="space-y-6">
      <div class="flex items-center justify-between">
        <div>
          <h1 class="text-2xl font-bold">Memberships</h1>
          <p class="text-base-content/70">Configure and manage membership types</p>
        </div>
        <button class="btn btn-primary">Create Membership Type</button>
      </div>

      <div class="card bg-base-100 shadow">
        <div class="card-body">
          <div class="overflow-x-auto">
            <table class="table table-zebra">
              <thead>
                <tr>
                  <th>Name</th>
                  <th>Price</th>
                  <th>Billing Cycle</th>
                  <th>Active Members</th>
                  <th>Capacity</th>
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody>
                @for (membership of memberships; track membership.id) {
                  <tr>
                    <td class="font-medium">{{ membership.name }}</td>
                    <td>£{{ membership.price }}</td>
                    <td>{{ membership.cycle }}</td>
                    <td>{{ membership.active }}</td>
                    <td>{{ membership.capacity }}</td>
                    <td>
                      <button class="btn btn-ghost btn-xs">Edit</button>
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
export class MembershipsComponent {
  memberships = [
    { id: 1, name: 'Adult Full', price: '120', cycle: 'Annual', active: 68, capacity: 150 },
    { id: 2, name: 'Junior', price: '60', cycle: 'Annual', active: 34, capacity: 80 },
    { id: 3, name: 'Social', price: '25', cycle: 'Annual', active: 22, capacity: 100 },
    { id: 4, name: 'Pay As You Go', price: '10', cycle: 'Monthly', active: 15, capacity: 50 },
  ];
}
