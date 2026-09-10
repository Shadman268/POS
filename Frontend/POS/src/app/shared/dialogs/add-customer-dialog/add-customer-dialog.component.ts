import { Component } from '@angular/core';
import { MatDialogRef } from '@angular/material/dialog';
import { CreateCustomerRequest } from 'src/app/core/models/customer';

@Component({
  selector: 'app-add-customer-dialog',
  templateUrl: './add-customer-dialog.component.html',
  styleUrls: ['./add-customer-dialog.component.scss']
})
export class AddCustomerDialogComponent {
  form: CreateCustomerRequest = {
    name: '',
    phone: ''
  };

  constructor(public dialogRef: MatDialogRef<AddCustomerDialogComponent, CreateCustomerRequest | undefined>) {}

  onSubmit(): void {
    const name = this.form.name.trim();
    const phone = this.form.phone.trim();

    if (!name || !phone) {
      return;
    }

    this.dialogRef.close({ name, phone });
  }

  onCancel(): void {
    this.dialogRef.close();
  }
}
