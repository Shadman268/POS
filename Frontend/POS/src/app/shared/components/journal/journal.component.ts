import {
  Component,
  ElementRef,
  EventEmitter,
  HostListener,
  OnDestroy,
  OnInit,
  Output
} from '@angular/core';
import { FormControl } from '@angular/forms';
import { MatDialog } from '@angular/material/dialog';
import { Subject } from 'rxjs';
import {
  debounceTime,
  distinctUntilChanged,
  filter,
  switchMap,
  takeUntil,
  tap
} from 'rxjs/operators';
import { ProductService } from '../../services/product.service';
import { TenantSettingsService } from '../../services/tenant-settings.service';
import { ReceiptService } from '../../services/receipt.service';
import { ReceiptPdfService } from '../../services/receipt-pdf.service';
import { ReceiptData, ReceiptItemData } from '../../../core/models/receipt';
import { LineDiscountUnit, ProductView } from '../../../core/models/product-data';
import { formatAmount } from '../../pipes/amount.pipe';
import {
  ChargeDialogComponent,
  ChargeDialogData,
  ChargeDialogResult
} from '../../dialogs/charge-dialog/charge-dialog.component';
import { AddCustomerDialogComponent } from '../../dialogs/add-customer-dialog/add-customer-dialog.component';
import { CustomerService } from '../../services/customer.service';
import {
  CreateCustomerRequest,
  Customer,
  WALK_IN_CUSTOMER_ID,
  WALK_IN_CUSTOMER_NAME
} from '../../../core/models/customer';

@Component({
  selector: 'app-journal',
  templateUrl: './journal.component.html',
  styleUrls: ['./journal.component.scss']
})
export class JournalComponent implements OnInit, OnDestroy {
  @Output() addProductRequest = new EventEmitter<void>();

  readonly walkInCustomerId = WALK_IN_CUSTOMER_ID;
  readonly walkInCustomerName = WALK_IN_CUSTOMER_NAME;
  selectedCustomerId = WALK_IN_CUSTOMER_ID;
  customerName = WALK_IN_CUSTOMER_NAME;
  customers: Customer[] = [];
  searchControl = new FormControl('');
  filteredProducts: ProductView[] = [];
  activeIndex = -1;
  searchLoading = false;
  showSearchDropdown = false;
  discountAmount = 0;
  discountUnit: LineDiscountUnit = 'BDT';
  showLineDiscount = false;
  showVat = false;
  vatPercent = 5;
  submitting = false;

  private readonly destroy$ = new Subject<void>();
  private latestSearchQuery = '';
  private loadedAdjustmentId: number | null = null;
  private searchDropdownDismissed = false;

  constructor(
    protected productService: ProductService,
    private tenantSettingsService: TenantSettingsService,
    private receiptService: ReceiptService,
    private receiptPdfService: ReceiptPdfService,
    private customerService: CustomerService,
    private dialog: MatDialog,
    private host: ElementRef<HTMLElement>
  ) {}

