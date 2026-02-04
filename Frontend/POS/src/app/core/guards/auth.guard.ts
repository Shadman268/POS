import { Injectable } from '@angular/core';
import { Router, CanActivate, ActivatedRouteSnapshot, RouterStateSnapshot } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { UserRole } from '../models/user';

@Injectable({
    providedIn: 'root'
})
export class AuthGuard implements CanActivate {

    constructor(
        private router: Router,
        private authService: AuthService
    ) { }

    canActivate(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): boolean {
        const currentUser = this.authService.currentUserValue;

        if (currentUser) {
            // Check if route is restricted by role
            const allowedRoles = route.data['roles'] as UserRole[];
            if (allowedRoles && allowedRoles.length > 0) {
                // Check if user has required role
                if (this.authService.hasAnyRole(allowedRoles)) {
                    return true;
                }

                // Role not authorized, redirect to home
                this.router.navigate(['/pos']);
                return false;
            }

            // Authorized, return true
            return true;
        }

        // Not logged in, redirect to login page
        const returnUrl = state.url !== '/pos' ? state.url : '/pos';
        this.router.navigate(['/login'], { queryParams: { returnUrl } });
        return false;
    }
}
