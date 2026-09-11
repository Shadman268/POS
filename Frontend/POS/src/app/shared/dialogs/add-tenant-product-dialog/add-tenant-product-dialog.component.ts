import { Component, ElementRef, OnInit, ViewChild } from '@angular/core';
import { MatDialogRef } from '@angular/material/dialog';
import { forkJoin, Observable, of } from 'rxjs';
import { catchError, map, switchMap } from 'rxjs/operators';
import { CatalogOptionType, TenantCatalogOptions } from 'src/app/core/models/tenant-catalog';
import { CreateTenantProductRequest } from 'src/app/core/models/tenant-product';
import { TenantCatalogService } from '../../services/tenant-catalog.service';

const LEGACY_CATALOG_KEYS: { key: string; type: CatalogOptionType }[] = [
  { key: 'pos.customDosageForms', type: 'DosageForm' },
  { key: 'pos.customBrands', type: 'Brand' },
  { key: 'pos.customUnits', type: 'Unit' }
];

const DEFAULT_DOSAGE_FORMS = [
  'Tablet',
  'Capsule',
  'Syrup',
  'Suspension',
  'Injection',
  'Infusion',
  'Cream',
  'Ointment',
  'Gel',
  'Lotion',
  'Drops',
  'Eye Drops',
  'Ear Drops',
  'Nasal Spray',
  'Inhaler',
  'Powder',
  'Sachet',
  'Suppository',
  'Solution',
  'Patch'
];

const DEFAULT_UNITS = [
  'Piece',
  'Tablet',
  'Capsule',
  'Strip',
  'Box',
  'Bottle',
  'Vial',
  'Ampoule',
  'Tube',
  'Sachet',
  'Pack',
  'Jar',
  'Carton',
  'Can',
  'Roll',
  'Unit'
];

const DEFAULT_BRANDS = [
  'General',
  'Square',
  'Beximco',
  'Incepta',
  'ACI',
  'Renata',
  'Healthcare',
  'Aristopharma',
  'Eskayef',
  'Opsonin',
  'ACME',
  'Popular',
  'Radiant',
  'Drug International',
  'Navana',
  'Sanofi',
  'GSK',
  'Novartis',
  'Pfizer',
  'Cipla',
  'Sun Pharma'
];

const CREATE_NEW_DOSE_VALUE = '__create_new_dose__';
const CREATE_NEW_BRAND_VALUE = '__create_new_brand__';
const CREATE_NEW_UNIT_VALUE = '__create_new_unit__';

@Component({
  selector: 'app-add-tenant-product-dialog',
  templateUrl: './add-tenant-product-dialog.component.html',
  styleUrls: ['./add-tenant-product-dialog.component.scss']
})
export class AddTenantProductDialogComponent implements OnInit {
  readonly createNewDoseValue = CREATE_NEW_DOSE_VALUE;
  readonly createNewBrandValue = CREATE_NEW_BRAND_VALUE;
  readonly createNewUnitValue = CREATE_NEW_UNIT_VALUE;

  dosageForms = [...DEFAULT_DOSAGE_FORMS];
  brands = [...DEFAULT_BRANDS];
  units = [...DEFAULT_UNITS];

  creatingNewDose = false;
  creatingNewBrand = false;
  creatingNewUnit = false;
  loadingOptions = true;
  savingOption = false;
  newDoseName = '';
  newBrandName = '';
  newUnitName = '';

  @ViewChild('newDoseInput') newDoseInput?: ElementRef<HTMLInputElement>;
  @ViewChild('newBrandInput') newBrandInput?: ElementRef<HTMLInputElement>;
  @ViewChild('newUnitInput') newUnitInput?: ElementRef<HTMLInputElement>;

  form: CreateTenantProductRequest = {
    name: '',
    genericName: '',
    strength: '',
    dosageForm: 'Tablet',
    category: 'Medicine',
    brand: 'General',
    unit: 'Piece',
    barcode: '',
    sellingPrice: 0,
    costPrice: null,
    isStockTracked: true,
    localSku: '',
    initialStock: 0,
    batchNumber: '',
    expiryDate: null
  };

