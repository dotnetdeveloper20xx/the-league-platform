import { Component } from '@angular/core';

@Component({
  selector: 'app-clubs-list',
  standalone: true,
  template: `
    <div class="space-y-6">
      <div class="flex items-center justify-between">
        <div>
          <h1 class="text-2xl font-bold">Clubs Management</h1>
          <p class="text-base-content/70">Manage all registered clubs on the platform</p>
        </div>
        <button class="btn btn-primary">Add Club</button>
      </div>

      <div class="card bg-base-100 shadow">
        <div class="card-body">
          <div class="overflow-x-auto">
            <table class="table table-zebra">
              <thead>
                <tr>
                  <th>Club Name</th>
                  <th>Sport</th>
                  <th>Members</th>
                  <th>Subscription</th>
                  <th>Status</th>
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody>
                @for (club of clubs; track club.id) {
                  <tr>
                    <td class="font-medium">{{ club.name }}</td>
                    <td>{{ club.sport }}</td>
                    <td>{{ club.members }}</td>
                    <td><span class="badge badge-outline">{{ club.subscription }}</span></td>
                    <td>
                      <span class="badge" [class]="club.active ? 'badge-success' : 'badge-error'">
                        {{ club.active ? 'Active' : 'Inactive' }}
                      </span>
                    </td>
                    <td>
                      <button class="btn btn-ghost btn-xs">View</button>
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
export class ClubsListComponent {
  clubs = [
    { id: 1, name: 'Riverside Cricket Club', sport: 'Cricket', members: 124, subscription: 'Pro', active: true },
    { id: 2, name: 'Oakfield Football Club', sport: 'Football', members: 89, subscription: 'Starter', active: true },
    { id: 3, name: 'Valley Hockey Club', sport: 'Hockey', members: 56, subscription: 'Enterprise', active: true },
    { id: 4, name: 'Hilltop Tennis Club', sport: 'Tennis', members: 34, subscription: 'Free', active: false },
  ];
}
