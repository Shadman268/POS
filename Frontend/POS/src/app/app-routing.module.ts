import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { PosUiComponent } from './shared/components/pos-ui/pos-ui.component';
import { LoginComponent } from './shared/components/login/login.component';
import { RegisterComponent } from './shared/components/register/register.component';
import { DashboardComponent } from './shared/components/dashboard/dashboard.component';
import { MainLayoutComponent } from './shared/components/main-layout/main-layout.component';
import { AllProductsComponent } from './shared/components/all-products/all-products.component';
import { CatalogComponent } from './shared/components/catalog/catalog.component';
import { SettingsShellComponent } from './shared/components/settings/settings-shell/settings-shell.component';
import { SettingsProductComponent } from './shared/components/settings/settings-product/settings-product.component';
import { SettingsReceiptComponent } from './shared/components/settings/settings-receipt/settings-receipt.component';
import { SettingsLineComponent } from './shared/components/settings/settings-line/settings-line.component';
import { CustomersComponent } from './shared/components/customers/customers.component';
import { AuthGuard } from './core/guards/auth.guard';

const routes: Routes = [
  { path: 'login', component: LoginComponent },
  { path: 'register', component: RegisterComponent },
  {
    path: '',
    component: MainLayoutComponent,
    canActivate: [AuthGuard],
    children: [
      {
        path: 'dashboard',
        component: DashboardComponent,
        data: { title: 'Dashboard' }
      },
      {
        path: 'pos',
        component: PosUiComponent,
        data: { title: 'POS Terminal' }
      },
      {
        path: 'products',
        component: AllProductsComponent,
        data: { title: 'All Products' }
      },
      {
        path: 'products/catalog',
        component: CatalogComponent,
        data: { title: 'Catalog' }
      },
      {
        path: 'customers',
        component: CustomersComponent,
        data: { title: 'Customers' }
      },
      {
        path: 'settings',
        component: SettingsShellComponent,
        data: { title: 'Settings' },
        children: [
          { path: '', redirectTo: 'product', pathMatch: 'full' },
          { path: 'product', component: SettingsProductComponent, data: { title: 'Product Settings' } },
          { path: 'receipt', component: SettingsReceiptComponent, data: { title: 'Receipt Settings' } },
          { path: 'line', component: SettingsLineComponent, data: { title: 'Line Settings' } }
        ]
      },
      {
        path: '',
        redirectTo: 'dashboard',
        pathMatch: 'full'
      }
    ]
  },
  { path: '**', redirectTo: 'dashboard' }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
