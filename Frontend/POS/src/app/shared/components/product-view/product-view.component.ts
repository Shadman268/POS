import { HttpClient } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { ProductView } from 'src/app/core/models/product-data';
import { ProductService } from '../../services/product.service';
import { ApiConfigService } from '../../../core/services/api-config.service';

@Component({
  selector: 'app-product-view',
  templateUrl: './product-view.component.html',
  styleUrls: ['./product-view.component.scss']
})
export class ProductViewComponent implements OnInit {
  products: ProductView[] = [];
  filteredProducts: ProductView[] = [];
  categories: string[] = ['All Category'];
  brands: string[] = ['All Brand'];
  selectedCategory = 'All Category';
  selectedBrand = 'All Brand';

  accentColors = ['#dbeafe', '#dcfce7', '#fef9c3', '#fee2e2', '#ede9fe', '#ffedd5'];

  constructor(
    private http: HttpClient,
    private productService: ProductService,
    private api: ApiConfigService
  ) {}

  ngOnInit(): void {
    this.http.get<ProductView[]>(this.api.url('Product')).subscribe(data => {
      this.products = data;
      this.productService.setProducts(data);
      this.categories = this.productService.getCategories();
      this.brands = this.productService.getBrands();
      this.applyFilter();
    });
  }

  onCategoryChange(): void {
    this.applyFilter();
  }

  onBrandChange(): void {
    this.applyFilter();
  }

  applyFilter(): void {
    this.filteredProducts = this.products.filter(p => {
      const category = p.category || 'Medicine';
      const brand = p.brand || 'General';
      const categoryMatch = this.selectedCategory === 'All Category' || category === this.selectedCategory;
      const brandMatch = this.selectedBrand === 'All Brand' || brand === this.selectedBrand;
      return categoryMatch && brandMatch;
    });
  }

  accentColor(index: number): string {
    return this.accentColors[index % this.accentColors.length];
  }

  productIcon(product: ProductView): string {
    const category = (product.category || '').toLowerCase();
    if (category.includes('equipment') || category.includes('device')) {
      return 'medical_services';
    }
    return 'medication';
  }

  selectProduct(product: ProductView): void {
    this.productService.addProductInReceipt(product);
  }
}
