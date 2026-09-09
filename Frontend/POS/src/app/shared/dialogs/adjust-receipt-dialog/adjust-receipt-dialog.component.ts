import { Component } from '@angular/core';
import { MatDialogRef } from '@angular/material/dialog';

@Component({
  selector: 'app-adjust-receipt-dialog',
  templateUrl: './adjust-receipt-dialog.component.html',
  styleUrls: ['./adjust-receipt-dialog.component.scss']
})
export class AdjustReceiptDialogComponent {
  receiptNo = '';

  constructor(
    public dialogRef: MatDialogRef<AdjustReceiptDialogComponent, string | undefined>
  ) {}

  onSubmit(): void {
    const value = this.receiptNo.trim();
    if (!value) {
      return;
    }

    this.dialogRef.close(value);
  }

  onCancel(): void {
    this.dialogRef.close();
  }
}
