import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { PosUiComponent } from './shared/components/pos-ui/pos-ui.component';
import { LoginComponent } from './shared/components/login/login.component';
import { RegisterComponent } from './shared/components/register/register.component';
import { DashboardComponent } from './shared/components/dashboard/dashboard.component';
import { MainLayoutComponent } from './shared/components/main-layout/main-layout.component';
import { AllProductsComponent } from './shared/components/all-products/all-products.component';
import { CatalogComponent } from './shared/components/catalog/catalog.component';
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
