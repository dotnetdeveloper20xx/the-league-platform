import { Component } from '@angular/core';

@Component({
  selector: 'app-events-list',
  standalone: true,
  template: `
    <div class="space-y-6">
      <div class="flex items-center justify-between">
        <div>
          <h1 class="text-2xl font-bold">Events</h1>
          <p class="text-base-content/70">Manage club events and ticketing</p>
        </div>
        <button class="btn btn-primary">Create Event</button>
      </div>

      <div class="card bg-base-100 shadow">
        <div class="card-body">
          <div class="overflow-x-auto">
            <table class="table table-zebra">
              <thead>
                <tr>
                  <th>Event</th>
                  <th>Type</th>
                  <th>Date</th>
                  <th>Registrations</th>
                  <th>Status</th>
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody>
                @for (event of events; track event.id) {
                  <tr>
                    <td class="font-medium">{{ event.title }}</td>
                    <td><span class="badge badge-outline">{{ event.type }}</span></td>
                    <td>{{ event.date }}</td>
                    <td>{{ event.registrations }}</td>
                    <td>
                      <span class="badge" [class]="event.statusClass">{{ event.status }}</span>
                    </td>
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
export class EventsListComponent {
  events = [
    { id: 1, title: 'Annual Dinner', type: 'Social', date: '2025-02-15', registrations: 45, status: 'Published', statusClass: 'badge-success' },
    { id: 2, title: 'AGM 2025', type: 'AGM', date: '2025-03-01', registrations: 0, status: 'Draft', statusClass: 'badge-ghost' },
    { id: 3, title: 'Charity Match', type: 'Fundraiser', date: '2025-04-10', registrations: 22, status: 'Registration Open', statusClass: 'badge-info' },
  ];
}
