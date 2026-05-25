import { Component } from '@angular/core';

@Component({
  selector: 'app-competitions-list',
  standalone: true,
  template: `
    <div class="space-y-6">
      <div class="flex items-center justify-between">
        <div>
          <h1 class="text-2xl font-bold">Competitions</h1>
          <p class="text-base-content/70">Manage leagues, tournaments, and fixtures</p>
        </div>
        <button class="btn btn-primary">Create Competition</button>
      </div>

      <div class="card bg-base-100 shadow">
        <div class="card-body">
          <div class="overflow-x-auto">
            <table class="table table-zebra">
              <thead>
                <tr>
                  <th>Competition</th>
                  <th>Type</th>
                  <th>Season</th>
                  <th>Teams</th>
                  <th>Status</th>
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody>
                @for (comp of competitions; track comp.id) {
                  <tr>
                    <td class="font-medium">{{ comp.name }}</td>
                    <td><span class="badge badge-outline">{{ comp.type }}</span></td>
                    <td>{{ comp.season }}</td>
                    <td>{{ comp.teams }}</td>
                    <td>
                      <span class="badge" [class]="comp.statusClass">{{ comp.status }}</span>
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
export class CompetitionsListComponent {
  competitions = [
    { id: 1, name: 'Premier League 2025', type: 'League', season: '2025', teams: 8, status: 'In Progress', statusClass: 'badge-success' },
    { id: 2, name: 'T20 Cup', type: 'Knockout', season: '2025', teams: 16, status: 'Registration', statusClass: 'badge-info' },
    { id: 3, name: 'Friendly Series', type: 'Friendly', season: '2025', teams: 4, status: 'Scheduled', statusClass: 'badge-ghost' },
  ];
}
