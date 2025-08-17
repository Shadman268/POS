import { Component } from '@angular/core';
import { MatDialogRef } from '@angular/material/dialog';
import { ProductData } from 'src/app/core/models/product-data';

@Component({
  selector: 'app-add-product-dialog',
  templateUrl: './add-product-dialog.component.html',
  styleUrls: ['./add-product-dialog.component.scss']
})
export class AddProductDialogComponent {

  productName: string = '';
  price: string = '';
  image: File | null = null;

  constructor(public dialogRef: MatDialogRef<AddProductDialogComponent>) { }

  onImageSelected(event: any) {
    const file = event.target.files[0];
    if (file) {
      this.image = file;
    }
  }

  onSubmit() {
    const productData: ProductData = {
      productName: this.productName,
      price: this.price,
      image: this.image
    };
    this.dialogRef.close(productData);
  }

  onCancel() {
    this.dialogRef.close();
  }
}
