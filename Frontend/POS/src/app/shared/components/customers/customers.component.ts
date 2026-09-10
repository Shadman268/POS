import { Component, OnInit } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import {
  CreateCustomerRequest,
  CUSTOMER_TABLE_COLUMNS,
  Customer
} from '../../../core/models/customer';
import { AddCustomerDialogComponent } from '../../dialogs/add-customer-dialog/add-customer-dialog.component';
import { CustomerService } from '../../services/customer.service';

@Component({
  selector: 'app-customers',
  templateUrl: './customers.component.html',
  styleUrls: ['./customers.component.scss']
})
export class CustomersComponent implements OnInit {
  columns = CUSTOMER_TABLE_COLUMNS;
  customers: Customer[] = [];
  totalCount = 0;
  page = 1;
  pageSize = 50;
  search = '';
  loading = false;
  saving = false;
  error = '';
  successMessage = '';

  readonly pageSizeOptions = [25, 50, 100, 200];

  constructor(
    private customerService: CustomerService,
    private dialog: MatDialog
  ) {}

  ngOnInit(): void {
    this.loadCustomers();
  }

  get totalPages(): number {
    return Math.max(1, Math.ceil(this.totalCount / this.pageSize));
  }

  get hasData(): boolean {
    return this.totalCount > 0;
  }

  openAddCustomerDialog(): void {
    const dialogRef = this.dialog.open(AddCustomerDialogComponent, {
      width: '420px',
      maxWidth: '95vw'
    });

    dialogRef.afterClosed().subscribe((request: CreateCustomerRequest | undefined) => {
      if (!request) {
        return;
      }

      this.saving = true;
      this.error = '';
      this.successMessage = '';

      this.customerService.createCustomer(request).subscribe({
        next: (created) => {
          this.saving = false;
          this.successMessage = `Added ${created.name}.`;
          this.page = 1;
          this.loadCustomers();
        },
        error: (err) => {
          this.saving = false;
          this.error = err.error?.message || 'Failed to add customer.';
        }
      });
    });
  }

  loadCustomers(): void {
    this.loading = true;
    this.customerService.getCustomers(this.page, this.pageSize, this.search).subscribe({
      next: (result) => {
        this.customers = result.items;
        this.totalCount = result.totalCount;
        this.page = result.page;
        this.pageSize = result.pageSize;
        this.loading = false;
      },
      error: () => {
        this.customers = [];
        this.totalCount = 0;
        this.loading = false;
        this.error = 'Failed to load customers.';
      }
    });
  }

  onSearch(): void {
    this.page = 1;
    this.loadCustomers();
  }

  onPageSizeChange(): void {
    this.page = 1;
    this.loadCustomers();
  }

  goToPage(page: number): void {
    if (page < 1 || page > this.totalPages) {
      return;
    }
    this.page = page;
    this.loadCustomers();
  }
}
