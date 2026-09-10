import { Component, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { formatAmount } from '../../pipes/amount.pipe';

export interface ChargeDialogData {
  total: number;
}

export interface ChargeDialogResult {
  cashReceived: number;
  changeAmount: number;
}

@Component({
  selector: 'app-charge-dialog',
  templateUrl: './charge-dialog.component.html',
  styleUrls: ['./charge-dialog.component.scss']
})
export class ChargeDialogComponent {
  readonly notes = [10, 20, 50, 100, 200, 500, 1000];
  cashReceived: number | null = null;

  constructor(
    public dialogRef: MatDialogRef<ChargeDialogComponent, ChargeDialogResult | undefined>,
    @Inject(MAT_DIALOG_DATA) public data: ChargeDialogData
  ) {}

  get changeAmount(): number {
    const cash = this.cashReceived ?? 0;
    return Math.max(0, this.round(cash - this.data.total));
  }

  get canProcess(): boolean {
    return (this.cashReceived ?? 0) >= this.data.total;
  }

  addNote(amount: number): void {
    this.cashReceived = this.round((this.cashReceived ?? 0) + amount);
  }

  setExact(): void {
    this.cashReceived = this.round(this.data.total);
  }

  format(value: number): string {
    return formatAmount(value);
  }

  onProcess(): void {
    if (!this.canProcess) {
      return;
    }

    this.dialogRef.close({
      cashReceived: this.round(this.cashReceived ?? 0),
      changeAmount: this.changeAmount
    });
  }

  onCancel(): void {
    this.dialogRef.close();
  }

  private round(value: number): number {
    return Math.round((value + Number.EPSILON) * 10) / 10;
  }
}
