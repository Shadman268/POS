export interface Customer {
  id: number;
  name: string;
  phone: string;
  createdAtUtc: string;
}

export interface PagedCustomerResult {
  items: Customer[];
  totalCount: number;
  page: number;
  pageSize: number;
}

export interface CreateCustomerRequest {
  name: string;
  phone: string;
}

export const WALK_IN_CUSTOMER_ID = 0;
export const WALK_IN_CUSTOMER_NAME = 'Walk-in Customer';

export const CUSTOMER_TABLE_COLUMNS: { key: keyof Customer; label: string }[] = [
  { key: 'name', label: 'Name' },
  { key: 'phone', label: 'Phone No' }
];
