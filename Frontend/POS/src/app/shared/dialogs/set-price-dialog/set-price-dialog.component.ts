import { Component, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';

export interface SetPriceDialogData {
  productName: string;
  genericName?: string | null;
  unit?: string;
}

export interface SetPriceDialogResult {
  price: number;
}

@Component({
  selector: 'app-set-price-dialog',
  templateUrl: './set-price-dialog.component.html',
  styleUrls: ['./set-price-dialog.component.scss']
})
export class SetPriceDialogComponent {
  price: number | null = null;

  constructor(
    public dialogRef: MatDialogRef<SetPriceDialogComponent, SetPriceDialogResult | undefined>,
    @Inject(MAT_DIALOG_DATA) public data: SetPriceDialogData
  ) {}

  onSubmit(): void {
    if (this.price == null || this.price <= 0) {
      return;
    }

    this.dialogRef.close({ price: this.price });
  }

  onCancel(): void {
    this.dialogRef.close();
  }
}
