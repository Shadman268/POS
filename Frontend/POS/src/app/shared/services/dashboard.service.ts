import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { DashboardPeriod, DashboardSummary } from '../../core/models/dashboard';
import { ApiConfigService } from '../../core/services/api-config.service';

@Injectable({
  providedIn: 'root'
})
export class DashboardService {
  constructor(
    private http: HttpClient,
    private api: ApiConfigService
  ) {}

  getSummary(period: DashboardPeriod = 'today'): Observable<DashboardSummary> {
    return this.http.get<DashboardSummary>(this.api.url(`Dashboard/summary?period=${period}`));
  }
}
