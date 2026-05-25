import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../../../core/services/api.service';
import { ThemeService, ThemeMode } from '../../../../core/services/theme.service';
import { SkeletonLoaderComponent } from '../../../../shared/components/skeleton-loader/skeleton-loader.component';

interface NotificationPreferences {
  emailNotifications: boolean;
  smsNotifications: boolean;
  marketingOptIn: boolean;
}

interface ChangePasswordForm {
  currentPassword: string;
  newPassword: string;
  confirmPassword: string;
}

@Component({
  selector: 'app-portal-settings',
  standalone: true,
  imports: [CommonModule, FormsModule, SkeletonLoaderComponent],
  template: `
    <div class="space-y-6">
      <h1 class="text-2xl font-bold">Settings</h1>

      <!-- Notification Preferences -->
      <div class="card bg-base-100 shadow-sm">
        <div class="card-body">
          <h2 class="card-title">Notification Preferences</h2>
          @if (loading()) {
            <app-skeleton-loader type="text" [count]="3" />
          } @else {
            <div class="space-y-4">
              <div class="form-control">
                <label class="label cursor-pointer justify-start gap-4">
                  <input
                    type="checkbox"
                    class="toggle toggle-primary"
                    [(ngModel)]="notifications.emailNotifications"
                    (ngModelChange)="saveNotifications()" />
                  <div>
                    <span class="label-text font-medium">Email Notifications</span>
                    <p class="text-xs text-base-content/60">Receive booking confirmations, event reminders, and payment receipts via email</p>
                  </div>
                </label>
              </div>
              <div class="form-control">
                <label class="label cursor-pointer justify-start gap-4">
                  <input
                    type="checkbox"
                    class="toggle toggle-primary"
                    [(ngModel)]="notifications.smsNotifications"
                    (ngModelChange)="saveNotifications()" />
                  <div>
                    <span class="label-text font-medium">SMS Notifications</span>
                    <p class="text-xs text-base-content/60">Receive time-sensitive alerts via text message</p>
                  </div>
                </label>
              </div>
              <div class="form-control">
                <label class="label cursor-pointer justify-start gap-4">
                  <input
                    type="checkbox"
                    class="toggle toggle-primary"
                    [(ngModel)]="notifications.marketingOptIn"
                    (ngModelChange)="saveNotifications()" />
                  <div>
                    <span class="label-text font-medium">Marketing Communications</span>
                    <p class="text-xs text-base-content/60">Receive news, offers, and promotional content</p>
                  </div>
                </label>
              </div>
            </div>
          }
        </div>
      </div>

      <!-- Theme Selection -->
      <div class="card bg-base-100 shadow-sm">
        <div class="card-body">
          <h2 class="card-title">Theme</h2>
          <div class="flex gap-2 flex-wrap">
            <button
              class="btn btn-sm"
              [class.btn-primary]="currentTheme() === 'system'"
              (click)="setTheme('system')">
              🖥️ System
            </button>
            <button
              class="btn btn-sm"
              [class.btn-primary]="currentTheme() === 'light'"
              (click)="setTheme('light')">
              ☀️ Light
            </button>
            <button
              class="btn btn-sm"
              [class.btn-primary]="currentTheme() === 'dark'"
              (click)="setTheme('dark')">
              🌙 Dark
            </button>
          </div>
        </div>
      </div>

      <!-- Change Password -->
      <div class="card bg-base-100 shadow-sm">
        <div class="card-body">
          <h2 class="card-title">Change Password</h2>
          <form (ngSubmit)="changePassword()" class="space-y-4">
            <div class="form-control">
              <label class="label"><span class="label-text">Current Password</span></label>
              <input
                type="password"
                class="input input-bordered"
                [(ngModel)]="passwordForm.currentPassword"
                name="currentPassword"
                required />
            </div>
            <div class="form-control">
              <label class="label"><span class="label-text">New Password</span></label>
              <input
                type="password"
                class="input input-bordered"
                [(ngModel)]="passwordForm.newPassword"
                name="newPassword"
                required
                minlength="8" />
            </div>
            <div class="form-control">
              <label class="label"><span class="label-text">Confirm New Password</span></label>
              <input
                type="password"
                class="input input-bordered"
                [(ngModel)]="passwordForm.confirmPassword"
                name="confirmPassword"
                required />
              @if (passwordForm.newPassword && passwordForm.confirmPassword && passwordForm.newPassword !== passwordForm.confirmPassword) {
                <label class="label"><span class="label-text-alt text-error">Passwords do not match</span></label>
              }
            </div>
            @if (passwordError()) {
              <div class="alert alert-error">
                <span>{{ passwordError() }}</span>
              </div>
            }
            @if (passwordSuccess()) {
              <div class="alert alert-success">
                <span>Password changed successfully</span>
              </div>
            }
            <div class="flex justify-end">
              <button
                type="submit"
                class="btn btn-primary"
                [disabled]="changingPassword() || !passwordForm.currentPassword || !passwordForm.newPassword || passwordForm.newPassword !== passwordForm.confirmPassword">
                {{ changingPassword() ? 'Changing...' : 'Change Password' }}
              </button>
            </div>
          </form>
        </div>
      </div>
    </div>
  `
})
export class PortalSettingsComponent implements OnInit {
  private api = inject(ApiService);
  private themeService = inject(ThemeService);

  loading = signal(true);
  changingPassword = signal(false);
  passwordError = signal('');
  passwordSuccess = signal(false);
  currentTheme = this.themeService.theme;

  notifications: NotificationPreferences = {
    emailNotifications: true,
    smsNotifications: false,
    marketingOptIn: false
  };

  passwordForm: ChangePasswordForm = {
    currentPassword: '',
    newPassword: '',
    confirmPassword: ''
  };

  ngOnInit(): void {
    this.loadSettings();
  }

  setTheme(theme: ThemeMode): void {
    this.themeService.setTheme(theme);
  }

  saveNotifications(): void {
    this.api.put<any>('portal/settings/notifications', this.notifications).subscribe();
  }

  changePassword(): void {
    this.passwordError.set('');
    this.passwordSuccess.set(false);
    this.changingPassword.set(true);

    this.api.post<any>('portal/settings/change-password', {
      currentPassword: this.passwordForm.currentPassword,
      newPassword: this.passwordForm.newPassword
    }).subscribe({
      next: () => {
        this.changingPassword.set(false);
        this.passwordSuccess.set(true);
        this.passwordForm = { currentPassword: '', newPassword: '', confirmPassword: '' };
      },
      error: (err) => {
        this.changingPassword.set(false);
        this.passwordError.set(err?.error?.message ?? 'Failed to change password. Please try again.');
      }
    });
  }

  private loadSettings(): void {
    this.api.get<any>('portal/settings').subscribe({
      next: (data) => {
        if (data.notifications) this.notifications = data.notifications;
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
      }
    });
  }
}
