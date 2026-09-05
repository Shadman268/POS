import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { Subject } from 'rxjs';
import { CartLine, ProductView } from 'src/app/core/models/product-data';
import { ApiConfigService } from '../../core/services/api-config.service';
import { SignalrService } from './signalr.service';

@Injectable({
  providedIn: 'root'
})
export class ProductService {

  allProducts: ProductView[] = [];
  receiptItems: CartLine[] = [];
  private cartChanged = new Subject<void>();
  cartChanged$ = this.cartChanged.asObservable();

  constructor(
    private http: HttpClient,
    private api: ApiConfigService,
    private signalrService: SignalrService
  ) {
    this.signalrService.productAdded$.subscribe((product: ProductView) => {
      this.allProducts.push(this.normalizeProduct(product));
    });
  }

  searchProducts(query: string): Observable<ProductView[]> {
    const params = new HttpParams().set('search', query.trim());
    return this.http.get<ProductView[]>(this.api.url('Product'), { params }).pipe(
      map(products => products.map(p => this.normalizeProduct(p)))
    );
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
      price: product.price != null ? String(product.price) : '0',
      category: product.category || 'Medicine',
      brand: product.brand || 'General',
      stockQuantity: product.stockQuantity ?? 100,
      unit: product.unit || 'Tablet'
    };
  }
}
