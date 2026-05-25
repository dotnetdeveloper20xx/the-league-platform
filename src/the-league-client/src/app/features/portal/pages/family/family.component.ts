import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../../../core/services/api.service';
import { SkeletonLoaderComponent } from '../../../../shared/components/skeleton-loader/skeleton-loader.component';
import { EmptyStateComponent } from '../../../../shared/components/empty-state/empty-state.component';
import { StatusBadgeComponent } from '../../../../shared/components/status-badge/status-badge.component';

interface FamilyMember {
  id: string;
  firstName: string;
  lastName: string;
  relationship: string;
  status: string;
  dateOfBirth?: string;
}

interface FamilyMemberForm {
  firstName: string;
  lastName: string;
  relationship: string;
  dateOfBirth: string;
  email: string;
}

@Component({
  selector: 'app-family',
  standalone: true,
  imports: [CommonModule, FormsModule, SkeletonLoaderComponent, EmptyStateComponent, StatusBadgeComponent],
  template: `
    <div class="space-y-6">
      <div class="flex items-center justify-between">
        <h1 class="text-2xl font-bold">Family Members</h1>
        <button
          class="btn btn-primary btn-sm"
          (click)="showAddForm.set(true)"
          [disabled]="familyMembers().length >= 10">
          Add Family Member
        </button>
      </div>

      <!-- Add/Edit Form Modal -->
      @if (showAddForm()) {
        <div class="card bg-base-100 shadow-sm">
          <div class="card-body">
            <h2 class="card-title">{{ editingMember() ? 'Edit' : 'Add' }} Family Member</h2>
            <form (ngSubmit)="saveFamily()" class="space-y-4">
              <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
                <div class="form-control">
                  <label class="label"><span class="label-text">First Name</span></label>
                  <input
                    type="text"
                    class="input input-bordered"
                    [(ngModel)]="form.firstName"
                    name="firstName"
                    required />
                </div>
                <div class="form-control">
                  <label class="label"><span class="label-text">Last Name</span></label>
                  <input
                    type="text"
                    class="input input-bordered"
                    [(ngModel)]="form.lastName"
                    name="lastName"
                    required />
                </div>
                <div class="form-control">
                  <label class="label"><span class="label-text">Relationship</span></label>
                  <select class="select select-bordered" [(ngModel)]="form.relationship" name="relationship" required>
                    <option value="">Select relationship</option>
                    <option value="Spouse">Spouse</option>
                    <option value="Child">Child</option>
                    <option value="Sibling">Sibling</option>
                    <option value="Parent">Parent</option>
                  </select>
                </div>
                <div class="form-control">
                  <label class="label"><span class="label-text">Date of Birth</span></label>
                  <input
                    type="date"
                    class="input input-bordered"
                    [(ngModel)]="form.dateOfBirth"
                    name="dateOfBirth" />
                </div>
                <div class="form-control md:col-span-2">
                  <label class="label"><span class="label-text">Email</span></label>
                  <input
                    type="email"
                    class="input input-bordered"
                    [(ngModel)]="form.email"
                    name="email" />
                </div>
              </div>
              <div class="flex gap-2 justify-end">
                <button type="button" class="btn btn-ghost" (click)="cancelForm()">Cancel</button>
                <button type="submit" class="btn btn-primary" [disabled]="saving()">
                  {{ saving() ? 'Saving...' : 'Save' }}
                </button>
              </div>
            </form>
          </div>
        </div>
      }

      <!-- Family Members List -->
      @if (loading()) {
        <app-skeleton-loader type="card" [count]="3" />
      } @else if (familyMembers().length === 0 && !showAddForm()) {
        <app-empty-state
          icon="👨‍👩‍👧‍👦"
          title="No family members"
          message="Add family members to manage their memberships and bookings together."
          actionLabel="Add Family Member"
          (actionClick)="showAddForm.set(true)" />
      } @else {
        <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
          @for (member of familyMembers(); track member.id) {
            <div class="card bg-base-100 shadow-sm">
              <div class="card-body">
                <h3 class="card-title text-base">{{ member.firstName }} {{ member.lastName }}</h3>
                <div class="space-y-1 text-sm text-base-content/70">
                  <p>Relationship: {{ member.relationship }}</p>
                  @if (member.dateOfBirth) {
                    <p>DOB: {{ member.dateOfBirth }}</p>
                  }
                </div>
                <div class="flex items-center gap-2 mt-2">
                  <app-status-badge [status]="member.status" />
                </div>
                <div class="card-actions justify-end mt-2">
                  <button class="btn btn-ghost btn-xs" (click)="editMember(member)">Edit</button>
                  <button class="btn btn-error btn-xs" (click)="removeMember(member.id)">Remove</button>
                </div>
              </div>
            </div>
          }
        </div>
      }
    </div>
  `
})
export class FamilyComponent implements OnInit {
  private api = inject(ApiService);

  loading = signal(true);
  saving = signal(false);
  showAddForm = signal(false);
  editingMember = signal<FamilyMember | null>(null);
  familyMembers = signal<FamilyMember[]>([]);

  form: FamilyMemberForm = this.getEmptyForm();

  ngOnInit(): void {
    this.loadFamilyMembers();
  }

  saveFamily(): void {
    this.saving.set(true);
    const editing = this.editingMember();

    if (editing) {
      this.api.put<any>(`portal/family/${editing.id}`, this.form).subscribe({
        next: () => {
          this.saving.set(false);
          this.cancelForm();
          this.loadFamilyMembers();
        },
        error: () => {
          this.saving.set(false);
        }
      });
    } else {
      this.api.post<any>('portal/family', this.form).subscribe({
        next: () => {
          this.saving.set(false);
          this.cancelForm();
          this.loadFamilyMembers();
        },
        error: () => {
          this.saving.set(false);
        }
      });
    }
  }

  editMember(member: FamilyMember): void {
    this.editingMember.set(member);
    this.form = {
      firstName: member.firstName,
      lastName: member.lastName,
      relationship: member.relationship,
      dateOfBirth: member.dateOfBirth ?? '',
      email: ''
    };
    this.showAddForm.set(true);
  }

  removeMember(memberId: string): void {
    this.api.delete<any>(`portal/family/${memberId}`).subscribe({
      next: () => {
        this.loadFamilyMembers();
      }
    });
  }

  cancelForm(): void {
    this.showAddForm.set(false);
    this.editingMember.set(null);
    this.form = this.getEmptyForm();
  }

  private loadFamilyMembers(): void {
    this.api.get<FamilyMember[]>('portal/family').subscribe({
      next: (data) => {
        this.familyMembers.set(data ?? []);
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
      }
    });
  }

  private getEmptyForm(): FamilyMemberForm {
    return {
      firstName: '',
      lastName: '',
      relationship: '',
      dateOfBirth: '',
      email: ''
    };
  }
}
