import { Component } from '@angular/core';

@Component({
  selector: 'app-sessions-list',
  standalone: true,
  template: `
    <div class="space-y-6">
      <div class="flex items-center justify-between">
        <div>
          <h1 class="text-2xl font-bold">Sessions</h1>
          <p class="text-base-content/70">Schedule and manage training sessions</p>
        </div>
        <button class="btn btn-primary">Create Session</button>
      </div>

      <div class="card bg-base-100 shadow">
        <div class="card-body">
          <div class="overflow-x-auto">
            <table class="table table-zebra">
              <thead>
                <tr>
                  <th>Title</th>
                  <th>Date</th>
                  <th>Time</th>
                  <th>Venue</th>
                  <th>Booked / Capacity</th>
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody>
                @for (session of sessions; track session.id) {
                  <tr>
                    <td class="font-medium">{{ session.title }}</td>
                    <td>{{ session.date }}</td>
                    <td>{{ session.time }}</td>
                    <td>{{ session.venue }}</td>
                    <td>{{ session.booked }} / {{ session.capacity }}</td>
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
export class SessionsListComponent {
  sessions = [
    { id: 1, title: 'Net Practice', date: '2025-01-20', time: '18:00', venue: 'Indoor Nets', booked: 12, capacity: 16 },
    { id: 2, title: 'Fielding Drills', date: '2025-01-21', time: '17:30', venue: 'Main Ground', booked: 8, capacity: 20 },
    { id: 3, title: 'Junior Coaching', date: '2025-01-22', time: '16:00', venue: 'Practice Area', booked: 15, capacity: 15 },
  ];
}
