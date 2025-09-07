import { Component, ElementRef, OnInit, ViewChild } from '@angular/core';
import { PriceUnit } from 'src/app/core/models/price-unit';
import { FormControl } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { ProductService } from '../../services/product.service';

@Component({
  selector: 'app-journal',
  templateUrl: './journal.component.html',
  styleUrls: ['./journal.component.scss']
})

export class JournalComponent implements OnInit {

  @ViewChild('discountInput') discountInput!: ElementRef;

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

  constructor(private http: HttpClient, protected productService: ProductService) {
    this.filteredItems = [];
  }

  ngOnInit(): void {
    this.http.get("http://localhost:5000/api/product")
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

}
