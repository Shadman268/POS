import { Injectable } from '@angular/core';
import { Subject } from 'rxjs';
import { CartLine, ProductView } from 'src/app/core/models/product-data';
import { SignalrService } from './signalr.service';

@Injectable({
  providedIn: 'root'
})
export class ProductService {

  allProducts: ProductView[] = [];
  receiptItems: CartLine[] = [];
  private cartChanged = new Subject<void>();
  cartChanged$ = this.cartChanged.asObservable();

  constructor(private signalrService: SignalrService) {
    this.signalrService.productAdded$.subscribe((product: ProductView) => {
      this.allProducts.push(this.normalizeProduct(product));
    });
  }

  setProducts(products: ProductView[]): void {
    this.allProducts = products.map(p => this.normalizeProduct(p));
  }

  addProductInReceipt(product: ProductView): void {
    const normalized = this.normalizeProduct(product);
    const existing = this.receiptItems.find(item => item.productId === normalized.id);
    if (existing) {
      existing.quantity++;
    } else {
      this.receiptItems.push({
        productId: normalized.id,
        productName: normalized.productName,
        price: +normalized.price,
        quantity: 1,
        unit: normalized.unit || 'Unit',
        imagePath: normalized.imagePath,
        category: normalized.category
      });
    }
    this.cartChanged.next();
  }

  updateQuantity(index: number, quantity: number): void {
    if (quantity < 1) {
      return;
    }
    this.receiptItems[index].quantity = quantity;
    this.cartChanged.next();
  }

  removeFromReceipt(index: number): void {
    this.receiptItems.splice(index, 1);
    this.cartChanged.next();
  }

  clearReceipt(): void {
    this.receiptItems = [];
    this.cartChanged.next();
  }

  getCategories(): string[] {
    const values = this.allProducts.map(p => p.category || 'General');
    return ['All Category', ...Array.from(new Set(values)).sort()];
  }

  getBrands(): string[] {
    const values = this.allProducts.map(p => p.brand || 'General');
    return ['All Brand', ...Array.from(new Set(values)).sort()];
  }

  private normalizeProduct(product: ProductView): ProductView {
    return {
      ...product,
      category: product.category || 'Medicine',
      brand: product.brand || 'General',
      stockQuantity: product.stockQuantity ?? 100,
      unit: product.unit || 'Tablet'
    };
  }
}
