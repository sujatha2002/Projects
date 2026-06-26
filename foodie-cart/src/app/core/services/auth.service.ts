import { Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { tap } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { AuthResponse } from '../models';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly api = `${environment.apiUrl}/auth`;
  currentUser = signal<AuthResponse | null>(this.loadUser());

  constructor(private http: HttpClient, private router: Router) {}

  login(email: string, password: string) {
    return this.http.post<AuthResponse>(`${this.api}/login`, { email, password }).pipe(
      tap(res => this.setSession(res))
    );
  }

  register(email: string, password: string, fullName: string, role = 'Customer') {
    return this.http.post<AuthResponse>(`${this.api}/register`, { email, password, fullName, role }).pipe(
      tap(res => this.setSession(res))
    );
  }

  handleOAuthToken(token: string, email: string, role: string) {
    const user: AuthResponse = { token, email, fullName: email, role, expiresAt: '' };
    this.setSession(user);
  }

  logout() {
    localStorage.removeItem('foodie_user');
    this.currentUser.set(null);
    this.router.navigate(['/login']);
  }

  getToken(): string | null {
    return this.currentUser()?.token ?? null;
  }

  isLoggedIn(): boolean {
    return !!this.getToken();
  }

  hasRole(role: string): boolean {
    return this.currentUser()?.role === role;
  }

  loginWithGoogle() {
    window.location.href = 'http://localhost:5149/api/auth/google';
  }

  loginWithFacebook() {
    window.location.href = 'http://localhost:5149/api/auth/facebook';
  }

  private setSession(user: AuthResponse) {
    const normalized: AuthResponse = {
      token: user.token ?? (user as any).Token,
      email: user.email ?? (user as any).Email,
      fullName: user.fullName ?? (user as any).FullName,
      role: user.role ?? (user as any).Role,
      expiresAt: user.expiresAt ?? (user as any).ExpiresAt ?? ''
    };
    localStorage.setItem('foodie_user', JSON.stringify(normalized));
    this.currentUser.set(normalized);
  }

  private loadUser(): AuthResponse | null {
    const data = localStorage.getItem('foodie_user');
    if (!data) return null;
    const parsed = JSON.parse(data);
    return {
      token: parsed.token ?? parsed.Token,
      email: parsed.email ?? parsed.Email,
      fullName: parsed.fullName ?? parsed.FullName,
      role: parsed.role ?? parsed.Role,
      expiresAt: parsed.expiresAt ?? parsed.ExpiresAt ?? ''
    };
  }
}
