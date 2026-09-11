import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import {
  AddTenantCatalogOptionRequest,
  CatalogOptionType,
  TenantCatalogOptions
} from '../../core/models/tenant-catalog';
import { ApiConfigService } from '../../core/services/api-config.service';

@Injectable({
  providedIn: 'root'
})
export class TenantCatalogService {
  constructor(
    private http: HttpClient,
    private api: ApiConfigService
  ) {}

  getOptions(): Observable<TenantCatalogOptions> {
    return this.http.get<TenantCatalogOptions>(this.api.url('TenantCatalog/options'));
  }

  addOption(type: CatalogOptionType, name: string): Observable<TenantCatalogOptions> {
    const body: AddTenantCatalogOptionRequest = { type, name };
    return this.http.post<TenantCatalogOptions>(this.api.url('TenantCatalog/options'), body);
  }
}
