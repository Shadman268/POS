import { HttpClient } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { ProductView } from 'src/app/core/models/product-data';
import { ProductService } from '../../services/product.service';
import { TenantSettingsService } from '../../services/tenant-settings.service';
import { ApiConfigService } from '../../../core/services/api-config.service';

@Component({
  selector: 'app-product-view',
  templateUrl: './product-view.component.html',
  styleUrls: ['./product-view.component.scss']
})
export class ProductViewComponent implements OnInit {
  filteredProducts: ProductView[] = [];
  brands: string[] = ['All Brand'];
  selectedBrand = 'All Brand';
  maintainStock = true;

  constructor(
    private http: HttpClient,
    private productService: ProductService,
    private tenantSettingsService: TenantSettingsService,
    private api: ApiConfigService
  ) {}

  ngOnInit(): void {
    this.tenantSettingsService.settings$.subscribe(settings => {
      this.maintainStock = settings?.maintainStock ?? true;
    });

    this.http.get<ProductView[]>(this.api.url('Product')).subscribe(data => {
      this.productService.setProducts(data);
      this.brands = this.productService.getBrands();
      this.applyFilter();
    });

    this.productService.cartChanged$.subscribe(() => {
      this.brands = this.productService.getBrands();
      this.applyFilter();
    });
  }

  onBrandChange(): void {
    this.applyFilter();
  }

  applyFilter(): void {
    this.filteredProducts = this.productService.allProducts.filter(p => {
      const brand = p.brand || 'General';
      return this.selectedBrand === 'All Brand' || brand === this.selectedBrand;
    });
  }

  needsPrice(product: ProductView): boolean {
    return !product.hasTenantPrice && (product.requiresPrice === true || +product.price <= 0);
  }

  stockLevel(stock: number): 'good' | 'low' | 'out' {
    if (stock <= 0) {
      return 'out';
    }
    if (stock <= 10) {
      return 'low';
    }
    return 'good';
  }

  selectProduct(product: ProductView): void {
    this.productService.addProductToCart(product);
  }
}
