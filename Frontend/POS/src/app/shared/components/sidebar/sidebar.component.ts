import { Component, ViewChild, AfterViewInit, OnDestroy } from '@angular/core';
import { MatDrawer } from '@angular/material/sidenav';
import { SidebarService } from '../../services/sidebar.service';
import { Subscription } from 'rxjs';

@Component({
    selector: 'app-sidebar',
    templateUrl: './sidebar.component.html',
    styleUrls: ['./sidebar.component.scss']
})
export class SidebarComponent implements AfterViewInit, OnDestroy {
    @ViewChild('drawer') drawer!: MatDrawer;
    private subscription!: Subscription;

    constructor(private sidebarService: SidebarService) { }

    ngAfterViewInit() {
        this.subscription = this.sidebarService.toggleSidebar$.subscribe(() => {
            console.log('Toggling sidebar');
            this.drawer.toggle();
        });
    }

    ngOnDestroy() {
        if (this.subscription) {
            this.subscription.unsubscribe();
        }
    }
}
