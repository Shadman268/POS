import { Injectable } from '@angular/core';
import jsPDF from 'jspdf';
import autoTable from 'jspdf-autotable';
import { ReceiptData } from '../../core/models/receipt';
import { MatDialog } from '@angular/material/dialog';
import { ReceiptPreviewComponent } from '../components/receipt-preview/receipt-preview.component';

@Injectable({
    providedIn: 'root'
})
export class ReceiptPdfService {
    constructor(private dialog: MatDialog) { }

    generateReceiptPdf(receipt: ReceiptData): jsPDF {
        const doc = new jsPDF();
        const pageWidth = doc.internal.pageSize.width;

        // Store Logo/Header
        doc.setFontSize(20);
        doc.text('YOUR STORE NAME', pageWidth / 2, 20, { align: 'center' });

        doc.setFontSize(10);
        doc.text('123 Store Street, City, Country', pageWidth / 2, 30, { align: 'center' });
        doc.text('Phone: +123 456 7890', pageWidth / 2, 35, { align: 'center' });

        // Receipt Details
        doc.setFontSize(12);
        doc.text('SALES RECEIPT', pageWidth / 2, 45, { align: 'center' });

        // Receipt Info
        doc.setFontSize(10);
        const currentDate = new Date().toLocaleString();
        doc.text(`Date: ${currentDate}`, 15, 55);
        doc.text(`Receipt #: ${receipt.id || 'NEW'}`, 15, 60);
        doc.text(`Customer: ${receipt.customerName}`, 15, 65);

        // Items Table
        const tableColumn = ['Item', 'Qty', 'Price', 'Total'];
        const tableRows = receipt.items.map(item => [
            item.productName,
            item.quantity,
            item.price.toFixed(2),
            item.subtotal.toFixed(2)
        ]);

        // Use autoTable function instead of doc.autoTable
        autoTable(doc, {
            head: [tableColumn],
            body: tableRows,
            startY: 75,
            theme: 'grid',
            headStyles: { fillColor: [66, 66, 66] },
            styles: {
                fontSize: 8,
                cellPadding: 2,
                halign: 'right'
            },
            columnStyles: {
                0: { halign: 'left' },
                1: { halign: 'center' }
            }
        });

        // Get the y position after the table
        const finalY = (doc as any).lastAutoTable.finalY + 10;

        // Totals section
        doc.setFontSize(10);
        const rightAlign = (text: string, y: number) => {
            const textWidth = doc.getStringUnitWidth(text) * doc.getFontSize() / doc.internal.scaleFactor;
            return doc.text(text, pageWidth - 15 - textWidth, y);
        };

        rightAlign(`Subtotal: ${receipt.total.toFixed(2)}`, finalY + 5);

        let currentY = finalY + 5;

        if (receipt.discountValue > 0) {
            currentY += 5;
            rightAlign(`Discount (${receipt.discountUnit}): ${receipt.discountValue}`, currentY);
            currentY += 5;
            rightAlign(`After Discount: ${receipt.priceAfterDiscount.toFixed(2)}`, currentY);
        }

        currentY += 5;
        rightAlign(`Cash Received: ${receipt.cashReceived.toFixed(2)}`, currentY);
        currentY += 5;
        rightAlign(`Change: ${receipt.changeAmount.toFixed(2)}`, currentY);

        // Footer
        currentY += 15;
        doc.setFontSize(10);
        doc.text('Thank you for shopping with us!', pageWidth / 2, currentY, { align: 'center' });
        currentY += 5;
        doc.text('Please come again', pageWidth / 2, currentY, { align: 'center' });

        return doc;
    }

    showReceiptPreview(receipt: ReceiptData) {
        const dialogRef = this.dialog.open(ReceiptPreviewComponent, {
            width: '600px',
            data: receipt
        });

        return dialogRef.afterClosed();
    }
}