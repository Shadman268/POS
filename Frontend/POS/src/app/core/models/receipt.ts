export interface ReceiptItemData {
    productId: number;
    productName: string;
    quantity: number;
    price: number;
    subtotal: number;
}

export interface ReceiptData {
    id?: number;
    customerName: string;
    total: number;
    discountUnit: string;
    discountValue: number;
    priceAfterDiscount: number;
    cashReceived: number;
    changeAmount: number;
    items: ReceiptItemData[];
    date?: Date;
}