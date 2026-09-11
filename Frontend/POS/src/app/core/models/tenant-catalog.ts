export type CatalogOptionType = 'DosageForm' | 'Brand' | 'Unit';

export interface TenantCatalogOptions {
  dosageForms: string[];
  brands: string[];
  units: string[];
}

export interface AddTenantCatalogOptionRequest {
  type: CatalogOptionType;
  name: string;
}
