export interface Medicine {
  id: number;
  name: string;
  genericName?: string | null;
  strength?: string | null;
  dosageForm?: string | null;
  category: string;
  brand: string;
  unit: string;
  barcode?: string | null;
  isActive: boolean;
  createdAtUtc: string;
  updatedAtUtc: string;
}

export interface PagedMedicineResult {
  items: Medicine[];
  totalCount: number;
  page: number;
  pageSize: number;
}

export interface MedicineImportResult {
  inserted: number;
  updated: number;
  skipped: number;
  errors: string[];
}

export const MEDICINE_TABLE_COLUMNS: { key: keyof Medicine; label: string }[] = [
  { key: 'id', label: 'ID' },
  { key: 'name', label: 'Name' },
  { key: 'genericName', label: 'Generic Name' },
  { key: 'strength', label: 'Strength' },
  { key: 'dosageForm', label: 'Dosage Form' },
  { key: 'category', label: 'Category' },
  { key: 'brand', label: 'Brand' },
  { key: 'unit', label: 'Unit' },
  { key: 'barcode', label: 'Barcode' }
];
