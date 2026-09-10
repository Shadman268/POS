import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable, tap } from 'rxjs';
import { CreateCustomerRequest, Customer, PagedCustomerResult } from '../../core/models/customer';
import { ApiConfigService } from '../../core/services/api-config.service';

@Injectable({
  providedIn: 'root'
})
export class CustomerService {
  private readonly customersSubject = new BehaviorSubject<Customer[]>([]);
  readonly customers$ = this.customersSubject.asObservable();

  constructor(
    private http: HttpClient,
    private api: ApiConfigService
  ) {}

  getCustomers(page: number, pageSize: number, search?: string): Observable<PagedCustomerResult> {
    let params = new HttpParams()
      .set('page', page)
      .set('pageSize', pageSize);

    if (search?.trim()) {
      params = params.set('search', search.trim());
    }

    return this.http.get<PagedCustomerResult>(this.api.url('Customer'), { params });
  }

  getAllCustomers(): Observable<PagedCustomerResult> {
    return this.getCustomers(1, 500).pipe(
      tap(result => this.customersSubject.next(result.items))
    );
  }

  createCustomer(request: CreateCustomerRequest): Observable<Customer> {
    return this.http.post<Customer>(this.api.url('Customer'), request).pipe(
      tap(created => {
        const next = [...this.customersSubject.value, created]
          .sort((a, b) => a.name.localeCompare(b.name));
        this.customersSubject.next(next);
      })
    );
  }
}
