export interface ProductUpload {
    productName: string;
    price: string;
    image: File | null;
}

export interface ProductView {
    id: number,
    productName: string;
    price: string;
    imagePath: string;
}  