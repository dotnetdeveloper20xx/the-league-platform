import { Component } from '@angular/core';

@Component({
  selector: 'app-members-list',
  standalone: true,
  template: `
    <div class="space-y-6">
      <div class="flex items-center justify-between">
        <div>
          <h1 class="text-2xl font-bold">Members</h1>
          <p class="text-base-content/70">Manage club members and their profiles</p>
        </div>
        <button class="btn btn-primary">Add Member</button>
      </div>

      <div class="card bg-base-100 shadow">
        <div class="card-body">
          <div class="overflow-x-auto">
            <table class="table table-zebra">
              <thead>
                <tr>
                  <th>Member #</th>
                  <th>Name</th>
                  <th>Email</th>
                  <th>Status</th>
                  <th>Joined</th>
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody>
                @for (member of members; track member.id) {
                  <tr>
                    <td class="font-mono text-sm">{{ member.number }}</td>
                    <td class="font-medium">{{ member.name }}</td>
                    <td>{{ member.email }}</td>
                    <td>
                      <span class="badge" [class]="member.statusClass">{{ member.status }}</span>
                    </td>
                    <td>{{ member.joined }}</td>
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
export class MembersListComponent {
  members = [
    { id: 1, number: 'MBR-001', name: 'James Wilson', email: 'james@email.com', status: 'Active', statusClass: 'badge-success', joined: '2024-01-15' },
    { id: 2, number: 'MBR-002', name: 'Emma Thompson', email: 'emma@email.com', status: 'Active', statusClass: 'badge-success', joined: '2024-02-20' },
    { id: 3, number: 'MBR-003', name: 'Oliver Brown', email: 'oliver@email.com', status: 'Suspended', statusClass: 'badge-warning', joined: '2024-03-10' },
    { id: 4, number: 'MBR-004', name: 'Sophie Davis', email: 'sophie@email.com', status: 'Expired', statusClass: 'badge-error', joined: '2023-11-05' },
  ];
}
