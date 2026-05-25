import { Component } from '@angular/core';

@Component({
  selector: 'app-facilities-list',
  standalone: true,
  template: `
    <div class="space-y-6">
      <div class="flex items-center justify-between">
        <div>
          <h1 class="text-2xl font-bold">Facilities</h1>
          <p class="text-base-content/70">Manage club facilities and bookings</p>
        </div>
        <button class="btn btn-primary">Add Facility</button>
      </div>

      <div class="card bg-base-100 shadow">
        <div class="card-body">
          <div class="overflow-x-auto">
            <table class="table table-zebra">
              <thead>
                <tr>
                  <th>Facility</th>
                  <th>Type</th>
                  <th>Status</th>
                  <th>Next Available</th>
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody>
                @for (facility of facilities; track facility.id) {
                  <tr>
                    <td class="font-medium">{{ facility.name }}</td>
                    <td><span class="badge badge-outline">{{ facility.type }}</span></td>
                    <td>
                      <span class="badge" [class]="facility.statusClass">{{ facility.status }}</span>
                    </td>
                    <td>{{ facility.nextAvailable }}</td>
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
export class FacilitiesListComponent {
  facilities = [
    { id: 1, name: 'Main Ground', type: 'Field', status: 'Available', statusClass: 'badge-success', nextAvailable: 'Now' },
    { id: 2, name: 'Indoor Nets', type: 'Court', status: 'In Use', statusClass: 'badge-warning', nextAvailable: '18:00 today' },
    { id: 3, name: 'Clubhouse', type: 'ClubHouse', status: 'Maintenance', statusClass: 'badge-error', nextAvailable: '2025-01-22' },
  ];
}
