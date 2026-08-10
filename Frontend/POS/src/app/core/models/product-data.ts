export interface ProductUpload {
    productName: string;
    price: string;
    image: File | null;
}

export interface ProductView {
    id: number;
    productName: string;
    genericName?: string;
    price: string;
    imagePath: string;
    category?: string;
    brand?: string;
    stockQuantity?: number;
    unit?: string;
}

export interface CartLine {
    productId: number;
    productName: string;
    price: number;
    quantity: number;
    unit: string;
    imagePath?: string;
    category?: string;
}
