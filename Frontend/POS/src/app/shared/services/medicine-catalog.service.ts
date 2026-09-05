import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { timeout } from 'rxjs/operators';
import { MedicineImportResult, PagedMedicineResult } from '../../core/models/medicine';
import { ApiConfigService } from '../../core/services/api-config.service';

@Injectable({
  providedIn: 'root'
})
export class MedicineCatalogService {
  constructor(
    private http: HttpClient,
    private api: ApiConfigService
  ) {}

  getMedicines(page: number, pageSize: number, search?: string): Observable<PagedMedicineResult> {
    let params = new HttpParams()
      .set('page', page)
      .set('pageSize', pageSize);

    if (search?.trim()) {
      params = params.set('search', search.trim());
    }

    return this.http.get<PagedMedicineResult>(this.api.url('Medicine'), { params });
  }

  importCsv(file: File): Observable<MedicineImportResult> {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post<MedicineImportResult>(this.api.url('Medicine/import'), formData)
      .pipe(timeout(600000));
  }

  deleteMedicine(id: number): Observable<void> {
    return this.http.delete<void>(this.api.url(`Medicine/${id}`));
  }
}
