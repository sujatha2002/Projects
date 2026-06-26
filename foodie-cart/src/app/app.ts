import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { NavbarComponent } from './shared/navbar/navbar.component';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, NavbarComponent],
  template: `
    <app-navbar />
    <main>
      <router-outlet />
    </main>
  `,
  styles: [`
    :host { display: block; min-height: 100vh; }
    main { background: #f5f6fa; min-height: calc(100vh - 60px); }
  `]
})
export class App {}
