import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ReceiptData } from '../../core/models/receipt';
import { ApiConfigService } from '../../core/services/api-config.service';

@Injectable({
    providedIn: 'root'
})
export class ReceiptService {
    constructor(
        private http: HttpClient,
        private api: ApiConfigService
    ) {}

    createReceipt(receiptData: ReceiptData): Observable<ReceiptData> {
        return this.http.post<ReceiptData>(this.api.url('Receipt'), receiptData);
    }

    getReceipt(id: number): Observable<ReceiptData> {
        return this.http.get<ReceiptData>(this.api.url(`Receipt/${id}`));
    }

    getAllReceipts(): Observable<ReceiptData[]> {
        return this.http.get<ReceiptData[]>(this.api.url('Receipt'));
    }
}
