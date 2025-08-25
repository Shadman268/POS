export interface ProductUpload {
    productName: string;
    price: string;
    image: File | null;
}

export interface ProductView {
    productName: string;
    price: string;
    imagePath: string;
}  