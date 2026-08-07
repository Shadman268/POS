import { Component, EventEmitter, OnInit, Output } from '@angular/core';
import { FormControl } from '@angular/forms';
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
export class JournalComponent implements OnInit {
  @Output() addProductRequest = new EventEmitter<void>();

  customerName = 'Walk-in Customer';
  searchControl = new FormControl('');
  filteredProducts: ProductView[] = [];
  discountAmount = 0;
  vatRate = 0.05;

  constructor(
    protected productService: ProductService,
    private receiptService: ReceiptService,
    private receiptPdfService: ReceiptPdfService
  ) {}

  ngOnInit(): void {
    this.searchControl.valueChanges.subscribe(value => {
      this.onSearchChange(value || '');
    });
  }

  onSearchChange(searchText: string): void {
    const query = searchText.toLowerCase().trim();
    if (!query) {
      this.filteredProducts = [];
      return;
    }

    this.filteredProducts = this.productService.allProducts.filter(p =>
      p.productName.toLowerCase().includes(query)
    );

    const exactMatch = this.productService.allProducts.find(
      p => p.productName.toLowerCase() === query
    );
    if (exactMatch) {
      this.addProductToCart(exactMatch);
      this.searchControl.setValue('', { emitEvent: false });
      this.filteredProducts = [];
    }
  }

  selectAutocomplete(product: ProductView): void {
    this.addProductToCart(product);
    this.searchControl.setValue('', { emitEvent: false });
    this.filteredProducts = [];
  }

  addProductToCart(product: ProductView): void {
    this.productService.addProductInReceipt(product);
  }

  addFromSearchButton(): void {
    const query = (this.searchControl.value || '').toLowerCase().trim();
    if (!query) {
      this.addProductRequest.emit();
      return;
    }
    const product = this.productService.allProducts.find(p =>
      p.productName.toLowerCase().includes(query)
    );
    if (product) {
      this.addProductToCart(product);
      this.searchControl.setValue('', { emitEvent: false });
      this.filteredProducts = [];
    } else {
      this.addProductRequest.emit();
    }
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
