export interface TenantProduct {
  id: number;
  medicineId: number;
  name: string;
  genericName?: string | null;
  strength?: string | null;
  dosageForm?: string | null;
  category: string;
  brand: string;
  unit: string;
  barcode?: string | null;
  sellingPrice?: number | null;
  costPrice?: number | null;
  isStockTracked: boolean;
  stockQuantity: number;
  localSku?: string | null;
}

export interface PagedTenantProductResult {
  items: TenantProduct[];
  totalCount: number;
  page: number;
  pageSize: number;
}

export interface UpdateTenantProductSettings {
  sellingPrice?: number | null;
  costPrice?: number | null;
  isStockTracked: boolean;
  stockQuantity: number;
}

export interface CreateTenantProductRequest {
  name: string;
  genericName?: string | null;
  strength?: string | null;
  dosageForm?: string | null;
  category: string;
  brand: string;
  unit: string;
  barcode?: string | null;
  sellingPrice: number;
  costPrice?: number | null;
  isStockTracked: boolean;
  localSku?: string | null;
  initialStock: number;
  batchNumber?: string | null;
  expiryDate?: string | null;
}

export const TENANT_PRODUCT_READONLY_COLUMNS: { key: keyof TenantProduct; label: string }[] = [
  { key: 'name', label: 'Name' },
  { key: 'genericName', label: 'Generic Name' },
  { key: 'strength', label: 'Strength' },
  { key: 'dosageForm', label: 'Dosage Form' },
  { key: 'category', label: 'Category' },
  { key: 'brand', label: 'Brand' },
  { key: 'unit', label: 'Unit' },
  { key: 'barcode', label: 'Barcode' }
];
