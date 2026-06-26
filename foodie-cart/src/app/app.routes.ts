import { Routes } from '@angular/router';
import { authGuard, roleGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  { path: '', loadComponent: () => import('./features/recipes/recipe-list/recipe-list.component').then(m => m.RecipeListComponent) },
  { path: 'login', loadComponent: () => import('./features/auth/login/login.component').then(m => m.LoginComponent) },
  { path: 'register', loadComponent: () => import('./features/auth/register/register.component').then(m => m.RegisterComponent) },
  { path: 'oauth-callback', loadComponent: () => import('./features/auth/oauth-callback/oauth-callback.component').then(m => m.OAuthCallbackComponent) },
  { path: 'cart', canActivate: [authGuard], loadComponent: () => import('./features/cart/cart/cart.component').then(m => m.CartComponent) },
  { path: 'admin', canActivate: [roleGuard('Admin')], loadComponent: () => import('./features/admin/dashboard/dashboard.component').then(m => m.DashboardComponent) },
  { path: '**', redirectTo: '' }
];