  saving = false;
  error = '';

  constructor(
    public dialogRef: MatDialogRef<AddTenantProductDialogComponent>,
    private tenantCatalogService: TenantCatalogService
  ) {}

  ngOnInit(): void {
    this.loadCatalogOptions();
  }

  onDosageFormChange(value: string): void {
    if (value !== CREATE_NEW_DOSE_VALUE) {
      return;
    }

    this.form.dosageForm = '';
    this.creatingNewDose = true;
    this.newDoseName = '';
    setTimeout(() => this.newDoseInput?.nativeElement.focus());
  }

  onNewDoseEnter(event: Event): void {
    event.preventDefault();
    this.confirmNewDose();
  }

  confirmNewDose(): void {
    this.saveCustomOption('DosageForm', this.newDoseName, name => {
      this.form.dosageForm = name;
      this.creatingNewDose = false;
      this.newDoseName = '';
    });
  }

  cancelNewDose(): void {
    this.creatingNewDose = false;
    this.newDoseName = '';
    this.form.dosageForm = 'Tablet';
  }

  onBrandChange(value: string): void {
    if (value !== CREATE_NEW_BRAND_VALUE) {
      return;
    }

    this.form.brand = '';
    this.creatingNewBrand = true;
    this.newBrandName = '';
    setTimeout(() => this.newBrandInput?.nativeElement.focus());
  }

  onNewBrandEnter(event: Event): void {
    event.preventDefault();
    this.confirmNewBrand();
  }

  confirmNewBrand(): void {
    this.saveCustomOption('Brand', this.newBrandName, name => {
      this.form.brand = name;
      this.creatingNewBrand = false;
      this.newBrandName = '';
    });
  }

  cancelNewBrand(): void {
    this.creatingNewBrand = false;
    this.newBrandName = '';
    this.form.brand = 'General';
  }

  onUnitChange(value: string): void {
    if (value !== CREATE_NEW_UNIT_VALUE) {
      return;
    }

    this.form.unit = '';
    this.creatingNewUnit = true;
    this.newUnitName = '';
    setTimeout(() => this.newUnitInput?.nativeElement.focus());
  }

  onNewUnitEnter(event: Event): void {
    event.preventDefault();
    this.confirmNewUnit();
  }

  confirmNewUnit(): void {
    this.saveCustomOption('Unit', this.newUnitName, name => {
      this.form.unit = name;
      this.creatingNewUnit = false;
      this.newUnitName = '';
    });
  }

  cancelNewUnit(): void {
    this.creatingNewUnit = false;
    this.newUnitName = '';
    this.form.unit = 'Piece';
  }

  generateBarcode(): void {
    this.form.barcode = generateEan13Barcode();
  }

  onSubmit(): void {
    if (this.creatingNewDose && this.newDoseName.trim()) {
      this.saveCustomOption('DosageForm', this.newDoseName, name => {
        this.form.dosageForm = name;
        this.creatingNewDose = false;
        this.newDoseName = '';
        this.submitForm();
      });
      return;
    }

    if (this.creatingNewBrand && this.newBrandName.trim()) {
      this.saveCustomOption('Brand', this.newBrandName, name => {
        this.form.brand = name;
        this.creatingNewBrand = false;
        this.newBrandName = '';
        this.submitForm();
      });
      return;
    }

    if (this.creatingNewUnit && this.newUnitName.trim()) {
      this.saveCustomOption('Unit', this.newUnitName, name => {
        this.form.unit = name;
        this.creatingNewUnit = false;
        this.newUnitName = '';
        this.submitForm();
      });
      return;
    }

    this.submitForm();
  }

  private submitForm(): void {
    if (this.savingOption || this.loadingOptions) {
      return;
    }

    if (!this.form.name.trim() || this.form.sellingPrice <= 0) {
      return;
    }

    this.dialogRef.close({
      ...this.form,
      name: this.form.name.trim(),
      genericName: this.form.genericName?.trim() || null,
      strength: this.form.strength?.trim() || null,
      dosageForm: this.form.dosageForm?.trim() || null,
      barcode: this.form.barcode?.trim() || null,
      localSku: this.form.localSku?.trim() || null,
      batchNumber: this.form.batchNumber?.trim() || null,
      expiryDate: this.form.expiryDate || null
    } as CreateTenantProductRequest);
  }

