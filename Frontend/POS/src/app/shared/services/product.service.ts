import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { Observable } from 'rxjs';
import { map, tap } from 'rxjs/operators';
import { Subject } from 'rxjs';
import {
  CartLine,
  LineDiscountUnit,
  ProductView,
  ResolvePosItemRequest,
  ResolvePosItemResponse
} from 'src/app/core/models/product-data';
import { ApiConfigService } from '../../core/services/api-config.service';
import { SignalrService } from './signalr.service';
import {
  SetPriceDialogComponent,
  SetPriceDialogData,
  SetPriceDialogResult
} from '../dialogs/set-price-dialog/set-price-dialog.component';

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
    private signalrService: SignalrService,
    private dialog: MatDialog
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

  resolveProduct(posItemId: number, price?: number, quantity = 1): Observable<ResolvePosItemResponse> {
    const body: ResolvePosItemRequest = { posItemId, quantity };
    if (price != null && price > 0) {
      body.price = price;
    }

    return this.http.post<ResolvePosItemResponse>(this.api.url('Product/resolve'), body);
  }

  setProducts(products: ProductView[]): void {
    this.allProducts = products.map(p => this.normalizeProduct(p));
  }

  addProductToCart(product: ProductView, onAdded?: () => void): void {
    const normalized = this.normalizeProduct(product);
    // Server is the source of truth for whether a tenant price already exists.
    this.resolveAndAdd(normalized, undefined, onAdded);
  }

  addProductInReceipt(product: ProductView): void {
    const normalized = this.normalizeProduct(product);
    const existing = this.findCartLine(normalized);

    if (existing) {
      existing.quantity++;
      existing.price = +normalized.price;
      existing.productId = normalized.id;
      if (normalized.medicineId != null) {
        existing.medicineId = normalized.medicineId;
      }
    } else {
      this.receiptItems.push({
        productId: normalized.id,
        medicineId: normalized.medicineId,
        productName: normalized.productName,
        price: +normalized.price,
        quantity: 1,
        lineDiscount: 0,
        lineDiscountUnit: 'BDT',
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

  updateLineDiscount(index: number, lineDiscount: number): void {
    this.receiptItems[index].lineDiscount = lineDiscount;
    this.cartChanged.next();
  }

  updateLineDiscountUnit(index: number, unit: LineDiscountUnit): void {
    this.receiptItems[index].lineDiscountUnit = unit;
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

  refreshCatalog(): Observable<ProductView[]> {
    return this.http.get<ProductView[]>(this.api.url('Product')).pipe(
      map(products => {
        this.setProducts(products);
        return this.allProducts;
      }),
      tap(() => this.cartChanged.next())
    );
  }

  private promptPriceAndAdd(product: ProductView, onAdded?: () => void): void {
    const dialogRef = this.dialog.open<SetPriceDialogComponent, SetPriceDialogData, SetPriceDialogResult>(
      SetPriceDialogComponent,
      {
        width: '420px',
        maxWidth: '95vw',
        disableClose: true,
        data: {
          productName: product.productName,
          genericName: product.genericName,
          unit: product.unit
        }
      }
    );

    dialogRef.afterClosed().subscribe(result => {
      if (result?.price && result.price > 0) {
        this.resolveAndAdd(product, result.price, onAdded);
      }
    });
  }

  private resolveAndAdd(product: ProductView, price?: number, onAdded?: () => void): void {
    this.resolveProduct(product.id, price).subscribe({
      next: (response) => {
        if (response.success && response.item) {
          const resolved = this.fromDto(response.item);
          this.updateCachedProduct(resolved);
          this.addProductInReceipt(resolved);
          onAdded?.();
          return;
        }

        if (response.requiresPrice) {
          this.promptPriceAndAdd(product, onAdded);
          return;
        }

        window.alert(response.message || 'Could not add product.');
      },
      error: (err) => {
        const body = err.error as ResolvePosItemResponse | undefined;

        if (err.status === 422 && body?.requiresPrice) {
          this.promptPriceAndAdd(product, onAdded);
          return;
        }

        window.alert(body?.message || 'Could not add product.');
      }
    });
  }

  private findCartLine(product: ProductView): CartLine | undefined {
    return this.receiptItems.find(item =>
      item.productId === product.id ||
      (product.medicineId != null && item.medicineId === product.medicineId) ||
      item.productName === product.productName
    );
  }

  private updateCachedProduct(product: ProductView): void {
    for (let i = 0; i < this.allProducts.length; i++) {
      if (this.isSameProduct(this.allProducts[i], product)) {
        this.allProducts[i] = product;
      }
    }
  }

  private isSameProduct(a: ProductView, b: ProductView): boolean {
    if (a.medicineId != null && b.medicineId != null && a.medicineId === b.medicineId) {
      return true;
    }

    if (a.id === b.id) {
      return true;
    }

    return a.productName === b.productName;
  }

  private fromDto(dto: ProductView): ProductView {
    return this.normalizeProduct({
      id: dto.id,
      medicineId: dto.medicineId,
      productName: dto.productName,
      genericName: dto.genericName,
      price: String(dto.price ?? 0),
      requiresPrice: dto.requiresPrice,
      hasTenantPrice: dto.hasTenantPrice,
      imagePath: dto.imagePath || '',
      category: dto.category,
      brand: dto.brand,
      stockQuantity: dto.stockQuantity,
      unit: dto.unit
    });
  }

  private normalizeProduct(product: ProductView): ProductView {
    const price = product.price != null ? String(product.price) : '0';
    const numericPrice = +price;
    const hasTenantPrice = product.hasTenantPrice === true
      || (product.requiresPrice === false && numericPrice > 0);

    return {
      ...product,
      price,
      hasTenantPrice,
      requiresPrice: hasTenantPrice ? false : (product.requiresPrice ?? numericPrice <= 0),
      category: product.category || 'Medicine',
      brand: product.brand || 'General',
      stockQuantity: product.stockQuantity ?? 100,
      unit: product.unit || 'Tablet',
      imagePath: product.imagePath || ''
    };
  }
}
