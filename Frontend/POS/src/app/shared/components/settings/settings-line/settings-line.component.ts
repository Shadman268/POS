import { Component, OnInit } from '@angular/core';
import { TenantSettingsService } from '../../../services/tenant-settings.service';

@Component({
  selector: 'app-settings-line',
  templateUrl: './settings-line.component.html',
  styleUrls: ['../settings-shared.scss']
})
export class SettingsLineComponent implements OnInit {
  showLineDiscount = false;
  showVat = false;
  vatPercent = 5;
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
        this.showLineDiscount = settings.showLineDiscount;
        this.showVat = settings.showVat;
        this.vatPercent = settings.vatPercent ?? 5;
        this.loading = false;
      },
      error: () => {
        this.loading = false;
        this.error = 'Failed to load line settings.';
      }
    });
  }

  save(): void {
    this.saving = true;
    this.error = '';
    this.successMessage = '';

    this.tenantSettingsService.updateSettings({
      showLineDiscount: this.showLineDiscount,
      showVat: this.showVat,
      vatPercent: Math.min(100, Math.max(0, Math.round(this.vatPercent)))
    }).subscribe({
      next: () => {
        this.saving = false;
        this.successMessage = 'Line settings saved.';
      },
      error: () => {
        this.saving = false;
        this.error = 'Failed to save line settings.';
      }
    });
  }
}
