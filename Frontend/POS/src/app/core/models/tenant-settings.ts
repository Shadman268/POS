export interface TenantSettings {
  inventoryMode: number;
  promptPriceWhenUnset: boolean;
  maintainStock: boolean;
  receiptHeader?: string | null;
  receiptFooter?: string | null;
  showLineDiscount: boolean;
  showVat: boolean;
  vatPercent: number;
  shopCode: string;
  name: string;
}

export interface UpdateTenantSettings {
  name?: string;
  maintainStock?: boolean;
  promptPriceWhenUnset?: boolean;
  receiptHeader?: string | null;
  receiptFooter?: string | null;
  showLineDiscount?: boolean;
  showVat?: boolean;
  vatPercent?: number;
}
