import { HttpClient } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { ProductView } from 'src/app/core/models/product-data';
import { ProductService } from '../../services/product.service';

@Component({
  selector: 'app-product-view',
  templateUrl: './product-view.component.html',
  styleUrls: ['./product-view.component.scss']
})
export class ProductViewComponent implements OnInit {

  public products: ProductView[] = [];

  constructor(
    private http: HttpClient,
    protected productService: ProductService
  ) { }

  ngOnInit(): void {
    // Load initial products
    this.http.get<ProductView[]>("http://localhost:5003/api/Product")
      .subscribe(data => {
        this.products = data;
        this.productService.allProducts = [...this.products];
      });
  }

  selectProduct(product: ProductView): void {
    this.productService.addProductInReceipt(product);
  }
}
