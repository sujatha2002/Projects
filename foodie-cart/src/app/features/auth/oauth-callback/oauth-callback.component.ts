import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-oauth-callback',
  standalone: true,
  template: `<div class="loading">Completing sign in...</div>`,
  styles: [`.loading { display: flex; justify-content: center; align-items: center; min-height: 50vh; font-size: 1.2rem; color: #7f8c8d; }`]
})
export class OAuthCallbackComponent implements OnInit {
  constructor(private route: ActivatedRoute, private auth: AuthService, private router: Router) {}

  ngOnInit() {
    const token = this.route.snapshot.queryParamMap.get('token');
    const email = this.route.snapshot.queryParamMap.get('email');
    const role = this.route.snapshot.queryParamMap.get('role') || 'Customer';
    if (token && email) {
      this.auth.handleOAuthToken(token, email, role);
      this.router.navigate(['/']);
    } else {
      this.router.navigate(['/login']);
    }
  }
}
