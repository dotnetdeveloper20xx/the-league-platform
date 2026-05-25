import { Component } from '@angular/core';

@Component({
  selector: 'app-users-list',
  standalone: true,
  template: `
    <div class="space-y-6">
      <div class="flex items-center justify-between">
        <div>
          <h1 class="text-2xl font-bold">Users Management</h1>
          <p class="text-base-content/70">Manage platform users and their roles</p>
        </div>
        <button class="btn btn-primary">Add User</button>
      </div>

      <div class="card bg-base-100 shadow">
        <div class="card-body">
          <div class="overflow-x-auto">
            <table class="table table-zebra">
              <thead>
                <tr>
                  <th>Name</th>
                  <th>Email</th>
                  <th>Role</th>
                  <th>Club</th>
                  <th>Status</th>
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody>
                @for (user of users; track user.id) {
                  <tr>
                    <td class="font-medium">{{ user.name }}</td>
                    <td>{{ user.email }}</td>
                    <td><span class="badge badge-outline">{{ user.role }}</span></td>
                    <td>{{ user.club }}</td>
                    <td>
                      <span class="badge" [class]="user.active ? 'badge-success' : 'badge-error'">
                        {{ user.active ? 'Active' : 'Locked' }}
                      </span>
                    </td>
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
export class UsersListComponent {
  users = [
    { id: 1, name: 'John Smith', email: 'john@riverside.cc', role: 'ClubManager', club: 'Riverside CC', active: true },
    { id: 2, name: 'Sarah Jones', email: 'sarah@oakfield.fc', role: 'ClubManager', club: 'Oakfield FC', active: true },
    { id: 3, name: 'Admin User', email: 'admin@theleague.io', role: 'SuperAdmin', club: 'Platform', active: true },
    { id: 4, name: 'Mike Brown', email: 'mike@valley.hc', role: 'Coach', club: 'Valley HC', active: false },
  ];
}
