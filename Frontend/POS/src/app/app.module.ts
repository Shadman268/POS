import { NgModule } from '@angular/core';
import { ReceiptPreviewComponent } from './shared/components/receipt-preview/receipt-preview.component';
import { BrowserModule } from '@angular/platform-browser';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { PosUiComponent } from './shared/components/pos-ui/pos-ui.component';
import { JournalComponent } from './shared/components/journal/journal.component';
import { ProductViewComponent } from './shared/components/product-view/product-view.component';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatIconModule } from '@angular/material/icon';
import { MatMenuModule } from '@angular/material/menu';
import { MatAutocompleteModule } from '@angular/material/autocomplete';
import { MatListModule } from '@angular/material/list';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { AddProductDialogComponent } from './shared/dialogs/add-product-dialog/add-product-dialog.component';
import { AddTenantProductDialogComponent } from './shared/dialogs/add-tenant-product-dialog/add-tenant-product-dialog.component';
import { SetPriceDialogComponent } from './shared/dialogs/set-price-dialog/set-price-dialog.component';
import { MatDialogModule } from '@angular/material/dialog';
import { MatGridListModule } from '@angular/material/grid-list';
import { LoginComponent } from './shared/components/login/login.component';
import { RegisterComponent } from './shared/components/register/register.component';
import { MainLayoutComponent } from './shared/components/main-layout/main-layout.component';
import { DashboardComponent } from './shared/components/dashboard/dashboard.component';
import { AllProductsComponent } from './shared/components/all-products/all-products.component';
import { CatalogComponent } from './shared/components/catalog/catalog.component';
import { SettingsShellComponent } from './shared/components/settings/settings-shell/settings-shell.component';
import { SettingsProductComponent } from './shared/components/settings/settings-product/settings-product.component';
import { SettingsReceiptComponent } from './shared/components/settings/settings-receipt/settings-receipt.component';
import { SettingsLineComponent } from './shared/components/settings/settings-line/settings-line.component';
import { JwtInterceptor } from './core/interceptors/jwt.interceptor';

@NgModule({
  declarations: [
    ReceiptPreviewComponent,
    AppComponent,
    PosUiComponent,
    JournalComponent,
    ProductViewComponent,
    AddProductDialogComponent,
    AddTenantProductDialogComponent,
    SetPriceDialogComponent,
    LoginComponent,
    RegisterComponent,
    MainLayoutComponent,
    DashboardComponent,
    AllProductsComponent,
    CatalogComponent,
    SettingsShellComponent,
    SettingsProductComponent,
    SettingsReceiptComponent,
    SettingsLineComponent
  ],
  imports: [
    BrowserModule,
    HttpClientModule,
    AppRoutingModule,
    ReactiveFormsModule,
    MatButtonModule,
    MatDialogModule,
    MatCardModule,
    MatCheckboxModule,
    BrowserAnimationsModule,
    FormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatToolbarModule,
    MatIconModule,
    MatMenuModule,
    MatAutocompleteModule,
    MatListModule,
    MatGridListModule,
    MatProgressSpinnerModule,
    MatTooltipModule,
    MatSlideToggleModule
  ],
  providers: [
    { provide: HTTP_INTERCEPTORS, useClass: JwtInterceptor, multi: true }
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
