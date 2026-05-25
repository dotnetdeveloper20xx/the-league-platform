import { Component } from '@angular/core';

@Component({
  selector: 'app-register',
  standalone: true,
  template: `
    <div class="min-h-screen flex items-center justify-center bg-base-200">
      <div class="card bg-base-100 shadow-xl w-full max-w-md">
        <div class="card-body">
          <h2 class="card-title text-2xl justify-center">Register</h2>
          <p class="text-center text-base-content/70">Create your account</p>
        </div>
      </div>
    </div>
  `
})
export class RegisterComponent {}
