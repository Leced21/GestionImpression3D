import { Injectable } from '@angular/core';
import { HttpInterceptor, HttpRequest, HttpHandler, HttpEvent, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError, switchMap } from 'rxjs/operators';
import { AuthService } from '../services/auth.service';
import { API_BASE_URL } from '../config/api.config';
import { ToastService } from '../services/toast.service';

@Injectable()
export class AuthInterceptor implements HttpInterceptor {
  constructor(private authService: AuthService, private toast: ToastService) {}

  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    const isApi = req.url.startsWith(API_BASE_URL);
    const token = this.authService.getToken();

    try { console.debug('[AuthInterceptor] url=', req.url, 'isApi=', isApi, 'tokenPresent=', !!token); } catch {}

    if (!isApi) {
      return next.handle(req);
    }

    const attachToken = (t: string | null) => {
      if (t) {
        try { console.debug('[AuthInterceptor] attaching token to', req.url); } catch {}
        return req.clone({ setHeaders: { Authorization: `Bearer ${t}` } });
      }
      return req;
    };

    // If token exists but is (near) expired, attempt refresh before sending
    if (token && this.authService.isTokenExpired()) {
      try { console.debug('[AuthInterceptor] token expired, attempting refresh'); } catch {}
      return this.authService.refreshToken().pipe(
        switchMap(success => {
          if (success) {
            const newToken = this.authService.getToken();
            this.toast.info('Session renouvelée', 2000);
            return next.handle(attachToken(newToken));
          }
          this.toast.error('Session expirée, reconnectez-vous');
          this.authService.logout();
          return throwError(() => new Error('Session expired'));
        }),
        catchError(err => throwError(() => err as any))
      );
    }

    // Normal request flow: attach token if present
    return next.handle(attachToken(token)).pipe(
      catchError((err: HttpErrorResponse) => {
        if (err.status === 401 && token) {
          try { /* debug */ } catch {}
          return this.authService.refreshToken().pipe(
            switchMap(success => {
              if (success) {
                const newToken = this.authService.getToken();
                this.toast.info('Session renouvelée', 2000);
                return next.handle(attachToken(newToken));
              }
              this.toast.error('Session expirée, reconnectez-vous');
              this.authService.logout();
              return throwError(() => err);
            })
          );
        }
        return throwError(() => err);
      })
    );
  }
}
