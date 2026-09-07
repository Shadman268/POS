import { Component } from '@angular/core';

@Component({
  selector: 'app-settings-shell',
  templateUrl: './settings-shell.component.html',
  styleUrls: ['./settings-shell.component.scss']
})
export class SettingsShellComponent {
  readonly tabs = [
    { label: 'Product Settings', route: '/settings/product', icon: 'medication' },
    { label: 'Receipt Settings', route: '/settings/receipt', icon: 'receipt_long' },
    { label: 'Line Settings', route: '/settings/line', icon: 'format_list_numbered' }
  ];
}
