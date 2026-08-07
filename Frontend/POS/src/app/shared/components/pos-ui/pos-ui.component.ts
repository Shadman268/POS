import { Component, OnInit, OnDestroy } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { HttpClient } from '@angular/common/http';
import { Subscription } from 'rxjs';
import { AddProductDialogComponent } from '../../dialogs/add-product-dialog/add-product-dialog.component';
import { ProductUpload, ProductView } from 'src/app/core/models/product-data';
import { ProductService } from '../../services/product.service';
import { ApiConfigService } from '../../../core/services/api-config.service';

@Component({
  selector: 'app-pos-ui',
  templateUrl: './pos-ui.component.html',
  styleUrls: ['./pos-ui.component.scss']
})
export class PosUiComponent implements OnInit, OnDestroy {
  mobileView: 'products' | 'journal' = 'products';
  cartCount = 0;
  private cartSub?: Subscription;

  constructor(
    private dialog: MatDialog,
    private http: HttpClient,
    private productService: ProductService,
    private api: ApiConfigService
  ) {}

  ngOnInit(): void {
    this.updateCartCount();
    this.cartSub = this.productService.cartChanged$.subscribe(() => {
      this.updateCartCount();
      if (this.cartCount > 0 && window.innerWidth <= 768) {
        this.mobileView = 'journal';
      }
    });
  }

  ngOnDestroy(): void {
    this.cartSub?.unsubscribe();
  }

  addProduct(): void {
    const dialogRef = this.dialog.open(AddProductDialogComponent, {
      width: '400px',
      maxWidth: '95vw'
    });

    dialogRef.afterClosed().subscribe((result: ProductUpload) => {
      if (result) {
        const formData = new FormData();
        formData.append('productName', result.productName);
        formData.append('price', result.price.toString());
        if (result.image) {
          formData.append('image', result.image);
        }

        this.http.post<ProductView>(this.api.url('Product'), formData).subscribe({
          error: (err) => console.error('Error creating product:', err)
        });
      }
    });
  }

  private updateCartCount(): void {
    this.cartCount = this.productService.receiptItems.reduce((sum, item) => sum + item.quantity, 0);
  }
}
