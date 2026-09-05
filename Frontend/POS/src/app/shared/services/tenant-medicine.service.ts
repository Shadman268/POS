import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import {
  CreateTenantProductRequest,
  PagedTenantProductResult,
  TenantProduct,
  UpdateTenantProductSettings
} from '../../core/models/tenant-product';
import { ApiConfigService } from '../../core/services/api-config.service';

@Injectable({
  providedIn: 'root'
})
export class TenantMedicineService {
  constructor(
    private http: HttpClient,
    private api: ApiConfigService
  ) {}

  getProducts(page: number, pageSize: number, search?: string): Observable<PagedTenantProductResult> {
    let params = new HttpParams()
      .set('page', page)
      .set('pageSize', pageSize);

    if (search?.trim()) {
      params = params.set('search', search.trim());
    }

    return this.http.get<PagedTenantProductResult>(this.api.url('TenantMedicine'), { params });
  }

  updateSettings(medicineId: number, settings: UpdateTenantProductSettings): Observable<TenantProduct> {
    return this.http.put<TenantProduct>(this.api.url(`TenantMedicine/${medicineId}/settings`), settings);
  }

  createProduct(request: CreateTenantProductRequest): Observable<TenantProduct> {
    return this.http.post<TenantProduct>(this.api.url('TenantMedicine'), request);
  }

  resetProduct(medicineId: number): Observable<void> {
    return this.http.delete<void>(this.api.url(`TenantMedicine/by-medicine/${medicineId}`));
  }
}
