import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../../../core/services/api.service';
import { SkeletonLoaderComponent } from '../../../../shared/components/skeleton-loader/skeleton-loader.component';

interface PersonalDetails {
  firstName: string;
  lastName: string;
  email: string;
  phone: string;
  dateOfBirth: string;
  gender: string;
}

interface AddressDetails {
  line1: string;
  line2: string;
  city: string;
  county: string;
  postcode: string;
  country: string;
}

interface EmergencyContact {
  name: string;
  phone: string;
  relation: string;
}

interface MedicalInfo {
  conditions: string;
  allergies: string;
  doctorName: string;
  doctorPhone: string;
  bloodType: string;
}

interface Preferences {
  emailNotifications: boolean;
  smsNotifications: boolean;
  marketingOptIn: boolean;
}

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [CommonModule, FormsModule, SkeletonLoaderComponent],
  template: `
    <div class="space-y-6">
      <h1 class="text-2xl font-bold">My Profile</h1>

      <!-- Profile Photo -->
      <div class="card bg-base-100 shadow-sm">
        <div class="card-body flex-row items-center gap-4">
          <div class="avatar placeholder">
            <div class="bg-neutral text-neutral-content rounded-full w-16 h-16">
              <span class="text-xl">{{ getInitials() }}</span>
            </div>
          </div>
          <div>
            <p class="font-semibold">Profile Photo</p>
            <p class="text-sm text-base-content/60">Upload a photo (coming soon)</p>
          </div>
        </div>
      </div>

      <!-- Tabs -->
      <div role="tablist" class="tabs tabs-bordered">
        <button role="tab" class="tab" [class.tab-active]="activeTab() === 'personal'" (click)="activeTab.set('personal')">Personal Details</button>
        <button role="tab" class="tab" [class.tab-active]="activeTab() === 'address'" (click)="activeTab.set('address')">Address</button>
        <button role="tab" class="tab" [class.tab-active]="activeTab() === 'emergency'" (click)="activeTab.set('emergency')">Emergency Contacts</button>
        <button role="tab" class="tab" [class.tab-active]="activeTab() === 'medical'" (click)="activeTab.set('medical')">Medical Info</button>
        <button role="tab" class="tab" [class.tab-active]="activeTab() === 'preferences'" (click)="activeTab.set('preferences')">Preferences</button>
      </div>

      @if (loading()) {
        <app-skeleton-loader type="card" [count]="1" />
      } @else {
        <!-- Personal Details Tab -->
        @if (activeTab() === 'personal') {
          <div class="card bg-base-100 shadow-sm">
            <div class="card-body">
              <form (ngSubmit)="savePersonal()" class="space-y-4">
                <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
                  <div class="form-control">
                    <label class="label"><span class="label-text">First Name</span></label>
                    <input type="text" class="input input-bordered" [(ngModel)]="personal.firstName" name="firstName" required />
                  </div>
                  <div class="form-control">
                    <label class="label"><span class="label-text">Last Name</span></label>
                    <input type="text" class="input input-bordered" [(ngModel)]="personal.lastName" name="lastName" required />
                  </div>
                  <div class="form-control">
                    <label class="label"><span class="label-text">Email</span></label>
                    <input type="email" class="input input-bordered" [(ngModel)]="personal.email" name="email" required />
                  </div>
                  <div class="form-control">
                    <label class="label"><span class="label-text">Phone</span></label>
                    <input type="tel" class="input input-bordered" [(ngModel)]="personal.phone" name="phone" />
                  </div>
                  <div class="form-control">
                    <label class="label"><span class="label-text">Date of Birth</span></label>
                    <input type="date" class="input input-bordered" [(ngModel)]="personal.dateOfBirth" name="dateOfBirth" />
                  </div>
                  <div class="form-control">
                    <label class="label"><span class="label-text">Gender</span></label>
                    <select class="select select-bordered" [(ngModel)]="personal.gender" name="gender">
                      <option value="">Prefer not to say</option>
                      <option value="Male">Male</option>
                      <option value="Female">Female</option>
                      <option value="Other">Other</option>
                    </select>
                  </div>
                </div>
                <div class="flex justify-end">
                  <button type="submit" class="btn btn-primary" [disabled]="saving()">
                    {{ saving() ? 'Saving...' : 'Save Personal Details' }}
                  </button>
                </div>
              </form>
            </div>
          </div>
        }

        <!-- Address Tab -->
        @if (activeTab() === 'address') {
          <div class="card bg-base-100 shadow-sm">
            <div class="card-body">
              <form (ngSubmit)="saveAddress()" class="space-y-4">
                <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
                  <div class="form-control md:col-span-2">
                    <label class="label"><span class="label-text">Address Line 1</span></label>
                    <input type="text" class="input input-bordered" [(ngModel)]="address.line1" name="line1" />
                  </div>
                  <div class="form-control md:col-span-2">
                    <label class="label"><span class="label-text">Address Line 2</span></label>
                    <input type="text" class="input input-bordered" [(ngModel)]="address.line2" name="line2" />
                  </div>
                  <div class="form-control">
                    <label class="label"><span class="label-text">City</span></label>
                    <input type="text" class="input input-bordered" [(ngModel)]="address.city" name="city" />
                  </div>
                  <div class="form-control">
                    <label class="label"><span class="label-text">County</span></label>
                    <input type="text" class="input input-bordered" [(ngModel)]="address.county" name="county" />
                  </div>
                  <div class="form-control">
                    <label class="label"><span class="label-text">Postcode</span></label>
                    <input type="text" class="input input-bordered" [(ngModel)]="address.postcode" name="postcode" />
                  </div>
                  <div class="form-control">
                    <label class="label"><span class="label-text">Country</span></label>
                    <input type="text" class="input input-bordered" [(ngModel)]="address.country" name="country" />
                  </div>
                </div>
                <div class="flex justify-end">
                  <button type="submit" class="btn btn-primary" [disabled]="saving()">
                    {{ saving() ? 'Saving...' : 'Save Address' }}
                  </button>
                </div>
              </form>
            </div>
          </div>
        }

        <!-- Emergency Contacts Tab -->
        @if (activeTab() === 'emergency') {
          <div class="card bg-base-100 shadow-sm">
            <div class="card-body">
              <form (ngSubmit)="saveEmergency()" class="space-y-4">
                <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
                  <div class="form-control">
                    <label class="label"><span class="label-text">Contact Name</span></label>
                    <input type="text" class="input input-bordered" [(ngModel)]="emergency.name" name="emergencyName" />
                  </div>
                  <div class="form-control">
                    <label class="label"><span class="label-text">Phone</span></label>
                    <input type="tel" class="input input-bordered" [(ngModel)]="emergency.phone" name="emergencyPhone" />
                  </div>
                  <div class="form-control">
                    <label class="label"><span class="label-text">Relation</span></label>
                    <input type="text" class="input input-bordered" [(ngModel)]="emergency.relation" name="emergencyRelation" />
                  </div>
                </div>
                <div class="flex justify-end">
                  <button type="submit" class="btn btn-primary" [disabled]="saving()">
                    {{ saving() ? 'Saving...' : 'Save Emergency Contact' }}
                  </button>
                </div>
              </form>
            </div>
          </div>
        }

        <!-- Medical Info Tab -->
        @if (activeTab() === 'medical') {
          <div class="card bg-base-100 shadow-sm">
            <div class="card-body">
              <form (ngSubmit)="saveMedical()" class="space-y-4">
                <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
                  <div class="form-control md:col-span-2">
                    <label class="label"><span class="label-text">Medical Conditions</span></label>
                    <textarea class="textarea textarea-bordered" [(ngModel)]="medical.conditions" name="conditions" rows="3"></textarea>
                  </div>
                  <div class="form-control md:col-span-2">
                    <label class="label"><span class="label-text">Allergies</span></label>
                    <textarea class="textarea textarea-bordered" [(ngModel)]="medical.allergies" name="allergies" rows="2"></textarea>
                  </div>
                  <div class="form-control">
                    <label class="label"><span class="label-text">Doctor Name</span></label>
                    <input type="text" class="input input-bordered" [(ngModel)]="medical.doctorName" name="doctorName" />
                  </div>
                  <div class="form-control">
                    <label class="label"><span class="label-text">Doctor Phone</span></label>
                    <input type="tel" class="input input-bordered" [(ngModel)]="medical.doctorPhone" name="doctorPhone" />
                  </div>
                  <div class="form-control">
                    <label class="label"><span class="label-text">Blood Type</span></label>
                    <select class="select select-bordered" [(ngModel)]="medical.bloodType" name="bloodType">
                      <option value="">Unknown</option>
                      <option value="A+">A+</option>
                      <option value="A-">A-</option>
                      <option value="B+">B+</option>
                      <option value="B-">B-</option>
                      <option value="AB+">AB+</option>
                      <option value="AB-">AB-</option>
                      <option value="O+">O+</option>
                      <option value="O-">O-</option>
                    </select>
                  </div>
                </div>
                <div class="flex justify-end">
                  <button type="submit" class="btn btn-primary" [disabled]="saving()">
                    {{ saving() ? 'Saving...' : 'Save Medical Info' }}
                  </button>
                </div>
              </form>
            </div>
          </div>
        }

        <!-- Preferences Tab -->
        @if (activeTab() === 'preferences') {
          <div class="card bg-base-100 shadow-sm">
            <div class="card-body">
              <form (ngSubmit)="savePreferences()" class="space-y-4">
                <div class="form-control">
                  <label class="label cursor-pointer justify-start gap-4">
                    <input type="checkbox" class="toggle toggle-primary" [(ngModel)]="preferences.emailNotifications" name="emailNotifications" />
                    <span class="label-text">Email Notifications</span>
                  </label>
                </div>
                <div class="form-control">
                  <label class="label cursor-pointer justify-start gap-4">
                    <input type="checkbox" class="toggle toggle-primary" [(ngModel)]="preferences.smsNotifications" name="smsNotifications" />
                    <span class="label-text">SMS Notifications</span>
                  </label>
                </div>
                <div class="form-control">
                  <label class="label cursor-pointer justify-start gap-4">
                    <input type="checkbox" class="toggle toggle-primary" [(ngModel)]="preferences.marketingOptIn" name="marketingOptIn" />
                    <span class="label-text">Marketing Communications</span>
                  </label>
                </div>
                <div class="flex justify-end">
                  <button type="submit" class="btn btn-primary" [disabled]="saving()">
                    {{ saving() ? 'Saving...' : 'Save Preferences' }}
                  </button>
                </div>
              </form>
            </div>
          </div>
        }
      }
    </div>
  `
})
export class ProfileComponent implements OnInit {
  private api = inject(ApiService);