  ngOnInit(): void {
    this.tenantSettingsService.settings$.pipe(takeUntil(this.destroy$)).subscribe(settings => {
      this.showLineDiscount = settings?.showLineDiscount ?? false;
      this.showVat = settings?.showVat ?? false;
      this.vatPercent = settings?.vatPercent ?? 5;
    });

    this.loadCustomers();
    this.customerService.customers$.pipe(takeUntil(this.destroy$)).subscribe(customers => {
      this.customers = customers;
      this.syncSelectedCustomer();
    });

    this.productService.cartChanged$.pipe(takeUntil(this.destroy$)).subscribe(() => {
      const context = this.productService.adjustmentContext;
      if (context && context.originalReceiptId !== this.loadedAdjustmentId) {
        this.loadedAdjustmentId = context.originalReceiptId;
        this.selectCustomerByName(context.customerName);
        this.discountAmount = context.originalDiscount;
        this.discountUnit = 'BDT';
      }
      if (!context) {
        this.loadedAdjustmentId = null;
      }
    });

    this.searchControl.valueChanges.pipe(
      debounceTime(250),
      distinctUntilChanged(),
      tap(query => {
        const trimmed = (query || '').trim();
        this.latestSearchQuery = trimmed;
        if (trimmed.length < 2) {
          this.filteredProducts = [];
          this.activeIndex = -1;
          this.searchLoading = false;
          this.showSearchDropdown = false;
        }
      }),
      filter(query => (query || '').trim().length >= 2),
      tap(() => {
        this.searchLoading = true;
        if (!this.searchDropdownDismissed) {
          this.showSearchDropdown = true;
        }
      }),
      switchMap(query => this.productService.searchProducts((query || '').trim())),
      takeUntil(this.destroy$)
    ).subscribe({
      next: (results) => {
        this.searchLoading = false;
        this.filteredProducts = results;
        this.activeIndex = results.length > 0 ? 0 : -1;
        if (!this.searchDropdownDismissed) {
          this.showSearchDropdown = true;
        }
        this.tryAutoAdd(this.latestSearchQuery, results);
      },
      error: () => {
        this.searchLoading = false;
        this.filteredProducts = [];
        this.activeIndex = -1;
        this.showSearchDropdown = false;
      }
    });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  onSearchKeydown(event: KeyboardEvent): void {
    if (this.filteredProducts.length === 0) {
      if (event.key === 'Enter') {
        event.preventDefault();
        this.addFromSearchButton();
      }
      return;
    }

    switch (event.key) {
      case 'ArrowDown':
        event.preventDefault();
        this.activeIndex = (this.activeIndex + 1) % this.filteredProducts.length;
        this.scrollActiveItemIntoView();
        break;
      case 'ArrowUp':
        event.preventDefault();
        this.activeIndex =
          this.activeIndex <= 0
            ? this.filteredProducts.length - 1
            : this.activeIndex - 1;
        this.scrollActiveItemIntoView();
        break;
      case 'Enter':
        event.preventDefault();
        if (this.activeIndex >= 0 && this.activeIndex < this.filteredProducts.length) {
          this.selectAutocomplete(this.filteredProducts[this.activeIndex]);
        } else {
          this.addFromSearchButton();
        }
        break;
      case 'Escape':
        event.preventDefault();
        this.dismissSearchDropdown();
        break;
    }
  }

  onSearchFocus(): void {
    this.searchDropdownDismissed = false;
    const query = (this.searchControl.value || '').trim();
    if (query.length < 2) {
      return;
    }

    this.showSearchDropdown = true;

    if (this.filteredProducts.length === 0 && !this.searchLoading) {
      this.searchLoading = true;
      this.productService.searchProducts(query).subscribe({
        next: (results) => {
          this.searchLoading = false;
          this.filteredProducts = results;
          this.activeIndex = results.length > 0 ? 0 : -1;
        },
        error: () => {
          this.searchLoading = false;
          this.filteredProducts = [];
          this.activeIndex = -1;
          this.showSearchDropdown = false;
        }
      });
    }
  }

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent): void {
    const searchWrap = this.host.nativeElement.querySelector('.search-wrap');
    if (searchWrap && !searchWrap.contains(event.target as Node)) {
      this.dismissSearchDropdown();
    }
  }

  private dismissSearchDropdown(): void {
    this.searchDropdownDismissed = true;
    this.showSearchDropdown = false;
    this.activeIndex = -1;
  }

  private scrollActiveItemIntoView(): void {
    setTimeout(() => {
      const active = this.host.nativeElement.querySelector(
        '.autocomplete-item.active'
      ) as HTMLElement | null;
      active?.scrollIntoView({ block: 'nearest' });
    });
  }

  selectAutocomplete(product: ProductView): void {
    this.addProductToCart(product);
  }

  private clearSearch(): void {
    this.searchControl.setValue('', { emitEvent: false });
    this.latestSearchQuery = '';
    this.filteredProducts = [];
    this.activeIndex = -1;
    this.searchLoading = false;
    this.showSearchDropdown = false;
    this.searchDropdownDismissed = false;
  }

  addProductToCart(product: ProductView): void {
    this.productService.addProductToCart(product, () => this.clearSearch());
  }

