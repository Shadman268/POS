import { Component, ElementRef, EventEmitter, OnDestroy, OnInit, Output } from '@angular/core';
import { FormControl } from '@angular/forms';
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

@Component({
  selector: 'app-journal',
  templateUrl: './journal.component.html',
  styleUrls: ['./journal.component.scss']
})
export class JournalComponent implements OnInit, OnDestroy {
  @Output() addProductRequest = new EventEmitter<void>();

  customerName = 'Walk-in Customer';
  searchControl = new FormControl('');
  filteredProducts: ProductView[] = [];
  activeIndex = -1;
  searchLoading = false;
  discountAmount = 0;
  discountUnit: LineDiscountUnit = 'BDT';
  showLineDiscount = false;
  showVat = false;
  vatPercent = 5;

  private readonly destroy$ = new Subject<void>();
  private latestSearchQuery = '';

  constructor(
    protected productService: ProductService,
    private tenantSettingsService: TenantSettingsService,
    private receiptService: ReceiptService,
    private receiptPdfService: ReceiptPdfService,
    private host: ElementRef<HTMLElement>
  ) {}

  ngOnInit(): void {
    this.tenantSettingsService.settings$.pipe(takeUntil(this.destroy$)).subscribe(settings => {
      this.showLineDiscount = settings?.showLineDiscount ?? false;
      this.showVat = settings?.showVat ?? false;
      this.vatPercent = settings?.vatPercent ?? 5;
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
        }
      }),
      filter(query => (query || '').trim().length >= 2),
      tap(() => {
        this.searchLoading = true;
      }),
      switchMap(query => this.productService.searchProducts((query || '').trim())),
      takeUntil(this.destroy$)
    ).subscribe({
      next: (results) => {
        this.searchLoading = false;
        this.filteredProducts = results;
        this.activeIndex = results.length > 0 ? 0 : -1;
        this.tryAutoAdd(this.latestSearchQuery, results);
      },
      error: () => {
        this.searchLoading = false;
        this.filteredProducts = [];
        this.activeIndex = -1;
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
        this.filteredProducts = [];
        this.activeIndex = -1;
        break;
    }
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
    if (this.productService.receiptItems.length === 0) {
      return;
    }

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
      cashReceived: total,
      changeAmount: 0,
      items: receiptItems,
      receiptHeader: settings?.receiptHeader,
      receiptFooter: settings?.receiptFooter,
      showLineDiscount: this.showLineDiscount
    };

    this.receiptService.createReceipt(receiptData).subscribe({
      next: (response) => {
        this.productService.refreshCatalog().subscribe({
          next: () => {
            this.receiptPdfService.showReceiptPreview(response || receiptData).subscribe(() => {
              this.clearReceipt();
            });
          },
          error: () => {
            this.receiptPdfService.showReceiptPreview(response || receiptData).subscribe(() => {
              this.clearReceipt();
            });
          }
        });
      },
      error: (error) => {
        const message = error.error?.message || 'Could not complete sale.';
        window.alert(message);
      }
    });
  }

  clearReceipt(): void {
    this.productService.clearReceipt();
    this.discountAmount = 0;
    this.discountUnit = 'BDT';
    this.customerName = 'Walk-in Customer';
  }
}
