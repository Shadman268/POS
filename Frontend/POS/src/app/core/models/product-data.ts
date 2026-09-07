export interface ProductUpload {
    productName: string;
    price: string;
    image: File | null;
}

export interface ProductView {
    id: number;
    medicineId?: number;
    productName: string;
    genericName?: string;
    price: string;
    requiresPrice?: boolean;
    hasTenantPrice?: boolean;
    imagePath: string;
    category?: string;
    brand?: string;
    stockQuantity?: number;
    unit?: string;
}

export interface ResolvePosItemRequest {
    posItemId: number;
    price?: number | null;
    quantity?: number;
    medicineBatchId?: number | null;
}

export interface ResolvePosItemResponse {
    success: boolean;
    requiresPrice?: boolean;
    message?: string;
    item?: ProductView;
}

export type LineDiscountUnit = 'BDT' | '%';

export interface CartLine {
    productId: number;
    medicineId?: number;
    productName: string;
    price: number;
    quantity: number;
    unit: string;
    lineDiscount?: number;
    lineDiscountUnit?: LineDiscountUnit;
    imagePath?: string;
    category?: string;
}
