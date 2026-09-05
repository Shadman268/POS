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
import { ReceiptService } from '../../services/receipt.service';
import { ReceiptPdfService } from '../../services/receipt-pdf.service';
import { ReceiptData, ReceiptItemData } from '../../../core/models/receipt';
import { ProductView } from '../../../core/models/product-data';

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
  vatRate = 0.05;

  private readonly destroy$ = new Subject<void>();
  private latestSearchQuery = '';

  constructor(
    protected productService: ProductService,
    private receiptService: ReceiptService,
    private receiptPdfService: ReceiptPdfService,
    private host: ElementRef<HTMLElement>
  ) {}

  ngOnInit(): void {
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
    this.clearSearch();
  }

  private clearSearch(): void {
    this.searchControl.setValue('', { emitEvent: false });
    this.latestSearchQuery = '';
    this.filteredProducts = [];
    this.activeIndex = -1;
    this.searchLoading = false;
  }

  addProductToCart(product: ProductView): void {
    this.productService.addProductInReceipt(product);
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
        this.clearSearch();
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
          this.clearSearch();
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
      (sum, item) => sum + item.price * item.quantity,
      0
    );
  }

  getDiscountValue(): number {
    return Math.min(this.discountAmount, this.getSubtotal());
  }

  getVatAmount(): number {
    const taxable = this.getSubtotal() - this.getDiscountValue();
    return Math.round(taxable * this.vatRate);
  }

  getTotal(): number {
    return this.getSubtotal() - this.getDiscountValue() + this.getVatAmount();
  }

  getLineTotal(index: number): number {
    const item = this.productService.receiptItems[index];
    return item.price * item.quantity;
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

  lineIcon(index: number): string {
    const category = (this.productService.receiptItems[index].category || '').toLowerCase();
    if (category.includes('equipment')) {
      return 'medical_services';
    }
    return 'medication';
  }

  lineAccent(index: number): string {
    const colors = ['#dbeafe', '#dcfce7', '#fef9c3', '#fee2e2', '#ede9fe'];
    return colors[index % colors.length];
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
      subtotal: item.price * item.quantity
    }));

    const receiptData: ReceiptData = {
      customerName: this.customerName,
      total: subtotal,
      discountValue: discount,
      discountUnit: 'BDT',
      priceAfterDiscount: total,
      cashReceived: total,
      changeAmount: 0,
      items: receiptItems
    };

    this.receiptService.createReceipt(receiptData).subscribe({
      next: (response) => {
        this.receiptPdfService.showReceiptPreview(response || receiptData).subscribe(() => {
          this.clearReceipt();
        });
      },
      error: (error) => console.error('Error creating receipt:', error)
    });
  }

  clearReceipt(): void {
    this.productService.clearReceipt();
    this.discountAmount = 0;
    this.customerName = 'Walk-in Customer';
  }
}
