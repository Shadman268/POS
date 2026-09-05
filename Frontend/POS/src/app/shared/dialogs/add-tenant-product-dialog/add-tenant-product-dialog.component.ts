import { Component } from '@angular/core';
import { MatDialogRef } from '@angular/material/dialog';
import { CreateTenantProductRequest } from 'src/app/core/models/tenant-product';

@Component({
  selector: 'app-add-tenant-product-dialog',
  templateUrl: './add-tenant-product-dialog.component.html',
  styleUrls: ['./add-tenant-product-dialog.component.scss']
})
export class AddTenantProductDialogComponent {
  form: CreateTenantProductRequest = {
    name: '',
    genericName: '',
    strength: '',
    dosageForm: '',
    category: 'Medicine',
    brand: 'General',
    unit: 'Tablet',
    barcode: '',
    sellingPrice: 0,
    costPrice: null,
    isStockTracked: true,
    localSku: '',
    initialStock: 0,
    batchNumber: '',
    expiryDate: null
  };

  saving = false;
  error = '';

  constructor(public dialogRef: MatDialogRef<AddTenantProductDialogComponent>) {}

  onSubmit(): void {
    if (!this.form.name.trim() || this.form.sellingPrice <= 0) {
      return;
    }

    this.dialogRef.close({
      ...this.form,
      name: this.form.name.trim(),
      genericName: this.form.genericName?.trim() || null,
      strength: this.form.strength?.trim() || null,
      dosageForm: this.form.dosageForm?.trim() || null,
      barcode: this.form.barcode?.trim() || null,
      localSku: this.form.localSku?.trim() || null,
      batchNumber: this.form.batchNumber?.trim() || null,
      expiryDate: this.form.expiryDate || null
    } as CreateTenantProductRequest);
  }

  onCancel(): void {
    this.dialogRef.close();
  }
}
