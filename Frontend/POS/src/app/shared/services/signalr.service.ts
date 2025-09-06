import { Injectable, OnDestroy } from '@angular/core';
import { HubConnection, HubConnectionBuilder } from '@microsoft/signalr';
import { Subject } from 'rxjs';
import { ProductView } from 'src/app/core/models/product-data';

@Injectable({
    providedIn: 'root'
})
export class SignalrService implements OnDestroy {
    private hubConnection!: HubConnection;
    private productAddedSubject = new Subject<ProductView>();

    public productAdded$ = this.productAddedSubject.asObservable();

    constructor() {
        this.createConnection();
        this.registerOnServerEvents();
        this.startConnection();
    }

    private createConnection() {
        this.hubConnection = new HubConnectionBuilder()
            .withUrl('http://localhost:5000/productHub')
            .build();
    }

    private registerOnServerEvents(): void {
        this.hubConnection.on('ProductAdded', (product: ProductView) => {
            this.productAddedSubject.next(product);
        });
    }

    private startConnection(): void {
        this.hubConnection
            .start()
            .then(() => {
                console.log('SignalR connection established');
            })
            .catch((err) => {
                console.error('Error while starting SignalR connection: ' + err);
            });
    }

    public stopConnection(): void {
        if (this.hubConnection) {
            this.hubConnection.stop();
        }
    }

    ngOnDestroy(): void {
        this.stopConnection();
    }
}
