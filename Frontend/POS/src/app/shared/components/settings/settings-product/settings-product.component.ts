import { Component, OnInit } from '@angular/core';
import { TenantSettingsService } from '../../../services/tenant-settings.service';

@Component({
  selector: 'app-settings-product',
  templateUrl: './settings-product.component.html',
  styleUrls: ['../settings-shared.scss']
})
export class SettingsProductComponent implements OnInit {
  maintainStock = true;
  loading = true;
  saving = false;
  error = '';
  successMessage = '';

  constructor(private tenantSettingsService: TenantSettingsService) {}

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading = true;
    this.tenantSettingsService.loadSettings().subscribe({
      next: (settings) => {
        this.maintainStock = settings.maintainStock;
        this.loading = false;
      },
      error: () => {
        this.loading = false;
        this.error = 'Failed to load product settings.';
      }
    });
  }

  save(): void {
    this.saving = true;
    this.error = '';
    this.successMessage = '';

    this.tenantSettingsService.updateSettings({ maintainStock: this.maintainStock }).subscribe({
      next: () => {
        this.saving = false;
        this.successMessage = 'Product settings saved.';
      },
      error: () => {
        this.saving = false;
        this.error = 'Failed to save product settings.';
      }
    });
  }
}
