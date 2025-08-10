import { Component } from '@angular/core';
import { MatDialogRef } from '@angular/material/dialog';

@Component({
  selector: 'app-add-product-dialog',
  templateUrl: './add-product-dialog.component.html',
  styleUrls: ['./add-product-dialog.component.scss']
})
export class AddProductDialogComponent  {

  productName: string = '';
  price: number | null = null;
  image: File | null = null;

  constructor(public dialogRef: MatDialogRef<AddProductDialogComponent>) {}

  onImageSelected(event: any) {
    const file = event.target.files[0];
    if (file) {
      this.image = file;
    }
  }

onSubmit() {
    const productData = {
      name: this.productName,
      price: this.price,
      image: this.image
    };
    this.dialogRef.close(productData);
  }

  onCancel() {
    this.dialogRef.close();
  }
}
