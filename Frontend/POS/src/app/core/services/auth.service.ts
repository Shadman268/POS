import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, of, throwError } from 'rxjs';
import { map, catchError, tap, switchMap } from 'rxjs/operators';
import { Router } from '@angular/router';
import { LoginRequest, LoginResponse, User, UserRole, RegisterRequest, RegisterResponse, RefreshTokenResponse } from '../../core/models/user';

@Injectable({
    providedIn: 'root'
})
export class AuthService {
    private baseUrl = 'http://localhost:5003/api';
    private currentUserSubject: BehaviorSubject<User | null>;
    public currentUser: Observable<User | null>;

    // Store access token in memory only (short-lived, secure from XSS)
    private accessTokenSubject: BehaviorSubject<string | null> = new BehaviorSubject<string | null>(null);

    // Track if we're currently refreshing to avoid multiple refresh calls
    private isRefreshing = false;

    constructor(
        private http: HttpClient,
        private router: Router
    ) {
        const storedUser = localStorage.getItem('currentUser');
        this.currentUserSubject = new BehaviorSubject<User | null>(
            storedUser ? JSON.parse(storedUser) : null
        );
        this.currentUser = this.currentUserSubject.asObservable();

        // On app init, try to refresh token if user exists (page refresh scenario)
        if (storedUser) {
            this.refreshToken().subscribe({
                error: () => {
                    // Refresh failed, clear user
                    this.clearAuthState();
                }
            });
        }
    }

    public get currentUserValue(): User | null {
        return this.currentUserSubject.value;
    }

    public get currentToken(): string | null {
        return this.accessTokenSubject.value;
    }

    public get accessToken$(): Observable<string | null> {
        return this.accessTokenSubject.asObservable();
    }

    login(credentials: LoginRequest): Observable<LoginResponse> {
        return this.http.post<LoginResponse>(`${this.baseUrl}/Auth/login`, credentials, {
            withCredentials: true // Required for receiving HttpOnly refresh token cookie
        }).pipe(
            map(response => {
                // Store user details in localStorage for persistence across refreshes
                localStorage.setItem('currentUser', JSON.stringify(response.user));
                // Store access token in memory only (short-lived)
                this.accessTokenSubject.next(response.accessToken);
                this.currentUserSubject.next(response.user);
                return response;
            })
        );
    }

    register(data: RegisterRequest): Observable<RegisterResponse> {
        return this.http.post<RegisterResponse>(`${this.baseUrl}/Auth/register`, data);
    }

    /**
     * Refresh the access token using the HttpOnly refresh token cookie.
     * Called automatically on 401 errors and on app initialization.
     */
    refreshToken(): Observable<RefreshTokenResponse> {
        if (this.isRefreshing) {
            // Return current token if already refreshing
            return this.accessToken$.pipe(
                switchMap(token => token ? of({ accessToken: token, user: this.currentUserValue! }) : throwError(() => new Error('No token')))
            );
        }

        this.isRefreshing = true;

        return this.http.post<RefreshTokenResponse>(`${this.baseUrl}/Auth/refresh-token`, {}, {
            withCredentials: true // Send HttpOnly refresh token cookie
        }).pipe(
            tap(response => {
                this.accessTokenSubject.next(response.accessToken);
                if (response.user) {
                    localStorage.setItem('currentUser', JSON.stringify(response.user));
                    this.currentUserSubject.next(response.user);
                }
                this.isRefreshing = false;
            }),
            catchError(error => {
                this.isRefreshing = false;
                this.clearAuthState();
                return throwError(() => error);
            })
        );
    }

    logout(): void {
        // Call backend to clear the HttpOnly refresh token cookie
        this.http.post(`${this.baseUrl}/Auth/logout`, {}, { withCredentials: true })
            .subscribe({
                complete: () => this.clearAuthState(),
                error: () => this.clearAuthState()
            });
    }

    private clearAuthState(): void {
        this.accessTokenSubject.next(null);
        localStorage.removeItem('currentUser');
        this.currentUserSubject.next(null);
        this.router.navigate(['/login']);
    }

    isAuthenticated(): boolean {
        return !!this.currentToken && !!this.currentUserValue;
    }

    hasRole(role: UserRole): boolean {
        const user = this.currentUserValue;
        return user ? user.role === role : false;
    }

    hasAnyRole(roles: UserRole[]): boolean {
        const user = this.currentUserValue;
        return user ? roles.includes(user.role) : false;
    }
}
