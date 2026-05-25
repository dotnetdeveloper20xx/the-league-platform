import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../../core/services/auth.service';
import { ApiService } from '../../../../core/services/api.service';
import { AuthResponse, UserRole } from '../../../../core/models/auth.model';

@Component({
  selector: 'app-login',
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
            <h2 class="card-title text-2xl">Sign In</h2>
            <p class="text-base-content/70 text-sm">Welcome back to The League</p>
          </div>

          <!-- Lockout Alert -->
          @if (lockoutMessage()) {
            <div class="alert alert-warning mb-4">
              <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5 shrink-0" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-2.5L13.732 4c-.77-.833-1.964-.833-2.732 0L4.082 16.5c-.77.833.192 2.5 1.732 2.5z" />
              </svg>
              <span>{{ lockoutMessage() }}</span>
            </div>
          }

          <!-- Email Verification Alert -->
          @if (verificationRequired()) {
            <div class="alert alert-info mb-4">
              <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5 shrink-0" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
              </svg>
              <span>Please verify your email address before signing in. Check your inbox for the verification link.</span>
            </div>
          }

          <!-- Error Alert -->
          @if (errorMessage()) {
            <div class="alert alert-error mb-4">
              <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5 shrink-0" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M10 14l2-2m0 0l2-2m-2 2l-2-2m2 2l2 2m7-2a9 9 0 11-18 0 9 9 0 0118 0z" />
              </svg>
              <span>{{ errorMessage() }}</span>
            </div>
          }

          <!-- Login Form -->
          <form [formGroup]="loginForm" (ngSubmit)="onSubmit()" class="space-y-4">
            <!-- Email -->
            <div class="form-control w-full">
              <label class="label" for="email">
                <span class="label-text">Email</span>
              </label>
              <input
                id="email"
                type="email"
                formControlName="email"
                placeholder="you@example.com"
                class="input input-bordered w-full"
                [class.input-error]="loginForm.get('email')?.invalid && loginForm.get('email')?.touched"
              />
              @if (loginForm.get('email')?.hasError('required') && loginForm.get('email')?.touched) {
                <label class="label">
                  <span class="label-text-alt text-error">Email is required</span>
                </label>
              }
              @if (loginForm.get('email')?.hasError('email') && loginForm.get('email')?.touched) {
                <label class="label">
                  <span class="label-text-alt text-error">Please enter a valid email</span>
                </label>
              }
            </div>

            <!-- Password -->
            <div class="form-control w-full">
              <label class="label" for="password">
                <span class="label-text">Password</span>
              </label>
              <input
                id="password"
                type="password"
                formControlName="password"
                placeholder="••••••••"
                class="input input-bordered w-full"
                [class.input-error]="loginForm.get('password')?.invalid && loginForm.get('password')?.touched"
              />
              @if (loginForm.get('password')?.hasError('required') && loginForm.get('password')?.touched) {
                <label class="label">
                  <span class="label-text-alt text-error">Password is required</span>
                </label>
              }
            </div>

            <!-- Remember Me + Forgot Password -->
            <div class="flex items-center justify-between">
              <label class="label cursor-pointer gap-2">
                <input type="checkbox" formControlName="rememberMe" class="checkbox checkbox-sm checkbox-primary" />
                <span class="label-text">Remember me</span>
              </label>
              <a routerLink="/auth/forgot-password" class="link link-primary text-sm">Forgot password?</a>
            </div>

            <!-- Submit Button -->
            <button
              type="submit"
              class="btn btn-primary w-full"
              [disabled]="loginForm.invalid || isLoading()"
            >
              @if (isLoading()) {
                <span class="loading loading-spinner loading-sm"></span>
              }
              Sign In
            </button>
          </form>

          <!-- Register Link -->
          <div class="divider text-sm">OR</div>
          <p class="text-center text-sm">
            Don't have an account?
            <a routerLink="/auth/register" class="link link-primary">Register</a>
          </p>
        </div>
      </div>
    </div>
  `
})
export class LoginComponent {
  private readonly fb = inject(FormBuilder);
  private readonly router = inject(Router);
  private readonly authService = inject(AuthService);
  private readonly apiService = inject(ApiService);

  isLoading = signal(false);
  errorMessage = signal('');
  lockoutMessage = signal('');
  verificationRequired = signal(false);

  loginForm = this.fb.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required]],
    rememberMe: [false]
  });

  onSubmit(): void {
    if (this.loginForm.invalid) return;

    this.isLoading.set(true);
    this.errorMessage.set('');
    this.lockoutMessage.set('');
    this.verificationRequired.set(false);

    const { email, password } = this.loginForm.getRawValue();

    this.apiService.post<AuthResponse>('auth/login', { email, password }).subscribe({
      next: (response) => {
        this.authService.setAuth(response);
        this.redirectByRole(response.user.role);
        this.isLoading.set(false);
      },
      error: (err) => {
        this.isLoading.set(false);
        this.handleLoginError(err);
      }
    });
  }

  private redirectByRole(role: UserRole): void {
    switch (role) {
      case 'SuperAdmin':
        this.router.navigate(['/admin']);
        break;
      case 'ClubManager':
        this.router.navigate(['/club']);
        break;
      default:
        this.router.navigate(['/portal']);
        break;
    }
  }

  private handleLoginError(err: any): void {
    const status = err?.status;
    const body = err?.error;

    if (status === 403) {
      if (body?.lockoutRemainingSeconds) {
        this.lockoutMessage.set(
          `Account locked. Please try again in ${body.lockoutRemainingSeconds} seconds.`
        );
      } else if (body?.emailVerificationRequired) {
        this.verificationRequired.set(true);
      } else {
        this.errorMessage.set(body?.message || 'Access denied.');
      }
    } else if (status === 401) {
      this.errorMessage.set('Invalid email or password.');
    } else {
      this.errorMessage.set('An unexpected error occurred. Please try again.');
    }
  }
}
