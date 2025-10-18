import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ReceiptData } from '../../core/models/receipt';

@Injectable({
    providedIn: 'root'
})
export class ReceiptService {
    private baseUrl = 'http://localhost:5003/api';

    constructor(private http: HttpClient) { }

    createReceipt(receiptData: ReceiptData): Observable<ReceiptData> {
        return this.http.post<ReceiptData>(`${this.baseUrl}/Receipt`, receiptData);
    }

    getReceipt(id: number): Observable<ReceiptData> {
        return this.http.get<ReceiptData>(`${this.baseUrl}/Receipt/${id}`);
    }

    getAllReceipts(): Observable<ReceiptData[]> {
        return this.http.get<ReceiptData[]>(`${this.baseUrl}/Receipt`);
    }
}