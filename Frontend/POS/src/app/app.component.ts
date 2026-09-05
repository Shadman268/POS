import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from './core/services/auth.service';
import { environment } from '../environments/environment';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent implements OnInit {
  title = 'POS';

  constructor(
    private authService: AuthService,
    private router: Router
  ) { }

  ngOnInit(): void {
    if (environment.disableAuth) {
      if (!this.authService.isAuthenticated()) {
        this.authService.bootstrapDevSession();
      }
      if (this.router.url === '/login' || this.router.url === '/register') {
        this.router.navigate(['/dashboard']);
      }
      return;
    }

    if (!this.authService.isAuthenticated() && this.router.url !== '/login') {
      this.router.navigate(['/login']);
    }
  }
}
