import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [FormsModule, RouterLink],
  template: `
    <div class="auth-container">
      <div class="auth-card">
        <h2>Create Account</h2>
        <p class="subtitle">Join Foodie Cart for AI-powered recipe suggestions</p>

        @if (error) { <div class="error">{{ error }}</div> }

        <form (ngSubmit)="onRegister()">
          <div class="form-group">
            <label>Full Name</label>
            <input type="text" [(ngModel)]="fullName" name="fullName" required />
          </div>
          <div class="form-group">
            <label>Email</label>
            <input type="email" [(ngModel)]="email" name="email" required />
          </div>
          <div class="form-group">
            <label>Password</label>
            <input type="password" [(ngModel)]="password" name="password" required />
          </div>
          <div class="form-group">
            <label>Role</label>
            <select [(ngModel)]="role" name="role">
              <option value="Customer">Customer</option>
              <option value="Vendor">Vendor</option>
            </select>
          </div>
          <button type="submit" class="btn-primary" [disabled]="loading">
            {{ loading ? 'Creating...' : 'Create Account' }}
          </button>
        </form>
        <p class="footer-link">Already have an account? <a routerLink="/login">Sign in</a></p>
      </div>
    </div>
  `,
  styles: [`
    .auth-container { display: flex; justify-content: center; align-items: center; min-height: calc(100vh - 70px); padding: 2rem; }
    .auth-card { background: white; border-radius: 16px; padding: 2.5rem; width: 100%; max-width: 420px; box-shadow: 0 10px 40px rgba(0,0,0,0.1); }
    h2 { margin: 0 0 0.5rem; color: #2c3e50; }
    .subtitle { color: #7f8c8d; margin-bottom: 1.5rem; }
    .form-group { margin-bottom: 1rem; }
    label { display: block; margin-bottom: 0.3rem; font-weight: 500; color: #34495e; }
    input, select { width: 100%; padding: 0.75rem; border: 1px solid #ddd; border-radius: 8px; font-size: 1rem; box-sizing: border-box; }
    .btn-primary { width: 100%; padding: 0.85rem; background: #e74c3c; color: white; border: none; border-radius: 8px; font-size: 1rem; font-weight: 600; cursor: pointer; margin-top: 0.5rem; }
    .footer-link { text-align: center; margin-top: 1.5rem; color: #7f8c8d; }
    .footer-link a { color: #e74c3c; }
    .error { background: #fde8e8; color: #c0392b; padding: 0.75rem; border-radius: 8px; margin-bottom: 1rem; }
  `]
})
export class RegisterComponent {
  fullName = '';
  email = '';
  password = '';
  role = 'Customer';
  error = '';
  loading = false;

  constructor(private auth: AuthService, private router: Router) {}

  onRegister() {
    this.loading = true;
    this.error = '';
    this.auth.register(this.email, this.password, this.fullName, this.role).subscribe({
      next: () => { this.loading = false; this.router.navigate(['/']); },
      error: (err) => { this.loading = false; this.error = err.error?.[0] || 'Registration failed'; }
    });
  }
}
