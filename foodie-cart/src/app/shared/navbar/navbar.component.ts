import { Component, OnInit } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';
import { CartService } from '../../core/services/cart.service';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [RouterLink, RouterLinkActive],
  template: `
    <nav class="navbar">
      <div class="nav-brand">
        <a routerLink="/">🍽️ Foodie Cart</a>
      </div>
      <div class="nav-links">
        <a routerLink="/" routerLinkActive="active" [routerLinkActiveOptions]="{exact: true}">Recipes</a>
        @if (auth.isLoggedIn()) {
          <a routerLink="/cart" routerLinkActive="active">Cart @if (cartService.cartCount() > 0) { ({{ cartService.cartCount() }}) }</a>
          @if (auth.hasRole('Admin')) {
            <a routerLink="/admin" routerLinkActive="active">Analytics</a>
          }
          <span class="user-info">{{ auth.currentUser()?.fullName }} ({{ auth.currentUser()?.role }})</span>
          <button class="btn-logout" (click)="auth.logout()">Logout</button>
        } @else {
          <a routerLink="/login" routerLinkActive="active">Login</a>
          <a routerLink="/register" routerLinkActive="active" class="btn-register">Sign Up</a>
        }
      </div>
    </nav>
  `,
  styles: [`
    .navbar {
      display: flex; justify-content: space-between; align-items: center;
      padding: 1rem 2rem; background: linear-gradient(135deg, #e74c3c, #c0392b);
      color: white; box-shadow: 0 2px 10px rgba(0,0,0,0.15);
    }
    .nav-brand a { color: white; text-decoration: none; font-size: 1.4rem; font-weight: 700; }
    .nav-links { display: flex; align-items: center; gap: 1.5rem; }
    .nav-links a { color: rgba(255,255,255,0.9); text-decoration: none; font-weight: 500; transition: color 0.2s; }
    .nav-links a:hover, .nav-links a.active { color: white; text-decoration: underline; }
    .user-info { font-size: 0.85rem; opacity: 0.9; }
    .btn-logout, .btn-register {
      background: rgba(255,255,255,0.2); border: 1px solid rgba(255,255,255,0.4);
      color: white; padding: 0.4rem 1rem; border-radius: 20px; cursor: pointer; font-weight: 500;
    }
    .btn-register { background: white; color: #e74c3c; text-decoration: none !important; }
  `]
})
export class NavbarComponent implements OnInit {
  constructor(public auth: AuthService, public cartService: CartService) {}

  ngOnInit() {
    if (this.auth.isLoggedIn()) {
      this.cartService.getCart().subscribe();
    }
  }
}