  addFromSearchButton(): void {
    const query = (this.searchControl.value || '').trim();
    if (!query) {
      this.addProductRequest.emit();
      return;
    }

    if (this.filteredProducts.length > 0) {
      const best = this.findBestMatch(query, this.filteredProducts);
      if (best) {
        this.addProductToCart(best);
        return;
      }
    }

    if (query.length < 2) {
      this.addProductRequest.emit();
      return;
    }

    this.searchLoading = true;
    this.productService.searchProducts(query).subscribe({
      next: (results) => {
        this.searchLoading = false;
        this.filteredProducts = results;
        const best = this.findBestMatch(query, results);
        if (best) {
          this.addProductToCart(best);
        } else {
          this.addProductRequest.emit();
        }
      },
      error: () => {
        this.searchLoading = false;
        this.addProductRequest.emit();
      }
    });
  }

  private tryAutoAdd(query: string, results: ProductView[]): void {
    if (results.length === 0) {
      return;
    }

    const best = this.findBestMatch(query, results);
    if (!best) {
      return;
    }

    const q = query.toLowerCase();
    const name = best.productName.toLowerCase();
    const generic = best.genericName?.toLowerCase() ?? '';

    if (name === q || generic === q || results.length === 1) {
      this.addProductToCart(best);
      this.clearSearch();
    }
  }

  private findBestMatch(query: string, results: ProductView[]): ProductView | null {
    if (results.length === 0) {
      return null;
    }

    const q = query.toLowerCase();
    const exact = results.find(p =>
      p.productName.toLowerCase() === q ||
      p.genericName?.toLowerCase() === q
    );
    if (exact) {
      return exact;
    }

    const startsWith = results.find(p =>
      p.productName.toLowerCase().startsWith(q) ||
      p.genericName?.toLowerCase().startsWith(q)
    );
    if (startsWith) {
      return startsWith;
    }

    return results[0];
  }

  getSubtotal(): number {
    return this.productService.receiptItems.reduce(
      (sum, item) => sum + this.getLineGrossTotal(item),
      0
    );
  }

  getLineGrossTotal(item: {
    price: number;
    quantity: number;
    lineDiscount?: number;
    lineDiscountUnit?: LineDiscountUnit;
  }): number {
    const gross = item.price * item.quantity;
    if (!this.showLineDiscount) {
      return gross;
    }

    return Math.max(0, gross - this.getLineDiscountAmount(item));
  }

  getLineDiscountAmount(item: {
    price: number;
    quantity: number;
    lineDiscount?: number;
    lineDiscountUnit?: LineDiscountUnit;
  }): number {
    const gross = item.price * item.quantity;
    const value = Math.max(0, item.lineDiscount ?? 0);

    if ((item.lineDiscountUnit ?? 'BDT') === '%') {
      const pct = Math.min(100, value);
      return Math.min(gross, gross * (pct / 100));
    }

    return Math.min(gross, value);
  }

  getDiscountValue(): number {
    const subtotal = this.getSubtotal();
    const value = Math.max(0, this.discountAmount);

    if (this.discountUnit === '%') {
      const pct = Math.min(100, value);
      return Math.min(subtotal, subtotal * (pct / 100));
    }

    return Math.min(value, subtotal);
  }

  needsPrice(product: ProductView): boolean {
    return !product.hasTenantPrice && (product.requiresPrice === true || +product.price <= 0);
  }

  getVatAmount(): number {
    if (!this.showVat) {
      return 0;
    }

    const taxable = this.getSubtotal() - this.getDiscountValue();
    return Math.round(taxable * (this.vatPercent / 100));
  }

  getTotal(): number {
    return this.getSubtotal() - this.getDiscountValue() + this.getVatAmount();
  }

  getAdjustmentDelta(): number {
    const original = this.productService.adjustmentContext?.originalTotal ?? 0;
    return this.roundMoney(this.getTotal() - original);
  }

  getActionLabel(): string {
    if (!this.productService.isAdjustmentMode) {
      return `Charge ৳${formatAmount(this.getTotal())}`;
    }

    const delta = this.getAdjustmentDelta();
    if (delta < 0) {
      return `Return ৳${formatAmount(-delta)}`;
    }
    if (delta > 0) {
      return `Extra charge ৳${formatAmount(delta)}`;
    }
    return 'Confirm adjustment';
  }

  private roundMoney(value: number): number {
    return Math.round((value + Number.EPSILON) * 100) / 100;
  }

  getLineTotal(index: number): number {
    const item = this.productService.receiptItems[index];
    return this.getLineGrossTotal(item);
  }

