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
        let currentY = 20;

        doc.setFontSize(10);
        const headerLines = this.splitLines(receipt.receiptHeader);
        if (receipt.shopName?.trim()) {
            doc.setFontSize(16);
            doc.text(receipt.shopName.trim(), pageWidth / 2, currentY, { align: 'center' });
            currentY += 10;
            doc.setFontSize(10);
        }

        if (headerLines.length > 0) {
            headerLines.forEach(line => {
                doc.text(line, pageWidth / 2, currentY, { align: 'center' });
                currentY += 5;
            });
        } else if (!receipt.shopName?.trim()) {
            doc.setFontSize(20);
            doc.text('YOUR STORE NAME', pageWidth / 2, currentY, { align: 'center' });
            currentY += 10;
            doc.setFontSize(10);
            doc.text('123 Store Street, City, Country', pageWidth / 2, currentY, { align: 'center' });
            currentY += 5;
            doc.text('Phone: +123 456 7890', pageWidth / 2, currentY, { align: 'center' });
            currentY += 5;
        }

        currentY += 8;
        doc.setFontSize(12);
        const title = receipt.isAdjustment
            ? ((receipt.adjustmentDelta || 0) < 0 ? 'RETURN RECEIPT' : 'ADJUSTED RECEIPT')
            : (receipt.isReturn ? 'RETURN RECEIPT' : 'SALES RECEIPT');
        doc.text(title, pageWidth / 2, currentY, { align: 'center' });

        currentY += 10;
        doc.setFontSize(10);
        const currentDate = new Date().toLocaleString();
        doc.text(`Date: ${currentDate}`, 15, currentY);
        currentY += 5;
        doc.text(`Receipt #: ${receipt.id || 'NEW'}`, 15, currentY);
        if (receipt.originalReceiptId) {
            currentY += 5;
            doc.text(`Original Receipt #: ${receipt.originalReceiptId}`, 15, currentY);
        }
        currentY += 5;
        doc.text(`Customer: ${receipt.customerName}`, 15, currentY);

        const tableColumn = receipt.showLineDiscount
            ? ['Item', 'Qty', 'Price', 'Disc', 'Total']
            : ['Item', 'Qty', 'Price', 'Total'];

        const tableRows = receipt.items.map(item => {
            const row = [
                item.productName,
                item.quantity,
                item.price.toFixed(2),
            ];
            if (receipt.showLineDiscount) {
                row.push((item.lineDiscount || 0).toFixed(2));
            }
            row.push(item.subtotal.toFixed(2));
            return row;
        });

        autoTable(doc, {
            head: [tableColumn],
            body: tableRows,
            startY: currentY + 5,
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

        const finalY = (doc as any).lastAutoTable.finalY + 10;

        doc.setFontSize(10);
        const rightAlign = (text: string, y: number) => {
            const textWidth = doc.getStringUnitWidth(text) * doc.getFontSize() / doc.internal.scaleFactor;
            return doc.text(text, pageWidth - 15 - textWidth, y);
        };

        rightAlign(`Subtotal: ${receipt.total.toFixed(2)}`, finalY + 5);

        let totalsY = finalY + 5;

        if (receipt.discountValue > 0) {
            totalsY += 5;
            rightAlign(`Discount (${receipt.discountUnit}): ${receipt.discountValue}`, totalsY);
            totalsY += 5;
            rightAlign(`After Discount: ${receipt.priceAfterDiscount.toFixed(2)}`, totalsY);
        }

        if (receipt.isAdjustment || receipt.isReturn) {
            const delta = receipt.adjustmentDelta ?? 0;
            totalsY += 5;
            if (delta < 0 || receipt.isReturn) {
                rightAlign(`Return: ${Math.abs(delta || receipt.changeAmount || receipt.priceAfterDiscount).toFixed(2)}`, totalsY);
            } else if (delta > 0) {
                rightAlign(`Extra charge: ${delta.toFixed(2)}`, totalsY);
            }
        } else {
            totalsY += 5;
            rightAlign(`Cash Received: ${receipt.cashReceived.toFixed(2)}`, totalsY);
            totalsY += 5;
            rightAlign(`Change: ${receipt.changeAmount.toFixed(2)}`, totalsY);
        }

        totalsY += 15;
        const footerLines = this.splitLines(receipt.receiptFooter);
        if (footerLines.length > 0) {
            footerLines.forEach(line => {
                doc.text(line, pageWidth / 2, totalsY, { align: 'center' });
                totalsY += 5;
            });
        } else {
            doc.text('Thank you for shopping with us!', pageWidth / 2, totalsY, { align: 'center' });
            totalsY += 5;
            doc.text('Please come again', pageWidth / 2, totalsY, { align: 'center' });
        }

        return doc;
    }

    showReceiptPreview(receipt: ReceiptData) {
        const dialogRef = this.dialog.open(ReceiptPreviewComponent, {
            width: '600px',
            data: receipt
        });

        return dialogRef.afterClosed();
    }

    private splitLines(value?: string | null): string[] {
        if (!value?.trim()) {
            return [];
        }
        return value.split(/\r?\n/).map(line => line.trim()).filter(Boolean);
    }
}
