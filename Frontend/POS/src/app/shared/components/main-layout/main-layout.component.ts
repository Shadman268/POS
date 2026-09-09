import { Component, OnInit, OnDestroy } from '@angular/core';
import { NavigationEnd, Router } from '@angular/router';
import { MatDialog } from '@angular/material/dialog';
import { AuthService } from '../../../core/services/auth.service';
import { TenantSettingsService } from '../../services/tenant-settings.service';
import { ProductService } from '../../services/product.service';
import { ReceiptService } from '../../services/receipt.service';
import { AdjustReceiptDialogComponent } from '../../dialogs/adjust-receipt-dialog/adjust-receipt-dialog.component';
import { User } from '../../../core/models/user';
import { filter, Subscription } from 'rxjs';

interface NavChild {
  label: string;
  route: string;
}

interface NavItem {
  label: string;
  icon: string;
  route?: string;
  disabled?: boolean;
  children?: NavChild[];
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
  expandedMenus = new Set<string>();
  shopName = 'MedPoint';
  calculatorOpen = false;

  private routerSub?: Subscription;
  private clockInterval?: ReturnType<typeof setInterval>;

  navItems: NavItem[] = [
    { label: 'Dashboard', icon: 'dashboard', route: '/dashboard' },
    { label: 'Point of Sale', icon: 'point_of_sale', route: '/pos' },
    {
      label: 'Products',
      icon: 'medication',
      children: [
        { label: 'All Products', route: '/products' },
        { label: 'Catalog', route: '/products/catalog' }
      ]
    },
    { label: 'Inventory', icon: 'inventory_2', disabled: true },
    { label: 'Purchases', icon: 'shopping_basket', disabled: true },
    { label: 'Customers', icon: 'people', disabled: true },
    { label: 'Sales', icon: 'payments', disabled: true },
    { label: 'Reports', icon: 'bar_chart', disabled: true },
    { label: 'User Management', icon: 'manage_accounts', disabled: true },
    {
      label: 'Settings',
      icon: 'settings',
      children: [
        { label: 'Product Settings', route: '/settings/product' },
        { label: 'Receipt Settings', route: '/settings/receipt' },
        { label: 'Line Settings', route: '/settings/line' }
      ]
    }
  ];

  constructor(
    private authService: AuthService,
    private tenantSettingsService: TenantSettingsService,
    private router: Router,
    private dialog: MatDialog,
    private receiptService: ReceiptService,
    public productService: ProductService
  ) {}

  ngOnInit(): void {
    if (window.innerWidth <= 768) {
      this.sidebarCollapsed = true;
    }

    this.authService.currentUser.subscribe(user => {
      this.currentUser = user;
      if (user) {
        this.tenantSettingsService.loadSettings().subscribe();
      }
    });

    this.tenantSettingsService.settings$.subscribe(settings => {
      this.shopName = settings?.name?.trim() || this.currentUser?.tenantName || 'MedPoint';
    });

    this.updateClock();
    this.clockInterval = setInterval(() => this.updateClock(), 1000);

    this.syncExpandedMenus(this.router.url);

    this.routerSub = this.router.events
      .pipe(filter(event => event instanceof NavigationEnd))
      .subscribe((event) => {
        const navEnd = event as NavigationEnd;
        this.syncExpandedMenus(navEnd.urlAfterRedirects);
        this.onNavClick();
      });
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

  toggleSubmenu(item: NavItem): void {
    if (this.sidebarCollapsed) {
      this.sidebarCollapsed = false;
      this.expandedMenus.add(item.label);
      return;
    }

    if (this.expandedMenus.has(item.label)) {
      this.expandedMenus.delete(item.label);
    } else {
      this.expandedMenus.add(item.label);
    }
  }

  isSubmenuExpanded(item: NavItem): boolean {
    return this.expandedMenus.has(item.label);
  }

  isNavGroupActive(item: NavItem): boolean {
    if (!item.children?.length) {
      return false;
    }
    return item.children.some(child => this.router.url.startsWith(child.route));
  }

  goToPos(): void {
    this.router.navigate(['/pos']);
  }

  openReturnReceipt(): void {
    const dialogRef = this.dialog.open(AdjustReceiptDialogComponent, {
      width: '420px',
      maxWidth: '95vw'
    });

    dialogRef.afterClosed().subscribe(receiptNo => {
      if (!receiptNo) {
        return;
      }

      const receiptId = this.parseReceiptNumber(receiptNo);
      if (!receiptId) {
        window.alert('Enter a valid receipt number.');
        return;
      }

      if (this.productService.receiptItems.length > 0 && !this.productService.isAdjustmentMode) {
        const proceed = window.confirm('The current cart will be replaced with this receipt. Continue?');
        if (!proceed) {
          return;
        }
      }

      this.receiptService.getReturnableReceipt(receiptId).subscribe({
        next: (receipt) => {
          if (!receipt.items?.length) {
            window.alert('This receipt has no items to return.');
            return;
          }

          this.productService.loadAdjustmentCart(receipt);
          if (!this.router.url.startsWith('/pos')) {
            this.router.navigate(['/pos']);
          }
        },
        error: (error) => {
          if (error.status === 404) {
            window.alert(`Receipt #${receiptId} was not found.`);
            return;
          }
          window.alert(error.error?.message || 'Could not load that receipt.');
        }
      });
    });
  }

  private parseReceiptNumber(value: string): number | null {
    const digits = (value || '').replace(/[^\d]/g, '');
    if (!digits) {
      return null;
    }

    const id = parseInt(digits, 10);
    return id > 0 ? id : null;
  }

  toggleCalculator(): void {
    this.calculatorOpen = !this.calculatorOpen;
  }

  closeCalculator(): void {
    this.calculatorOpen = false;
  }

  logout(): void {
    this.authService.logout();
  }

  private syncExpandedMenus(url: string): void {
    if (url.startsWith('/products')) {
      this.expandedMenus.add('Products');
    }
    if (url.startsWith('/settings')) {
      this.expandedMenus.add('Settings');
    }
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
