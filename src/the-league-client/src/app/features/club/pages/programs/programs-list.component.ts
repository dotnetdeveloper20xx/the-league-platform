import { Component } from '@angular/core';

@Component({
  selector: 'app-programs-list',
  standalone: true,
  template: `
    <div class="space-y-6">
      <div class="flex items-center justify-between">
        <div>
          <h1 class="text-2xl font-bold">Programs</h1>
          <p class="text-base-content/70">Manage coaching programs and courses</p>
        </div>
        <button class="btn btn-primary">Create Program</button>
      </div>

      <div class="card bg-base-100 shadow">
        <div class="card-body">
          <div class="overflow-x-auto">
            <table class="table table-zebra">
              <thead>
                <tr>
                  <th>Program</th>
                  <th>Duration</th>
                  <th>Enrolled</th>
                  <th>Capacity</th>
                  <th>Status</th>
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody>
                @for (program of programs; track program.id) {
                  <tr>
                    <td class="font-medium">{{ program.name }}</td>
                    <td>{{ program.duration }}</td>
                    <td>{{ program.enrolled }}</td>
                    <td>{{ program.capacity }}</td>
                    <td>
                      <span class="badge" [class]="program.statusClass">{{ program.status }}</span>
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
export class ProgramsListComponent {
  programs = [
    { id: 1, name: 'Junior Development', duration: '12 weeks', enrolled: 18, capacity: 24, status: 'Active', statusClass: 'badge-success' },
    { id: 2, name: 'Fast Bowling Academy', duration: '8 weeks', enrolled: 8, capacity: 12, status: 'Active', statusClass: 'badge-success' },
    { id: 3, name: 'Women\'s Cricket', duration: '10 weeks', enrolled: 0, capacity: 20, status: 'Upcoming', statusClass: 'badge-info' },
  ];
}
