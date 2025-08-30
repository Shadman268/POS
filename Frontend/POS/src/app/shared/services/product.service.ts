import { Injectable } from '@angular/core';
import { ProductView } from 'src/app/core/models/product-data';

@Injectable({
  providedIn: 'root'
})
export class ProductService {

  allProducts: ProductView[] = [];

  receiptItems: ProductView[] = [];

  constructor() { }

  addProductInReceipt(product: ProductView): void {
    this.receiptItems.push(product);
  }
}
