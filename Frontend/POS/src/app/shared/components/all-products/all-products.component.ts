import { Component, OnInit } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import {
  CreateTenantProductRequest,
  TENANT_PRODUCT_READONLY_COLUMNS,
  TenantProduct,
  UpdateTenantProductSettings
} from '../../../core/models/tenant-product';
import { AddTenantProductDialogComponent } from '../../dialogs/add-tenant-product-dialog/add-tenant-product-dialog.component';
import { TenantMedicineService } from '../../services/tenant-medicine.service';
import { TenantSettingsService } from '../../services/tenant-settings.service';

interface ProductEditableSnapshot {
  costPrice: number | null;
  sellingPrice: number | null;
  stockQuantity: number;
  isStockTracked: boolean;
}

@Component({
  selector: 'app-all-products',
  templateUrl: './all-products.component.html',
  styleUrls: ['./all-products.component.scss']
})
export class AllProductsComponent implements OnInit {
  readonlyColumns = TENANT_PRODUCT_READONLY_COLUMNS;
  products: TenantProduct[] = [];
  totalCount = 0;
  page = 1;
  pageSize = 50;
  search = '';
  loading = false;
  saving = false;
  savingMedicineId: number | null = null;
  resettingMedicineId: number | null = null;
  error = '';
  successMessage = '';
  maintainStock = true;
  private savedSnapshots = new Map<number, ProductEditableSnapshot>();

  readonly pageSizeOptions = [25, 50, 100, 200];

  constructor(
    private tenantMedicineService: TenantMedicineService,
    private tenantSettingsService: TenantSettingsService,
    private dialog: MatDialog
  ) {}

  ngOnInit(): void {
    this.tenantSettingsService.settings$.subscribe(settings => {
      this.maintainStock = settings?.maintainStock ?? true;
    });
    this.loadProducts();
  }

  get totalPages(): number {
    return Math.max(1, Math.ceil(this.totalCount / this.pageSize));
  }

  get hasData(): boolean {
    return this.totalCount > 0;
  }

  openAddProductDialog(): void {
    const dialogRef = this.dialog.open(AddTenantProductDialogComponent, {
      width: '720px',
      maxWidth: '95vw'
    });

    dialogRef.afterClosed().subscribe((request: CreateTenantProductRequest | undefined) => {
      if (!request) {
        return;
      }

      this.saving = true;
      this.error = '';

      this.tenantMedicineService.createProduct(request).subscribe({
        next: () => {
          this.saving = false;
          this.page = 1;
          this.loadProducts();
        },
        error: (err) => {
          this.saving = false;
          this.error = err.error?.message || 'Failed to add product.';
        }
      });
    });
  }

  loadProducts(): void {
    this.loading = true;
    this.tenantMedicineService.getProducts(this.page, this.pageSize, this.search).subscribe({
      next: (result) => {
        this.products = result.items.map(item => ({
          ...item,
          sellingPrice: item.sellingPrice ?? null,
          costPrice: item.costPrice ?? null,
          stockQuantity: item.stockQuantity ?? 0,
          isStockTracked: item.isStockTracked ?? false
        }));
        this.syncSavedSnapshots(this.products);
        this.totalCount = result.totalCount;
        this.page = result.page;
        this.pageSize = result.pageSize;
        this.loading = false;
      },
      error: () => {
        this.products = [];
        this.totalCount = 0;
        this.loading = false;
      }
    });
  }

  onSearch(): void {
    this.page = 1;
    this.loadProducts();
  }

  onPageSizeChange(): void {
    this.page = 1;
    this.loadProducts();
  }

  goToPage(page: number): void {
    if (page < 1 || page > this.totalPages) {
      return;
    }
    this.page = page;
    this.loadProducts();
  }

  displayValue(product: TenantProduct, key: keyof TenantProduct): string {
    const value = product[key];
    if (value == null || value === '') {
      return '—';
    }
    return String(value);
  }

  saveProduct(product: TenantProduct): void {
    const settings: UpdateTenantProductSettings = {
      sellingPrice: product.sellingPrice ?? null,
      costPrice: product.costPrice ?? null,
      isStockTracked: this.maintainStock ? product.isStockTracked : false,
      stockQuantity: this.maintainStock && product.isStockTracked ? (product.stockQuantity ?? 0) : 0
    };

    this.savingMedicineId = product.medicineId;
    this.error = '';
    this.successMessage = '';

    this.tenantMedicineService.updateSettings(product.medicineId, settings).subscribe({
      next: (updated) => {
        Object.assign(product, updated);
        this.savedSnapshots.set(product.medicineId, this.snapshotProduct(product));
        this.savingMedicineId = null;
        this.successMessage = `Saved settings for ${product.name}.`;
      },
      error: (err) => {
        this.savingMedicineId = null;
        this.error = err.error?.message || `Failed to save ${product.name}.`;
      }
    });
  }

  resetProduct(product: TenantProduct): void {
    if (!product.id) {
      return;
    }

    if (!confirm(`Clear tenant prices and stock for "${product.name}"?`)) {
      return;
    }

    this.resettingMedicineId = product.medicineId;
    this.error = '';
    this.successMessage = '';

    this.tenantMedicineService.resetProduct(product.medicineId).subscribe({
      next: () => {
        product.id = 0;
        product.sellingPrice = null;
        product.costPrice = null;
        product.isStockTracked = false;
        product.stockQuantity = 0;
        this.savedSnapshots.set(product.medicineId, this.snapshotProduct(product));
        this.resettingMedicineId = null;
        this.successMessage = `Cleared tenant settings for ${product.name}.`;
      },
      error: (err) => {
        this.resettingMedicineId = null;
        this.error = err.error?.message || 'Failed to reset product settings.';
      }
    });
  }

  hasTenantSettings(product: TenantProduct): boolean {
    return product.id > 0;
  }

  isProductDirty(product: TenantProduct): boolean {
    const saved = this.savedSnapshots.get(product.medicineId);
    if (!saved) {
      return false;
    }

    return !this.snapshotsEqual(saved, this.snapshotProduct(product));
  }

  private syncSavedSnapshots(products: TenantProduct[]): void {
    for (const product of products) {
      this.savedSnapshots.set(product.medicineId, this.snapshotProduct(product));
    }
  }

  private snapshotProduct(product: TenantProduct): ProductEditableSnapshot {
    return {
      costPrice: product.costPrice ?? null,
      sellingPrice: product.sellingPrice ?? null,
      stockQuantity: product.stockQuantity ?? 0,
      isStockTracked: product.isStockTracked ?? false
    };
  }

  private snapshotsEqual(a: ProductEditableSnapshot, b: ProductEditableSnapshot): boolean {
    return a.costPrice === b.costPrice
      && a.sellingPrice === b.sellingPrice
      && a.stockQuantity === b.stockQuantity
      && a.isStockTracked === b.isStockTracked;
  }
}
