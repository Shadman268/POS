export interface ReceiptItemData {
    id?: number;
    productId: number;
    productName: string;
    quantity: number;
    price: number;
    subtotal: number;
    lineDiscount?: number;
    soldQuantity?: number;
    alreadyReturnedQuantity?: number;
    returnableQuantity?: number;
}

export interface ReceiptData {
    id?: number;
    receiptType?: number;
    originalReceiptId?: number | null;
    createdAt?: string | Date;
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
    isReturn?: boolean;
    isAdjustment?: boolean;
    adjustmentDelta?: number;
}

export const ReceiptTypeSale = 0;
export const ReceiptTypeReturn = 1;
export const ReceiptTypeAdjustment = 2;