import { Injectable } from '@angular/core';
import { Subject } from 'rxjs';

@Injectable({
    providedIn: 'root'
})
export class SidebarService {
    private toggleSidebarSubject = new Subject<void>();
    public toggleSidebar$ = this.toggleSidebarSubject.asObservable();

    toggle() {
        this.toggleSidebarSubject.next();
    }
}
