import { Component, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { ReceiptData } from '../../../core/models/receipt';
import { ReceiptPdfService } from '../../services/receipt-pdf.service';

@Component({
    selector: 'app-receipt-preview',
    templateUrl: './receipt-preview.component.html',
    styleUrls: ['./receipt-preview.component.scss']
})
export class ReceiptPreviewComponent {
    currentDate = new Date();

    constructor(
        public dialogRef: MatDialogRef<ReceiptPreviewComponent>,
        @Inject(MAT_DIALOG_DATA) public receipt: ReceiptData,
        private receiptPdfService: ReceiptPdfService
    ) { }

    downloadPdf(): void {
        const doc = this.receiptPdfService.generateReceiptPdf(this.receipt);
        const timestamp = new Date().getTime();
        const filename = `receipt - ${this.receipt.id || timestamp}.pdf`;
        doc.save(filename);

        this.dialogRef.close();
    }
}