import { Component, OnInit, OnDestroy } from '@angular/core';
import { NavigationEnd, Router } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { User } from '../../../core/models/user';
import { filter, Subscription } from 'rxjs';

interface NavItem {
  label: string;
  icon: string;
  route?: string;
  disabled?: boolean;
}

@Component({
  selector: 'app-main-layout',
  templateUrl: './main-layout.component.html',
  styleUrls: ['./main-layout.component.scss']
})
export class MainLayoutComponent implements OnInit, OnDestroy {
  sidebarCollapsed = false;
  currentUser: User | null = null;
  currentDate = '';
  currentTime = '';
  selectedLanguage = 'English';
  languages = ['English', 'Bengali'];

  private routerSub?: Subscription;
  private clockInterval?: ReturnType<typeof setInterval>;

  navItems: NavItem[] = [
    { label: 'Dashboard', icon: 'dashboard', route: '/dashboard' },
    { label: 'Point of Sale', icon: 'point_of_sale', route: '/pos' },
    { label: 'Products', icon: 'medication', disabled: true },
    { label: 'Inventory', icon: 'inventory_2', disabled: true },
    { label: 'Purchases', icon: 'shopping_basket', disabled: true },
    { label: 'Customers', icon: 'people', disabled: true },
    { label: 'Sales', icon: 'payments', disabled: true },
    { label: 'Reports', icon: 'bar_chart', disabled: true },
    { label: 'User Management', icon: 'manage_accounts', disabled: true },
    { label: 'Settings', icon: 'settings', disabled: true }
  ];

  constructor(
    private authService: AuthService,
    private router: Router
  ) {}

  ngOnInit(): void {
    if (window.innerWidth <= 768) {
      this.sidebarCollapsed = true;
    }

    this.authService.currentUser.subscribe(user => {
      this.currentUser = user;
    });

    this.updateClock();
    this.clockInterval = setInterval(() => this.updateClock(), 1000);

    this.routerSub = this.router.events
      .pipe(filter(event => event instanceof NavigationEnd))
      .subscribe(() => this.onNavClick());
  }

  ngOnDestroy(): void {
    this.routerSub?.unsubscribe();
    if (this.clockInterval) {
      clearInterval(this.clockInterval);
    }
  }

  get userInitials(): string {
    const name = this.currentUser?.username || 'U';
    return name.substring(0, 2).toUpperCase();
  }

  get displayName(): string {
    if (!this.currentUser?.username) {
      return 'User';
    }
    return this.currentUser.username.charAt(0).toUpperCase() + this.currentUser.username.slice(1);
  }

  toggleCollapse(): void {
    this.sidebarCollapsed = !this.sidebarCollapsed;
  }

  onNavClick(): void {
    if (window.innerWidth <= 768) {
      this.sidebarCollapsed = true;
    }
  }

  logout(): void {
    this.authService.logout();
  }

  private updateClock(): void {
    const now = new Date();
    this.currentDate = now.toLocaleDateString('en-US', {
      month: 'short',
      day: 'numeric',
      year: 'numeric'
    });
    this.currentTime = now.toLocaleTimeString('en-US', {
      hour: 'numeric',
      minute: '2-digit',
      hour12: true
    });
  }
}
