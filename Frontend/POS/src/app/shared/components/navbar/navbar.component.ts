import { Component, OnInit } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { AddProductDialogComponent } from '../../dialogs/add-product-dialog/add-product-dialog.component';
import { ProductUpload, ProductView } from 'src/app/core/models/product-data';
import { HttpClient } from '@angular/common/http';
import { ProductService } from '../../services/product.service';

@Component({
  selector: 'app-navbar',
  templateUrl: './navbar.component.html',
  styleUrls: ['./navbar.component.scss']
})
export class NavbarComponent implements OnInit {

  constructor(private dialog: MatDialog, private http: HttpClient, private productService: ProductService) { }

  ngOnInit(): void {
  }

  addProduct() {
    const dialogRef = this.dialog.open(AddProductDialogComponent, {
      width: '400px',
      height: '330px'
    });

    dialogRef.afterClosed().subscribe((result: ProductUpload) => {
      if (result) {
        console.log('Product Data:', result);
        const formData = new FormData();
        formData.append("productName", result.productName);
        formData.append("price", result.price.toString());
        if (result.image) {
          formData.append("image", result.image); // actual File object
        }

        this.http.post<ProductView>("http://localhost:5000/api/Product", formData)
          .subscribe({
            next: (res: ProductView) => {
              this.productService.allProducts.push(res);
            },
            error: (err) => {
              console.error("Error creating product:", err);
            }
          });
      }
    });
  }

}
