import { Component, ElementRef, OnInit, ViewChild } from '@angular/core';
import { Medicine, MEDICINE_TABLE_COLUMNS } from '../../../core/models/medicine';
import { MedicineCatalogService } from '../../services/medicine-catalog.service';

@Component({
  selector: 'app-catalog',
  templateUrl: './catalog.component.html',
  styleUrls: ['./catalog.component.scss']
})
export class CatalogComponent implements OnInit {
  @ViewChild('fileInput') fileInput!: ElementRef<HTMLInputElement>;

  columns = MEDICINE_TABLE_COLUMNS;
  medicines: Medicine[] = [];
  totalCount = 0;
  page = 1;
  pageSize = 50;
  search = '';
  loading = false;
  importing = false;
  deletingId: number | null = null;
  importMessage = '';
  importError = '';

  readonly pageSizeOptions = [25, 50, 100, 200];

  constructor(private catalogService: MedicineCatalogService) {}

  ngOnInit(): void {
    this.loadMedicines();
  }

  get totalPages(): number {
    return Math.max(1, Math.ceil(this.totalCount / this.pageSize));
  }

  get hasData(): boolean {
    return this.totalCount > 0;
  }

  triggerFileSelect(): void {
    this.fileInput.nativeElement.click();
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) {
      return;
    }

    if (!file.name.toLowerCase().endsWith('.csv')) {
      this.importError = 'Please select a CSV file.';
      this.importMessage = '';
      input.value = '';
      return;
    }

    this.importing = true;
    this.importError = '';
    this.importMessage = '';

    this.catalogService.importCsv(file).subscribe({
      next: (result) => {
        this.importing = false;
        const hasRowErrors = result.errors?.length > 0;
        if (hasRowErrors && result.inserted === 0 && result.updated === 0) {
          this.importMessage = '';
          this.importError = result.errors.join(' ');
        } else {
          this.importMessage = `Import complete: ${result.inserted} inserted, ${result.updated} updated, ${result.skipped} skipped.`;
          this.importError = hasRowErrors ? result.errors.slice(0, 5).join(' ') : '';
        }
        input.value = '';
        this.page = 1;
        this.loadMedicines();
      },
      error: (err) => {
        this.importing = false;
        this.importError = err.error?.message || err.error || 'Failed to import CSV file.';
        this.importMessage = '';
        input.value = '';
      }
    });
  }

  loadMedicines(): void {
    this.loading = true;
    this.catalogService.getMedicines(this.page, this.pageSize, this.search).subscribe({
      next: (result) => {
        this.medicines = result.items;
        this.totalCount = result.totalCount;
        this.page = result.page;
        this.pageSize = result.pageSize;
        this.loading = false;
      },
      error: () => {
        this.medicines = [];
        this.totalCount = 0;
        this.loading = false;
      }
    });
  }

  onSearch(): void {
    this.page = 1;
    this.loadMedicines();
  }

  onPageSizeChange(): void {
    this.page = 1;
    this.loadMedicines();
  }

  goToPage(page: number): void {
    if (page < 1 || page > this.totalPages) {
      return;
    }
    this.page = page;
    this.loadMedicines();
  }

  displayValue(medicine: Medicine, key: keyof Medicine): string {
    const value = medicine[key];
    if (value == null || value === '') {
      return '—';
    }
    if (key === 'isActive') {
      return value ? 'Yes' : 'No';
    }
    if (key === 'createdAtUtc' || key === 'updatedAtUtc') {
      return new Date(String(value)).toLocaleString();
    }
    return String(value);
  }

  deleteMedicine(medicine: Medicine): void {
    if (!confirm(`Delete "${medicine.name}" from the global catalog?`)) {
      return;
    }

    this.deletingId = medicine.id;
    this.importError = '';

    this.catalogService.deleteMedicine(medicine.id).subscribe({
      next: () => {
        this.deletingId = null;
        if (this.medicines.length === 1 && this.page > 1) {
          this.page--;
        }
        this.loadMedicines();
      },
      error: (err) => {
        this.deletingId = null;
        this.importError = err.error?.message || 'Failed to delete medicine.';
      }
    });
  }
}
