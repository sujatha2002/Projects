import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { CartService } from '../../../core/services/cart.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [FormsModule, RouterLink],
  template: `
    <div class="auth-container">
      <div class="auth-card">
        <h2>Welcome Back</h2>
        <p class="subtitle">Sign in to get personalized recipe suggestions</p>

        @if (error) { <div class="error">{{ error }}</div> }

        <form (ngSubmit)="onLogin()">
          <div class="form-group">
            <label>Email</label>
            <input type="email" [(ngModel)]="email" name="email" required placeholder="you@example.com" />
          </div>
          <div class="form-group">
            <label>Password</label>
            <input type="password" [(ngModel)]="password" name="password" required placeholder="••••••••" />
          </div>
          <button type="submit" class="btn-primary" [disabled]="loading">
            {{ loading ? 'Signing in...' : 'Sign In' }}
          </button>
        </form>

        <div class="divider"><span>or continue with</span></div>

        <div class="oauth-buttons">
          <button class="btn-oauth google" (click)="auth.loginWithGoogle()">Google</button>
          <button class="btn-oauth facebook" (click)="auth.loginWithFacebook()">Facebook</button>
        </div>

        <p class="footer-link">Don't have an account? <a routerLink="/register">Sign up</a></p>

        <div class="demo-accounts">
          <p>Demo accounts:</p>
          <small>admin@foodiecart.com / Admin&#64;123</small><br>
          <small>alice@example.com / Customer&#64;123 (Vegetarian)</small>
        </div>
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
    input { width: 100%; padding: 0.75rem; border: 1px solid #ddd; border-radius: 8px; font-size: 1rem; box-sizing: border-box; }
    input:focus { outline: none; border-color: #e74c3c; }
    .btn-primary { width: 100%; padding: 0.85rem; background: #e74c3c; color: white; border: none; border-radius: 8px; font-size: 1rem; font-weight: 600; cursor: pointer; margin-top: 0.5rem; }
    .btn-primary:disabled { opacity: 0.6; }
    .divider { text-align: center; margin: 1.5rem 0; color: #bdc3c7; position: relative; }
    .divider span { background: white; padding: 0 1rem; position: relative; z-index: 1; }
    .divider::before { content: ''; position: absolute; top: 50%; left: 0; right: 0; height: 1px; background: #eee; }
    .oauth-buttons { display: flex; gap: 1rem; }
    .btn-oauth { flex: 1; padding: 0.7rem; border: 1px solid #ddd; border-radius: 8px; cursor: pointer; font-weight: 500; }
    .btn-oauth.google { background: #fff; }
    .btn-oauth.facebook { background: #1877f2; color: white; border-color: #1877f2; }
    .footer-link { text-align: center; margin-top: 1.5rem; color: #7f8c8d; }
    .footer-link a { color: #e74c3c; }
    .error { background: #fde8e8; color: #c0392b; padding: 0.75rem; border-radius: 8px; margin-bottom: 1rem; }
    .demo-accounts { margin-top: 1.5rem; padding: 1rem; background: #f8f9fa; border-radius: 8px; font-size: 0.85rem; color: #7f8c8d; }
  `]
})
export class LoginComponent {
  email = '';
  password = '';
  error = '';
  loading = false;

  constructor(public auth: AuthService, private router: Router, private cartService: CartService) {}

  onLogin() {
    this.loading = true;
    this.error = '';
    this.auth.login(this.email, this.password).subscribe({
      next: () => {
        this.loading = false;
        this.cartService.getCart().subscribe();
        this.router.navigate(['/']);
      },
      error: () => { this.loading = false; this.error = 'Invalid email or password'; }
    });
  }
}
