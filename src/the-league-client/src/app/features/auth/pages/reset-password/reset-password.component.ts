import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators, AbstractControl, ValidationErrors } from '@angular/forms';
import { Router, RouterLink, ActivatedRoute } from '@angular/router';
import { ApiService } from '../../../../core/services/api.service';

@Component({
  selector: 'app-reset-password',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  template: `
    <div class="min-h-screen flex items-center justify-center bg-base-200 p-4">
      <div class="card bg-base-100 shadow-xl w-full max-w-md">
        <div class="card-body">
          <!-- Logo / App Name -->
          <div class="flex flex-col items-center mb-4">
            <div class="w-16 h-16 bg-primary rounded-full flex items-center justify-center mb-2">
              <span class="text-primary-content text-2xl font-bold">TL</span>
            </div>
            <h2 class="card-title text-2xl">Reset Password</h2>
            <p class="text-base-content/70 text-sm">Enter your new password below</p>
          </div>

          <!-- Invalid Token -->
          @if (invalidToken()) {
            <div class="alert alert-error mb-4">
              <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5 shrink-0" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M10 14l2-2m0 0l2-2m-2 2l-2-2m2 2l2 2m7-2a9 9 0 11-18 0 9 9 0 0118 0z" />
              </svg>
              <span>Invalid or expired reset link. Please request a new password reset.</span>
            </div>
            <p class="text-center mt-4">
              <a routerLink="/auth/forgot-password" class="link link-primary">Request New Reset Link</a>
            </p>
          } @else if (resetSuccess()) {
            <!-- Success Message -->
            <div class="alert alert-success mb-4">
              <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5 shrink-0" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
              </svg>
              <span>Password reset successful! You can now sign in with your new password.</span>
            </div>
            <p class="text-center mt-4">
              <a routerLink="/auth/login" class="btn btn-primary w-full">Go to Sign In</a>
            </p>
          } @else {
            <!-- Error Alert -->
            @if (errorMessage()) {
              <div class="alert alert-error mb-4">
                <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5 shrink-0" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M10 14l2-2m0 0l2-2m-2 2l-2-2m2 2l2 2m7-2a9 9 0 11-18 0 9 9 0 0118 0z" />
                </svg>
                <span>{{ errorMessage() }}</span>
              </div>
            }

            <!-- Reset Form -->
            <form [formGroup]="resetForm" (ngSubmit)="onSubmit()" class="space-y-4">
              <!-- New Password -->
              <div class="form-control w-full">
                <label class="label" for="password">
                  <span class="label-text">New Password</span>
                </label>
                <input
                  id="password"
                  type="password"
                  formControlName="password"
                  placeholder="••••••••"
                  class="input input-bordered w-full"
                  [class.input-error]="resetForm.get('password')?.invalid && resetForm.get('password')?.touched"
                />
                @if (resetForm.get('password')?.hasError('required') && resetForm.get('password')?.touched) {
                  <label class="label">
                    <span class="label-text-alt text-error">Password is required</span>
                  </label>
                }
                @if (resetForm.get('password')?.hasError('minlength') && resetForm.get('password')?.touched) {
                  <label class="label">
                    <span class="label-text-alt text-error">Password must be at least 8 characters</span>
                  </label>
                }
              </div>

              <!-- Confirm Password -->
              <div class="form-control w-full">
                <label class="label" for="confirmPassword">
                  <span class="label-text">Confirm New Password</span>
                </label>
                <input
                  id="confirmPassword"
                  type="password"
                  formControlName="confirmPassword"
                  placeholder="••••••••"
                  class="input input-bordered w-full"
                  [class.input-error]="resetForm.get('confirmPassword')?.invalid && resetForm.get('confirmPassword')?.touched"
                />
                @if (resetForm.get('confirmPassword')?.hasError('required') && resetForm.get('confirmPassword')?.touched) {
                  <label class="label">
                    <span class="label-text-alt text-error">Please confirm your password</span>
                  </label>
                }
                @if (resetForm.hasError('passwordMismatch') && resetForm.get('confirmPassword')?.touched) {
                  <label class="label">
                    <span class="label-text-alt text-error">Passwords do not match</span>
                  </label>
                }
              </div>

              <!-- Submit Button -->
              <button
                type="submit"
                class="btn btn-primary w-full"
                [disabled]="resetForm.invalid || isLoading()"
              >
                @if (isLoading()) {
                  <span class="loading loading-spinner loading-sm"></span>
                }
                Reset Password
              </button>
            </form>

            <!-- Back to Login -->
            <div class="mt-4 text-center">
              <a routerLink="/auth/login" class="link link-primary text-sm">
                ← Back to Sign In
              </a>
            </div>
          }
        </div>
      </div>
    </div>
  `
})
export class ResetPasswordComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly apiService = inject(ApiService);

  isLoading = signal(false);
  errorMessage = signal('');
  resetSuccess = signal(false);
  invalidToken = signal(false);

  private token = '';

  resetForm = this.fb.group({
    password: ['', [Validators.required, Validators.minLength(8)]],
    confirmPassword: ['', [Validators.required]]
  }, { validators: [this.passwordMatchValidator] });

  ngOnInit(): void {
    this.route.queryParams.subscribe(params => {
      this.token = params['token'] || '';
      if (!this.token) {
        this.invalidToken.set(true);
      }
    });
  }

  onSubmit(): void {
    if (this.resetForm.invalid || !this.token) return;

    this.isLoading.set(true);
    this.errorMessage.set('');

    const { password } = this.resetForm.getRawValue();

    this.apiService.post('auth/reset-password', { token: this.token, password }).subscribe({
      next: () => {
        this.isLoading.set(false);
        this.resetSuccess.set(true);
      },
      error: (err) => {
        this.isLoading.set(false);
        if (err?.status === 400 && err?.error?.message?.includes('token')) {
          this.invalidToken.set(true);
        } else {
          this.errorMessage.set(err?.error?.message || 'Failed to reset password. Please try again.');
        }
      }
    });
  }

  private passwordMatchValidator(control: AbstractControl): ValidationErrors | null {
    const password = control.get('password')?.value;
    const confirmPassword = control.get('confirmPassword')?.value;
    if (password && confirmPassword && password !== confirmPassword) {
      return { passwordMismatch: true };
    }
    return null;
  }
}
