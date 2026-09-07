export interface ReceiptItemData {
    productId: number;
    productName: string;
    quantity: number;
    price: number;
    subtotal: number;
    lineDiscount?: number;
}

export interface ReceiptData {
    id?: number;
    customerName: string;
    shopName?: string | null;
    total: number;
    discountUnit: string;
    discountValue: number;
    priceAfterDiscount: number;
    cashReceived: number;
    changeAmount: number;
    items: ReceiptItemData[];
    date?: Date;
    receiptHeader?: string | null;
    receiptFooter?: string | null;
    showLineDiscount?: boolean;
}