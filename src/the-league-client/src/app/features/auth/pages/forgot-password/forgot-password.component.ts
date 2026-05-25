import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { ApiService } from '../../../../core/services/api.service';

@Component({
  selector: 'app-forgot-password',
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
            <h2 class="card-title text-2xl">Forgot Password</h2>
            <p class="text-base-content/70 text-sm text-center">
              Enter your email and we'll send you a link to reset your password
            </p>
          </div>

          <!-- Success Message -->
          @if (submitted()) {
            <div class="alert alert-success mb-4">
              <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5 shrink-0" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
              </svg>
              <span>If an account with that email exists, we've sent a password reset link. Please check your inbox.</span>
            </div>
            <p class="text-center mt-4">
              <a routerLink="/auth/login" class="link link-primary">Back to Sign In</a>
            </p>
          } @else {
            <!-- Form -->
            <form [formGroup]="forgotForm" (ngSubmit)="onSubmit()" class="space-y-4">
              <!-- Email -->
              <div class="form-control w-full">
                <label class="label" for="email">
                  <span class="label-text">Email Address</span>
                </label>
                <input
                  id="email"
                  type="email"
                  formControlName="email"
                  placeholder="you@example.com"
                  class="input input-bordered w-full"
                  [class.input-error]="forgotForm.get('email')?.invalid && forgotForm.get('email')?.touched"
                />
                @if (forgotForm.get('email')?.hasError('required') && forgotForm.get('email')?.touched) {
                  <label class="label">
                    <span class="label-text-alt text-error">Email is required</span>
                  </label>
                }
                @if (forgotForm.get('email')?.hasError('email') && forgotForm.get('email')?.touched) {
                  <label class="label">
                    <span class="label-text-alt text-error">Please enter a valid email</span>
                  </label>
                }
              </div>

              <!-- Submit Button -->
              <button
                type="submit"
                class="btn btn-primary w-full"
                [disabled]="forgotForm.invalid || isLoading()"
              >
                @if (isLoading()) {
                  <span class="loading loading-spinner loading-sm"></span>
                }
                Send Reset Link
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
export class ForgotPasswordComponent {
  private readonly fb = inject(FormBuilder);
  private readonly apiService = inject(ApiService);

  isLoading = signal(false);
  submitted = signal(false);

  forgotForm = this.fb.group({
    email: ['', [Validators.required, Validators.email]]
  });

  onSubmit(): void {
    if (this.forgotForm.invalid) return;

    this.isLoading.set(true);

    const { email } = this.forgotForm.getRawValue();

    this.apiService.post('auth/forgot-password', { email }).subscribe({
      next: () => {
        this.isLoading.set(false);
        this.submitted.set(true);
      },
      error: () => {
        // Always show success message regardless of whether email exists
        // This prevents email enumeration attacks
        this.isLoading.set(false);
        this.submitted.set(true);
      }
    });
  }
}
