import { Injectable } from '@angular/core';
import { ProductView } from 'src/app/core/models/product-data';
import { SignalrService } from './signalr.service';

@Injectable({
  providedIn: 'root'
})
export class ProductService {

  allProducts: ProductView[] = [];

  receiptItems: ProductView[] = [];

  constructor(private signalrService: SignalrService) {
    this.signalrService.productAdded$.subscribe((product: ProductView) => {
      this.allProducts.push(product);
    });
  }

  addProductInReceipt(product: ProductView): void {
    this.receiptItems.push(product);
  }
}
