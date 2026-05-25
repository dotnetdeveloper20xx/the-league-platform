import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators, AbstractControl, ValidationErrors } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { ApiService } from '../../../../core/services/api.service';

@Component({
  selector: 'app-register',
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
            <h2 class="card-title text-2xl">Create Account</h2>
            <p class="text-base-content/70 text-sm">Join The League today</p>
          </div>

          <!-- Success Message -->
          @if (registrationSuccess()) {
            <div class="alert alert-success mb-4">
              <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5 shrink-0" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
              </svg>
              <span>Registration successful! Check your email for a verification link.</span>
            </div>
            <p class="text-center mt-4">
              <a routerLink="/auth/login" class="link link-primary">Back to Sign In</a>
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

            <!-- Register Form -->
            <form [formGroup]="registerForm" (ngSubmit)="onSubmit()" class="space-y-4">
              <!-- Name Row -->
              <div class="grid grid-cols-2 gap-3">
                <div class="form-control">
                  <label class="label" for="firstName">
                    <span class="label-text">First Name</span>
                  </label>
                  <input
                    id="firstName"
                    type="text"
                    formControlName="firstName"
                    placeholder="John"
                    class="input input-bordered w-full"
                    [class.input-error]="registerForm.get('firstName')?.invalid && registerForm.get('firstName')?.touched"
                  />
                  @if (registerForm.get('firstName')?.hasError('required') && registerForm.get('firstName')?.touched) {
                    <label class="label">
                      <span class="label-text-alt text-error">Required</span>
                    </label>
                  }
                </div>
                <div class="form-control">
                  <label class="label" for="lastName">
                    <span class="label-text">Last Name</span>
                  </label>
                  <input
                    id="lastName"
                    type="text"
                    formControlName="lastName"
                    placeholder="Doe"
                    class="input input-bordered w-full"
                    [class.input-error]="registerForm.get('lastName')?.invalid && registerForm.get('lastName')?.touched"
                  />
                  @if (registerForm.get('lastName')?.hasError('required') && registerForm.get('lastName')?.touched) {
                    <label class="label">
                      <span class="label-text-alt text-error">Required</span>
                    </label>
                  }
                </div>
              </div>

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
                  [class.input-error]="registerForm.get('email')?.invalid && registerForm.get('email')?.touched"
                />
                @if (registerForm.get('email')?.hasError('required') && registerForm.get('email')?.touched) {
                  <label class="label">
                    <span class="label-text-alt text-error">Email is required</span>
                  </label>
                }
                @if (registerForm.get('email')?.hasError('email') && registerForm.get('email')?.touched) {
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
                  [class.input-error]="registerForm.get('password')?.invalid && registerForm.get('password')?.touched"
                />
                @if (registerForm.get('password')?.hasError('required') && registerForm.get('password')?.touched) {
                  <label class="label">
                    <span class="label-text-alt text-error">Password is required</span>
                  </label>
                }
                @if (registerForm.get('password')?.hasError('minlength') && registerForm.get('password')?.touched) {
                  <label class="label">
                    <span class="label-text-alt text-error">Password must be at least 8 characters</span>
                  </label>
                }
                <!-- Password Strength Indicator -->
                @if (registerForm.get('password')?.value) {
                  <div class="mt-2">
                    <progress
                      class="progress w-full"
                      [class.progress-error]="passwordStrength() <= 1"
                      [class.progress-warning]="passwordStrength() === 2"
                      [class.progress-info]="passwordStrength() === 3"
                      [class.progress-success]="passwordStrength() >= 4"
                      [value]="passwordStrength() * 25"
                      max="100"
                    ></progress>
                    <span class="text-xs text-base-content/60">
                      {{ passwordStrengthLabel() }}
                    </span>
                  </div>
                }
              </div>

              <!-- Confirm Password -->
              <div class="form-control w-full">
                <label class="label" for="confirmPassword">
                  <span class="label-text">Confirm Password</span>
                </label>
                <input
                  id="confirmPassword"
                  type="password"
                  formControlName="confirmPassword"
                  placeholder="••••••••"
                  class="input input-bordered w-full"
                  [class.input-error]="registerForm.get('confirmPassword')?.invalid && registerForm.get('confirmPassword')?.touched"
                />
                @if (registerForm.get('confirmPassword')?.hasError('required') && registerForm.get('confirmPassword')?.touched) {
                  <label class="label">
                    <span class="label-text-alt text-error">Please confirm your password</span>
                  </label>
                }
                @if (registerForm.hasError('passwordMismatch') && registerForm.get('confirmPassword')?.touched) {
                  <label class="label">
                    <span class="label-text-alt text-error">Passwords do not match</span>
                  </label>
                }
              </div>

              <!-- Terms Acceptance -->
              <div class="form-control">
                <label class="label cursor-pointer justify-start gap-3">
                  <input
                    type="checkbox"
                    formControlName="acceptTerms"
                    class="checkbox checkbox-sm checkbox-primary"
                  />
                  <span class="label-text text-sm">
                    I agree to the <a href="/terms" class="link link-primary">Terms of Service</a>
                    and <a href="/privacy" class="link link-primary">Privacy Policy</a>
                  </span>
                </label>
                @if (registerForm.get('acceptTerms')?.hasError('requiredTrue') && registerForm.get('acceptTerms')?.touched) {
                  <label class="label">
                    <span class="label-text-alt text-error">You must accept the terms to continue</span>
                  </label>
                }
              </div>

              <!-- Submit Button -->
              <button
                type="submit"
                class="btn btn-primary w-full"
                [disabled]="registerForm.invalid || isLoading()"
              >
                @if (isLoading()) {
                  <span class="loading loading-spinner loading-sm"></span>
                }
                Create Account
              </button>
            </form>

            <!-- Login Link -->
            <div class="divider text-sm">OR</div>
            <p class="text-center text-sm">
              Already have an account?
              <a routerLink="/auth/login" class="link link-primary">Sign In</a>
            </p>
          }
        </div>
      </div>
    </div>
  `
})
export class RegisterComponent {
  private readonly fb = inject(FormBuilder);
  private readonly apiService = inject(ApiService);

  isLoading = signal(false);
  errorMessage = signal('');
  registrationSuccess = signal(false);
  passwordStrength = signal(0);
  passwordStrengthLabel = signal('');

  registerForm = this.fb.group({
    firstName: ['', [Validators.required]],
    lastName: ['', [Validators.required]],
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(8)]],
    confirmPassword: ['', [Validators.required]],
    acceptTerms: [false, [Validators.requiredTrue]]
  }, { validators: [this.passwordMatchValidator] });

  constructor() {
    this.registerForm.get('password')?.valueChanges.subscribe(value => {
      this.calculatePasswordStrength(value || '');
    });
  }

  onSubmit(): void {
    if (this.registerForm.invalid) return;

    this.isLoading.set(true);
    this.errorMessage.set('');

    const { firstName, lastName, email, password } = this.registerForm.getRawValue();

    this.apiService.post('auth/register', { firstName, lastName, email, password }).subscribe({
      next: () => {
        this.isLoading.set(false);
        this.registrationSuccess.set(true);
      },
      error: (err) => {
        this.isLoading.set(false);
        this.errorMessage.set(err?.error?.message || 'Registration failed. Please try again.');
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

  private calculatePasswordStrength(password: string): void {
    let strength = 0;
    if (password.length >= 8) strength++;
    if (/[a-z]/.test(password) && /[A-Z]/.test(password)) strength++;
    if (/\d/.test(password)) strength++;
    if (/[^a-zA-Z0-9]/.test(password)) strength++;

    this.passwordStrength.set(strength);

    const labels = ['', 'Weak', 'Fair', 'Good', 'Strong'];
    this.passwordStrengthLabel.set(labels[strength] || '');
  }
}
