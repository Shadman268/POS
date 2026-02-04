import { Injectable } from '@angular/core';
import { HttpRequest, HttpHandler, HttpEvent, HttpInterceptor, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError, BehaviorSubject } from 'rxjs';
import { catchError, filter, take, switchMap } from 'rxjs/operators';
import { AuthService } from '../services/auth.service';

@Injectable()
export class JwtInterceptor implements HttpInterceptor {
    private isRefreshing = false;
    private refreshTokenSubject: BehaviorSubject<string | null> = new BehaviorSubject<string | null>(null);

    constructor(private authService: AuthService) { }

    intercept(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
        // Add authorization header with JWT token if available
        const token = this.authService.currentToken;
        const isApiUrl = request.url.startsWith('http://localhost:5003/api');

        if (token && isApiUrl) {
            request = this.addToken(request, token);
        }

        // Always send credentials for cookie handling
        if (isApiUrl) {
            request = request.clone({ withCredentials: true });
        }

        return next.handle(request).pipe(
            catchError(error => {
                if (error instanceof HttpErrorResponse && error.status === 401 && isApiUrl) {
                    // Don't try to refresh if this is already a refresh request
                    if (request.url.includes('/Auth/refresh-token') || request.url.includes('/Auth/login')) {
                        return throwError(() => error);
                    }
                    return this.handle401Error(request, next);
                }
                return throwError(() => error);
            })
        );
    }

    private addToken(request: HttpRequest<any>, token: string): HttpRequest<any> {
        return request.clone({
            setHeaders: {
                Authorization: `Bearer ${token}`
            }
        });
    }

    private handle401Error(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
        if (!this.isRefreshing) {
            this.isRefreshing = true;
            this.refreshTokenSubject.next(null);

            return this.authService.refreshToken().pipe(
                switchMap(response => {
                    this.isRefreshing = false;
                    this.refreshTokenSubject.next(response.accessToken);
                    return next.handle(this.addToken(request, response.accessToken));
                }),
                catchError(error => {
                    this.isRefreshing = false;
                    return throwError(() => error);
                })
            );
        } else {
            // Wait for the refresh to complete and retry with new token
            return this.refreshTokenSubject.pipe(
                filter(token => token !== null),
                take(1),
                switchMap(token => next.handle(this.addToken(request, token!)))
            );
        }
    }
}
