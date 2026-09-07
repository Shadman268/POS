import { Component, OnInit } from '@angular/core';
import { TenantSettingsService } from '../../../services/tenant-settings.service';

@Component({
  selector: 'app-settings-receipt',
  templateUrl: './settings-receipt.component.html',
  styleUrls: ['../settings-shared.scss', './settings-receipt.component.scss']
})
export class SettingsReceiptComponent implements OnInit {
  shopName = '';
  receiptHeader = '';
  receiptFooter = '';
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
        this.shopName = settings.name || '';
        this.receiptHeader = settings.receiptHeader || '';
        this.receiptFooter = settings.receiptFooter || '';
        this.loading = false;
      },
      error: () => {
        this.loading = false;
        this.error = 'Failed to load receipt settings.';
      }
    });
  }

  save(): void {
    if (!this.shopName.trim()) {
      this.error = 'Shop name is required.';
      return;
    }

    this.saving = true;
    this.error = '';
    this.successMessage = '';

    this.tenantSettingsService.updateSettings({
      name: this.shopName.trim(),
      receiptHeader: this.receiptHeader,
      receiptFooter: this.receiptFooter
    }).subscribe({
      next: () => {
        this.saving = false;
        this.successMessage = 'Receipt settings saved.';
      },
      error: () => {
        this.saving = false;
        this.error = 'Failed to save receipt settings.';
      }
    });
  }
}