  onLineDiscountInput(index: number, value: string): void {
    const discount = parseFloat(value);
    const item = this.productService.receiptItems[index];
    let next = isNaN(discount) ? 0 : Math.max(0, discount);

    if ((item.lineDiscountUnit ?? 'BDT') === '%') {
      next = Math.min(100, next);
    }

    this.productService.updateLineDiscount(index, next);
  }

  onLineDiscountUnitChange(index: number, unit: LineDiscountUnit): void {
    this.productService.updateLineDiscountUnit(index, unit);
  }

  decreaseQuantity(index: number): void {
    const item = this.productService.receiptItems[index];
    if (item.quantity > 1) {
      this.productService.updateQuantity(index, item.quantity - 1);
    }
  }

  increaseQuantity(index: number): void {
    const item = this.productService.receiptItems[index];
    this.productService.updateQuantity(index, item.quantity + 1);
  }

  onQuantityInput(index: number, value: string): void {
    const qty = parseInt(value, 10);
    if (!isNaN(qty) && qty > 0) {
      this.productService.updateQuantity(index, qty);
    }
  }

  removeItem(index: number): void {
    this.productService.removeFromReceipt(index);
  }

  holdOrder(): void {
    // Placeholder for hold functionality
  }

  processPayment(): void {
    if (this.productService.isAdjustmentMode) {
      this.processAdjustment();
      return;
    }

    if (this.productService.receiptItems.length === 0 || this.submitting) {
      return;
    }

    const total = this.getTotal();
    const dialogRef = this.dialog.open<ChargeDialogComponent, ChargeDialogData, ChargeDialogResult>(
      ChargeDialogComponent,
      {
        width: '420px',
        maxWidth: '95vw',
        disableClose: true,
        data: { total }
      }
    );

    dialogRef.afterClosed().subscribe(result => {
      if (!result) {
        return;
      }

      this.completeSale(result.cashReceived, result.changeAmount);
    });
  }

  private completeSale(cashReceived: number, changeAmount: number): void {
    const subtotal = this.getSubtotal();
    const discount = this.getDiscountValue();
    const total = this.getTotal();

    const receiptItems: ReceiptItemData[] = this.productService.receiptItems.map(item => ({
      productId: item.productId,
      productName: item.productName,
      quantity: item.quantity,
      price: item.price,
      lineDiscount: this.showLineDiscount ? this.getLineDiscountAmount(item) : 0,
      subtotal: this.getLineGrossTotal(item)
    }));

    const settings = this.tenantSettingsService.settings;

    const receiptData: ReceiptData = {
      customerName: this.customerName,
      shopName: settings?.name,
      total: subtotal,
      discountValue: discount,
      discountUnit: 'BDT',
      priceAfterDiscount: total,
      cashReceived,
      changeAmount,
      items: receiptItems,
      receiptHeader: settings?.receiptHeader,
      receiptFooter: settings?.receiptFooter,
      showLineDiscount: this.showLineDiscount
    };

    this.submitting = true;
    this.receiptService.createReceipt(receiptData).subscribe({
      next: (response) => {
        const preview: ReceiptData = {
          ...receiptData,
          ...response,
          cashReceived,
          changeAmount,
          shopName: settings?.name,
          receiptHeader: settings?.receiptHeader,
          receiptFooter: settings?.receiptFooter,
          showLineDiscount: this.showLineDiscount
        };

        this.productService.refreshCatalog().subscribe({
          next: () => {
            this.receiptPdfService.showReceiptPreview(preview).subscribe(() => {
              this.submitting = false;
              this.clearReceipt();
            });
          },
          error: () => {
            this.receiptPdfService.showReceiptPreview(preview).subscribe(() => {
              this.submitting = false;
              this.clearReceipt();
            });
          }
        });
      },
      error: (error) => {
        this.submitting = false;
        const message = error.error?.message || 'Could not complete sale.';
        window.alert(message);
      }
    });
  }

