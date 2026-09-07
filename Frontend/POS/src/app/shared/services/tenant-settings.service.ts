import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable, tap } from 'rxjs';
import { TenantSettings, UpdateTenantSettings } from '../../core/models/tenant-settings';
import { ApiConfigService } from '../../core/services/api-config.service';

@Injectable({
  providedIn: 'root'
})
export class TenantSettingsService {
  private settingsSubject = new BehaviorSubject<TenantSettings | null>(null);
  settings$ = this.settingsSubject.asObservable();

  constructor(
    private http: HttpClient,
    private api: ApiConfigService
  ) {}

  get settings(): TenantSettings | null {
    return this.settingsSubject.value;
  }

  loadSettings(): Observable<TenantSettings> {
    return this.http.get<TenantSettings>(this.api.url('Settings')).pipe(
      tap(settings => this.settingsSubject.next(settings))
    );
  }

  updateSettings(update: UpdateTenantSettings): Observable<TenantSettings> {
    return this.http.put<TenantSettings>(this.api.url('Settings'), update).pipe(
      tap(settings => this.settingsSubject.next(settings))
    );
  }
}