  onCancel(): void {
    this.dialogRef.close();
  }

  private loadCatalogOptions(): void {
    this.loadingOptions = true;
    this.error = '';

    this.migrateLegacyCatalogOptions().pipe(
      switchMap(() => this.tenantCatalogService.getOptions())
    ).subscribe({
      next: (options: TenantCatalogOptions) => {
        this.applyCatalogOptions(options);
        this.loadingOptions = false;
      },
      error: (err: { error?: { message?: string } }) => {
        this.loadingOptions = false;
        this.error = err.error?.message || 'Failed to load catalog options.';
      }
    });
  }

  private migrateLegacyCatalogOptions(): Observable<void> {
    const migrations = LEGACY_CATALOG_KEYS.flatMap(({ key, type }) =>
      readLegacyCatalogNames(key).map(name =>
        this.tenantCatalogService.addOption(type, name).pipe(catchError(() => of(null)))
      )
    );

    LEGACY_CATALOG_KEYS.forEach(({ key }) => localStorage.removeItem(key));

    if (migrations.length === 0) {
      return of(undefined);
    }

    return forkJoin(migrations).pipe(map(() => undefined));
  }

  private applyCatalogOptions(options: TenantCatalogOptions): void {
    this.dosageForms = mergePresetList(DEFAULT_DOSAGE_FORMS, options.dosageForms);
    this.brands = mergePresetList(DEFAULT_BRANDS, options.brands);
    this.units = mergePresetList(DEFAULT_UNITS, options.units);
  }

  private saveCustomOption(
    type: CatalogOptionType,
    rawName: string,
    onSuccess: (name: string) => void
  ): void {
    const name = rawName.trim();
    if (!name || this.savingOption) {
      return;
    }

    const existingList = type === 'DosageForm'
      ? this.dosageForms
      : type === 'Brand'
        ? this.brands
        : this.units;
    const existing = existingList.find(item => item.toLowerCase() === name.toLowerCase());
    if (existing) {
      onSuccess(existing);
      return;
    }

    this.savingOption = true;
    this.error = '';

    this.tenantCatalogService.addOption(type, name).subscribe({
      next: options => {
        this.applyCatalogOptions(options);
        this.savingOption = false;
        onSuccess(name);
      },
      error: err => {
        this.savingOption = false;
        this.error = err.error?.message || `Failed to save ${type.toLowerCase()}.`;
      }
    });
  }
}

function readLegacyCatalogNames(key: string): string[] {
  try {
    const raw = localStorage.getItem(key);
    const parsed = raw ? JSON.parse(raw) : [];
    if (!Array.isArray(parsed)) {
      return [];
    }
    return parsed
      .filter((name): name is string => typeof name === 'string' && !!name.trim())
      .map(name => name.trim());
  } catch {
    return [];
  }
}

function mergePresetList(presets: string[], custom: string[]): string[] {
  const seen = new Set(presets.map(name => name.toLowerCase()));
  const extras = custom.filter(name => {
    const trimmed = name.trim();
    if (!trimmed) {
      return false;
    }
    const key = trimmed.toLowerCase();
    if (seen.has(key)) {
      return false;
    }
    seen.add(key);
    return true;
  });
  return [...presets, ...extras];
}

function generateEan13Barcode(): string {
  const seed = `${Date.now()}${Math.floor(Math.random() * 1000).toString().padStart(3, '0')}`;
  const body = `200${seed}`.slice(0, 12);
  return `${body}${ean13CheckDigit(body)}`;
}

function ean13CheckDigit(digits: string): string {
  let sum = 0;
  for (let i = 0; i < 12; i++) {
    const value = Number(digits[i]);
    sum += i % 2 === 0 ? value : value * 3;
  }
  return ((10 - (sum % 10)) % 10).toString();
}