  processAdjustment(): void {
    const context = this.productService.adjustmentContext;
    if (!context || this.submitting) {
      return;
    }

    const subtotal = this.getSubtotal();
    const discount = this.getDiscountValue();
    const total = this.getTotal();
    const delta = this.getAdjustmentDelta();
    const settings = this.tenantSettingsService.settings;

    const receiptData: ReceiptData = {
      originalReceiptId: context.originalReceiptId,
      customerName: this.customerName,
      shopName: settings?.name,
      total: subtotal,
      discountValue: discount,
      discountUnit: 'BDT',
      priceAfterDiscount: total,
      cashReceived: delta > 0 ? delta : 0,
      changeAmount: delta < 0 ? -delta : 0,
      items: this.productService.receiptItems.map(item => ({
        productId: item.productId,
        productName: item.productName,
        quantity: item.quantity,
        price: item.price,
        lineDiscount: this.showLineDiscount ? this.getLineDiscountAmount(item) : 0,
        subtotal: this.getLineGrossTotal(item)
      })),
      receiptHeader: settings?.receiptHeader,
      receiptFooter: settings?.receiptFooter,
      showLineDiscount: this.showLineDiscount
    };

    this.submitting = true;
    this.receiptService.returnReceipt(receiptData).subscribe({
      next: (response) => {
        const preview: ReceiptData = {
          ...response,
          shopName: settings?.name,
          receiptHeader: settings?.receiptHeader,
          receiptFooter: settings?.receiptFooter,
          showLineDiscount: this.showLineDiscount,
          isAdjustment: true,
          isReturn: delta < 0,
          originalReceiptId: context.originalReceiptId,
          adjustmentDelta: delta
        };

        this.productService.refreshCatalog().subscribe({
          next: () => {
            this.receiptPdfService.showReceiptPreview(preview).subscribe(() => {
              this.submitting = false;
              this.clearReceipt();
            });
          },
          error: () => {
            this.receiptPdfService.showReceiptPreview(preview).subscribe(() => {
              this.submitting = false;
              this.clearReceipt();
            });
          }
        });
      },
      error: (error) => {
        this.submitting = false;
        const message = error.error?.message || 'Could not complete adjustment.';
        window.alert(message);
      }
    });
  }

  cancelAdjustment(): void {
    this.productService.exitAdjustmentMode();
    this.discountAmount = 0;
    this.discountUnit = 'BDT';
    this.resetCustomer();
  }

  clearReceipt(): void {
    this.productService.clearReceipt();
    this.discountAmount = 0;
    this.discountUnit = 'BDT';
    this.resetCustomer();
  }

  onCustomerChange(): void {
    if (this.selectedCustomerId === WALK_IN_CUSTOMER_ID) {
      this.customerName = WALK_IN_CUSTOMER_NAME;
      return;
    }

    const selected = this.customers.find(customer => customer.id === this.selectedCustomerId);
    this.customerName = selected?.name ?? WALK_IN_CUSTOMER_NAME;
  }

  openAddCustomerDialog(): void {
    const dialogRef = this.dialog.open(AddCustomerDialogComponent, {
      width: '420px',
      maxWidth: '95vw'
    });

    dialogRef.afterClosed().subscribe((request: CreateCustomerRequest | undefined) => {
      if (!request) {
        return;
      }

      this.customerService.createCustomer(request).subscribe({
        next: (created) => {
          this.selectedCustomerId = created.id;
          this.customerName = created.name;
        },
        error: (error) => {
          const message = error.error?.message || 'Could not add customer.';
          window.alert(message);
        }
      });
    });
  }

  private loadCustomers(): void {
    this.customerService.getAllCustomers().subscribe({
      error: () => {
        this.customers = [];
      }
    });
  }

  private selectCustomerByName(name: string): void {
    const match = this.customers.find(
      customer => customer.name.toLowerCase() === (name || '').trim().toLowerCase()
    );

    if (match) {
      this.selectedCustomerId = match.id;
      this.customerName = match.name;
      return;
    }

    this.selectedCustomerId = WALK_IN_CUSTOMER_ID;
    this.customerName = name?.trim() || WALK_IN_CUSTOMER_NAME;
  }

  private syncSelectedCustomer(): void {
    if (this.selectedCustomerId !== WALK_IN_CUSTOMER_ID) {
      const selected = this.customers.find(customer => customer.id === this.selectedCustomerId);
      if (!selected) {
        this.selectCustomerByName(this.customerName);
      }
      return;
    }

    if (this.customerName && this.customerName !== WALK_IN_CUSTOMER_NAME) {
      this.selectCustomerByName(this.customerName);
    }
  }

  private resetCustomer(): void {
    this.selectedCustomerId = WALK_IN_CUSTOMER_ID;
    this.customerName = WALK_IN_CUSTOMER_NAME;
  }
}
