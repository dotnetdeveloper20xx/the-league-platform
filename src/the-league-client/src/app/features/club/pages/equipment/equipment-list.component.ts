import { Component } from '@angular/core';

@Component({
  selector: 'app-equipment-list',
  standalone: true,
  template: `
    <div class="space-y-6">
      <div class="flex items-center justify-between">
        <div>
          <h1 class="text-2xl font-bold">Equipment</h1>
          <p class="text-base-content/70">Track equipment inventory and loans</p>
        </div>
        <button class="btn btn-primary">Add Equipment</button>
      </div>

      <div class="card bg-base-100 shadow">
        <div class="card-body">
          <div class="overflow-x-auto">
            <table class="table table-zebra">
              <thead>
                <tr>
                  <th>Item</th>
                  <th>Category</th>
                  <th>Condition</th>
                  <th>Location</th>
                  <th>Status</th>
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody>
                @for (item of equipment; track item.id) {
                  <tr>
                    <td class="font-medium">{{ item.name }}</td>
                    <td>{{ item.category }}</td>
                    <td>
                      <span class="badge" [class]="item.conditionClass">{{ item.condition }}</span>
                    </td>
                    <td>{{ item.location }}</td>
                    <td>{{ item.status }}</td>
                    <td>
                      <button class="btn btn-ghost btn-xs">Manage</button>
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
export class EquipmentListComponent {
  equipment = [
    { id: 1, name: 'Bowling Machine', category: 'Training', condition: 'Good', conditionClass: 'badge-success', location: 'Indoor Nets', status: 'Available' },
    { id: 2, name: 'Cricket Bat (Senior)', category: 'Batting', condition: 'Fair', conditionClass: 'badge-warning', location: 'Equipment Store', status: 'On Loan' },
    { id: 3, name: 'Sight Screen', category: 'Ground', condition: 'Needs Repair', conditionClass: 'badge-error', location: 'Main Ground', status: 'Unavailable' },
  ];
}
