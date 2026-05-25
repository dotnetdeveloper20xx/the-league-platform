import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-onboarding',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="min-h-screen bg-base-200 p-4">
      <div class="max-w-2xl mx-auto">
        <!-- Progress Steps -->
        <ul class="steps steps-horizontal w-full mb-8">
          <li class="step" [class.step-primary]="currentStep() >= 1">Choose Plan</li>
          <li class="step" [class.step-primary]="currentStep() >= 2">Club Profile</li>
          <li class="step" [class.step-primary]="currentStep() >= 3">Membership Types</li>
          <li class="step" [class.step-primary]="currentStep() >= 4">Import Members</li>
        </ul>

        <!-- Step 1: Choose Plan -->
        @if (currentStep() === 1) {
          <div class="card bg-base-100 shadow-lg">
            <div class="card-body">
              <h2 class="card-title text-2xl">Choose Your Plan</h2>
              <div class="grid grid-cols-1 md:grid-cols-2 gap-4 mt-4">
                @for (tier of tiers; track tier.name) {
                  <div
                    class="card border-2 cursor-pointer transition-all"
                    [class.border-primary]="selectedTier() === tier.name"
                    [class.border-base-300]="selectedTier() !== tier.name"
                    (click)="selectedTier.set(tier.name)">
                    <div class="card-body">
                      <h3 class="card-title">{{ tier.name }}</h3>
                      <p class="text-2xl font-bold">{{ tier.price }}</p>
                      <ul class="text-sm space-y-1 mt-2">
                        @for (feature of tier.features; track feature) {
                          <li>✓ {{ feature }}</li>
                        }
                      </ul>
                    </div>
                  </div>
                }
              </div>
              <div class="card-actions justify-end mt-4">
                <button class="btn btn-primary" (click)="nextStep()" [disabled]="!selectedTier()">
                  Continue
                </button>
              </div>
            </div>
          </div>
        }

        <!-- Step 2: Club Profile -->
        @if (currentStep() === 2) {
          <div class="card bg-base-100 shadow-lg">
            <div class="card-body">
              <h2 class="card-title text-2xl">Club Profile</h2>
              <form class="space-y-4 mt-4">
                <div class="form-control">
                  <label class="label"><span class="label-text">Club Name</span></label>
                  <input type="text" class="input input-bordered" [(ngModel)]="clubProfile.name" name="name" required />
                </div>
                <div class="form-control">
                  <label class="label"><span class="label-text">Sport Type</span></label>
                  <select class="select select-bordered" [(ngModel)]="clubProfile.sportType" name="sportType">
                    <option value="Cricket">Cricket</option>
                    <option value="Football">Football</option>
                    <option value="Hockey">Hockey</option>
                    <option value="Rugby">Rugby</option>
                    <option value="Tennis">Tennis</option>
                    <option value="Swimming">Swimming</option>
                    <option value="Athletics">Athletics</option>
                    <option value="Golf">Golf</option>
                    <option value="MultiSport">Multi-Sport</option>
                  </select>
                </div>
                <div class="form-control">
                  <label class="label"><span class="label-text">Contact Email</span></label>
                  <input type="email" class="input input-bordered" [(ngModel)]="clubProfile.email" name="email" />
                </div>
              </form>
              <div class="card-actions justify-between mt-4">
                <button class="btn btn-ghost" (click)="prevStep()">Back</button>
                <div class="flex gap-2">
                  <button class="btn btn-ghost" (click)="skipStep()">Skip</button>
                  <button class="btn btn-primary" (click)="nextStep()">Continue</button>
                </div>
              </div>
            </div>
          </div>
        }

        <!-- Step 3: Membership Types -->
        @if (currentStep() === 3) {
          <div class="card bg-base-100 shadow-lg">
            <div class="card-body">
              <h2 class="card-title text-2xl">Membership Types</h2>
              <p class="text-base-content/70">Configure your membership categories (you can change these later)</p>
              <div class="mt-4 text-center py-8 border-2 border-dashed border-base-300 rounded-lg">
                <p class="text-base-content/50">Membership type configuration — coming soon</p>
              </div>
              <div class="card-actions justify-between mt-4">
                <button class="btn btn-ghost" (click)="prevStep()">Back</button>
                <div class="flex gap-2">
                  <button class="btn btn-ghost" (click)="skipStep()">Skip</button>
                  <button class="btn btn-primary" (click)="nextStep()">Continue</button>
                </div>
              </div>
            </div>
          </div>
        }

        <!-- Step 4: Import Members -->
        @if (currentStep() === 4) {
          <div class="card bg-base-100 shadow-lg">
            <div class="card-body">
              <h2 class="card-title text-2xl">Import Members</h2>
              <p class="text-base-content/70">Upload a CSV or Excel file with your existing members</p>
              <div class="mt-4 text-center py-8 border-2 border-dashed border-base-300 rounded-lg">
                <p class="text-base-content/50 mb-2">Drag & drop your file here or click to browse</p>
                <input type="file" class="file-input file-input-bordered" accept=".csv,.xlsx,.xls" />
              </div>
              <div class="card-actions justify-between mt-4">
                <button class="btn btn-ghost" (click)="prevStep()">Back</button>
                <div class="flex gap-2">
                  <button class="btn btn-ghost" (click)="skipStep()">Skip</button>
                  <button class="btn btn-success" (click)="completeOnboarding()">Complete Setup</button>
                </div>
              </div>
            </div>
          </div>
        }

        <!-- Completion -->
        @if (currentStep() === 5) {
          <div class="card bg-base-100 shadow-lg">
            <div class="card-body text-center">
              <div class="text-6xl mb-4">🎉</div>
              <h2 class="card-title text-2xl justify-center">You're All Set!</h2>
              <p class="text-base-content/70">Your club is ready to go. Start managing your members, sessions, and events.</p>
              <div class="card-actions justify-center mt-6">
                <a href="/club/dashboard" class="btn btn-primary btn-lg">Go to Dashboard</a>
              </div>
            </div>
          </div>
        }
      </div>
    </div>
  `
})
export class OnboardingComponent {
  currentStep = signal(1);
  selectedTier = signal('');

  clubProfile = { name: '', sportType: 'Cricket', email: '' };

  tiers = [
    { name: 'Free', price: '£0/month', features: ['50 members', '1 GB storage', 'Basic features'] },
    { name: 'Starter', price: '£29/month', features: ['200 members', '5 GB storage', 'SMS credits', 'All core features'] },
    { name: 'Pro', price: '£79/month', features: ['1000 members', '25 GB storage', '2000 SMS/month', 'Analytics', 'Integrations'] },
    { name: 'Enterprise', price: '£199/month', features: ['Unlimited members', '100 GB storage', '10000 SMS/month', 'White-label', 'Priority support'] }
  ];

  nextStep(): void { this.currentStep.update(s => Math.min(s + 1, 5)); }
  prevStep(): void { this.currentStep.update(s => Math.max(s - 1, 1)); }
  skipStep(): void { this.nextStep(); }
  completeOnboarding(): void { this.currentStep.set(5); }
}