  loading = signal(true);
  saving = signal(false);
  activeTab = signal<'personal' | 'address' | 'emergency' | 'medical' | 'preferences'>('personal');

  personal: PersonalDetails = { firstName: '', lastName: '', email: '', phone: '', dateOfBirth: '', gender: '' };
  address: AddressDetails = { line1: '', line2: '', city: '', county: '', postcode: '', country: '' };
  emergency: EmergencyContact = { name: '', phone: '', relation: '' };
  medical: MedicalInfo = { conditions: '', allergies: '', doctorName: '', doctorPhone: '', bloodType: '' };
  preferences: Preferences = { emailNotifications: true, smsNotifications: false, marketingOptIn: false };

  ngOnInit(): void {
    this.loadProfile();
  }

  getInitials(): string {
    const first = this.personal.firstName?.charAt(0) ?? '';
    const last = this.personal.lastName?.charAt(0) ?? '';
    return (first + last).toUpperCase() || '?';
  }

  savePersonal(): void {
    this.saving.set(true);
    this.api.put<any>('portal/profile/personal', this.personal).subscribe({
      next: () => this.saving.set(false),
      error: () => this.saving.set(false)
    });
  }

  saveAddress(): void {
    this.saving.set(true);
    this.api.put<any>('portal/profile/address', this.address).subscribe({
      next: () => this.saving.set(false),
      error: () => this.saving.set(false)
    });
  }

  saveEmergency(): void {
    this.saving.set(true);
    this.api.put<any>('portal/profile/emergency-contact', this.emergency).subscribe({
      next: () => this.saving.set(false),
      error: () => this.saving.set(false)
    });
  }

  saveMedical(): void {
    this.saving.set(true);
    this.api.put<any>('portal/profile/medical', this.medical).subscribe({
      next: () => this.saving.set(false),
      error: () => this.saving.set(false)
    });
  }

  savePreferences(): void {
    this.saving.set(true);
    this.api.put<any>('portal/profile/preferences', this.preferences).subscribe({
      next: () => this.saving.set(false),
      error: () => this.saving.set(false)
    });
  }

  private loadProfile(): void {
    this.api.get<any>('portal/profile').subscribe({
      next: (data) => {
        if (data.personal) this.personal = data.personal;
        if (data.address) this.address = data.address;
        if (data.emergencyContact) this.emergency = data.emergencyContact;
        if (data.medical) this.medical = data.medical;
        if (data.preferences) this.preferences = data.preferences;
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
      }
    });
  }
}
