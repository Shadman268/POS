import { Component, ElementRef, OnInit, ViewChild } from '@angular/core';
import { PriceUnit } from 'src/app/core/models/price-unit';
import { FormControl } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { ProductService } from '../../services/product.service';
import { ReceiptService } from '../../services/receipt.service';
import { ReceiptData, ReceiptItemData } from '../../../core/models/receipt';

@Component({
  selector: 'app-journal',
  templateUrl: './journal.component.html',
  styleUrls: ['./journal.component.scss']
})

export class JournalComponent implements OnInit {

  @ViewChild('discountInput') discountInput!: ElementRef;
  @ViewChild('received') cashReceived!: ElementRef;

  applyDiscount: boolean = false;

  priceUnit: PriceUnit[] = [
    { amount: 'percentage', viewAmount: '%' },
    { amount: 'BDT', viewAmount: 'BDT' }
  ]

  selectedUnit = this.priceUnit[0].amount;

  priceAfterDiscount: number = 0;
  changeAmount: number = 0;

  searchControl = new FormControl();
  allItems: string[] = ['pops', 'potato', 'pepe', 'papor', 'ponir', 'poppa', 'popsa'];
  filteredItems: string[] = [];
  selectedItems: string[] = [];

  constructor(private http: HttpClient, protected productService: ProductService, private receiptService: ReceiptService) {
    this.filteredItems = [];
  }

  ngOnInit(): void {
    this.http.get("http://localhost:5003/api/product")
      .subscribe(res => console.log(res));
  }

  filterItems(): void {
    const searchText = this.searchControl.value?.toLowerCase() || '';
    if (searchText.trim() === '') {
      this.filteredItems = [];
    } else {
      this.filteredItems = this.allItems.filter(item =>
        item.toLowerCase().includes(searchText)
      );
    }
  }

  onDiscountCheckboxChange(): void {
    if (this.applyDiscount) {
      setTimeout(() => {
        this.discountInput.nativeElement.focus();
      });
    }
  }

  getTotal(): number {
    let total = 0;
    this.productService.receiptItems.forEach(item => {
      total = total + +item.price;
    })
    return total;
  }

  updatePrice(discount: number, unit: string): void {
    const total = this.getTotal();
    if (unit === 'BDT') {
      this.priceAfterDiscount = this.priceInBdtDiscount(total, discount);
    }
    else {
      this.priceAfterDiscount = this.priceInPercentageDiscount(total, discount);
    }
  }

  priceInPercentageDiscount(total: number, discount: number): number {
    return total - (total * (discount / 100));
  }

  priceInBdtDiscount(total: number, discount: number): number {
    return total - discount;
  }

  updateChangeAmount(received: number, discount?: number, unit?: string): void {
    if (this.applyDiscount) {
      this.changeAmount = received - this.priceAfterDiscount;
    } else {
      this.changeAmount = received - this.getTotal();
    }
  }

  processPayment(): void {
    const customerName = 'Walk-in Customer';

    // Convert receipt items to the format expected by backend
    const receiptItems: ReceiptItemData[] = this.productService.receiptItems.map(item => ({
      productId: item.id || 0, // Assuming your ProductService items have productId
      productName: item.productName,
      quantity: 1, // Currently hardcoded to 1
      price: +item.price,
      subtotal: +item.price
    }));

    const total = this.getTotal();
    const finalPrice = this.applyDiscount ? this.priceAfterDiscount : total;

    const receiptData: ReceiptData = {
      customerName: customerName,
      total: total,
      discountValue: this.applyDiscount ? +(this.discountInput?.nativeElement?.value || 0) : 0,
      discountUnit: this.applyDiscount ? this.selectedUnit : '',
      priceAfterDiscount: finalPrice,
      cashReceived: +(this.cashReceived?.nativeElement?.value || 0),
      changeAmount: this.changeAmount,
      items: receiptItems
    };

    this.receiptService.createReceipt(receiptData).subscribe({
      next: (response) => {
        console.log('Receipt created successfully:', response);
        this.clearReceipt();
      },
      error: (error) => {
        console.error('Error creating receipt:', error);
      }
    });
  }

  clearReceipt(): void {
    this.productService.receiptItems = [];
    this.applyDiscount = false;
    this.priceAfterDiscount = 0;
    this.changeAmount = 0;

    if (this.discountInput) this.discountInput.nativeElement.value = '';
    if (this.cashReceived) this.cashReceived.nativeElement.value = '';
  }

}
