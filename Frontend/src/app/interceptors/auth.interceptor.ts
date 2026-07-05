import { Injectable } from '@angular/core';
import { HttpErrorResponse, HttpEvent, HttpHandler, HttpInterceptor, HttpRequest } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError, switchMap } from 'rxjs/operators';
import { AuthService } from '../services/auth.service';
import { API_BASE_URL } from '../config/api.config';
import { ToastService } from '../services/toast.service';

@Injectable()
export class AuthInterceptor implements HttpInterceptor {
  constructor(private authService: AuthService, private toast: ToastService) {}

  intercept(req: HttpRequest<unknown>, next: HttpHandler): Observable<HttpEvent<unknown>> {
    const isApi = req.url.startsWith(API_BASE_URL);
    // Le portail client a sa propre session/JWT (ClientPortalInterceptor) : ne jamais y
    // attacher le token interne, ni tenter un refresh/logout interne dessus.
    const isClientPortal = req.url.startsWith(`${API_BASE_URL}/client-portal/`);
    const token = this.authService.getToken();

    if (!isApi || isClientPortal) {
      return next.handle(req);
    }

    const attachToken = (currentToken: string | null) =>
      currentToken
        ? req.clone({ setHeaders: { Authorization: `Bearer ${currentToken}` } })
        : req;

    if (token && this.authService.isTokenExpired()) {
      return this.authService.refreshToken().pipe(
        switchMap((success) => {
          if (success) {
            const newToken = this.authService.getToken();
            this.toast.info('Session renouvelée', 2000);
            return next.handle(attachToken(newToken));
          }

          this.toast.error('Session expirée, reconnectez-vous');
          this.authService.logout();
          return throwError(() => new Error('Session expired'));
        }),
        catchError((err) => throwError(() => err))
      );
    }

    return next.handle(attachToken(token)).pipe(
      catchError((err: HttpErrorResponse) => {
        if (err.status === 401 && token) {
          return this.authService.refreshToken().pipe(
            switchMap((success) => {
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

        this.showHttpError(err);
        return throwError(() => err);
      })
    );
  }

  private showHttpError(err: HttpErrorResponse): void {
    if (err.status === 0) {
      this.toast.error('Impossible de joindre le serveur');
      return;
    }

    if (err.status === 403) {
      this.toast.error('Accès refusé');
      return;
    }

    if (err.status >= 500) {
      this.toast.error('Erreur serveur. Réessayez dans un instant.');
      return;
    }

    const message = this.extractErrorMessage(err);
    if (message) {
      this.toast.error(message);
    }
  }

  private extractErrorMessage(err: HttpErrorResponse): string {
    if (typeof err.error === 'string') return err.error;
    if (err.error?.error) return err.error.error;
    if (err.error?.message) return err.error.message;
    return err.message || 'Une erreur est survenue';
  }
}
