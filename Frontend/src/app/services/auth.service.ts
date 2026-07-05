// src/app/services/auth.service.ts
import { Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { BehaviorSubject, Observable, catchError, tap, throwError, of, map } from 'rxjs';
import { Router } from '@angular/router';
import { AuthResponse, LoginRequest, RegisterRequest, User } from '../models/user.model';
import { API_BASE_URL, AUTH_TOKEN_KEY, CURRENT_USER_KEY } from '../config/api.config';
import { REFRESH_TOKEN_KEY } from '../config/api.config';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private apiUrl = `${API_BASE_URL}/auth`;
  private currentUserSubject = new BehaviorSubject<User | null>(null);
  public currentUser$ = this.currentUserSubject.asObservable();

  constructor(private http: HttpClient, private router: Router) {
    const stored = localStorage.getItem(CURRENT_USER_KEY);
    if (stored) {
      try {
        this.currentUserSubject.next(JSON.parse(stored));
      } catch {
        localStorage.removeItem(CURRENT_USER_KEY);
      }
    }
  }

  login(request: LoginRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/login`, request).pipe(
      tap(response => this.storeAuthResponse(response)),
      catchError(this.handleError)
    );
  }

  register(request: RegisterRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/register`, request).pipe(
      tap(response => this.storeAuthResponse(response)),
      catchError(this.handleError)
    );
  }

  logout(): void {
    localStorage.removeItem(AUTH_TOKEN_KEY);
    localStorage.removeItem(REFRESH_TOKEN_KEY);
    localStorage.removeItem(CURRENT_USER_KEY);
    this.currentUserSubject.next(null);
    this.router.navigate(['/login']);
  }

  // Déconnexion initiée par l'utilisateur : révoque le refresh token côté serveur
  // avant de vider la session locale (contrairement à logout(), utilisé en interne
  // par l'intercepteur/le refresh en échec, où le token est de toute façon déjà invalide).
  logoutAndRevoke(): void {
    this.http.post(`${this.apiUrl}/logout`, {}).subscribe({
      next: () => this.logout(),
      error: () => this.logout()
    });
  }

  getToken(): string | null {
    return localStorage.getItem(AUTH_TOKEN_KEY);
  }

  getRefreshToken(): string | null {
    return localStorage.getItem(REFRESH_TOKEN_KEY);
  }

  // Decode JWT payload and return expiry (seconds since epoch) if present
  private getTokenExpiry(token: string): number | null {
    try {
      const parts = token.split('.');
      if (parts.length !== 3) return null;
      const payload = JSON.parse(atob(parts[1]));
      return payload.exp ?? null;
    } catch {
      return null;
    }
  }

  isTokenExpired(bufferSeconds = 30): boolean {
    const token = this.getToken();
    if (!token) return true;
    const exp = this.getTokenExpiry(token);
    if (!exp) return true;
    const now = Math.floor(Date.now() / 1000);
    return exp - now <= bufferSeconds;
  }

  isLoggedIn(): boolean {
    return !!this.getToken();
  }

  hasRole(roles: string[]): boolean {
    const userRole = this.currentUserSubject.value?.role;
    if (!userRole) {
      return false;
    }

    return roles.some(role => role.toLowerCase() === userRole.toLowerCase());
  }

  isAdmin(): boolean {
    return this.hasRole(['Admin']);
  }

  private storeAuthResponse(response: AuthResponse): void {
    const user: User = {
      id: response.id,
      email: response.email,
      nom: response.nom,
      prenom: response.prenom,
      role: response.role,
      isActive: true, // Par défaut, on considère que l'utilisateur est actif
      dateCreation: new Date(), // Date de création actuelle
      fullName: `${response.prenom} ${response.nom}` // Concaténation du prénom et du nom
    };

    localStorage.setItem(AUTH_TOKEN_KEY, response.token);
    if ((response as any).refreshToken) {
      localStorage.setItem(REFRESH_TOKEN_KEY, (response as any).refreshToken);
    }
    localStorage.setItem(CURRENT_USER_KEY, JSON.stringify(user));
    this.currentUserSubject.next(user);
  }

  // Attempt to refresh JWT using refresh token or current token
  refreshToken(): Observable<boolean> {
    const refreshToken = this.getRefreshToken();
    if (!refreshToken) {
      return of(false);
    }

    return this.http.post<AuthResponse>(`${this.apiUrl}/refresh`, { refreshToken }).pipe(
      tap(response => this.storeAuthResponse(response)),
      map(() => true),
      catchError(err => {
        console.warn('Refresh token failed', err);
        this.logout();
        return of(false);
      })
    );
  }

  private handleError(error: HttpErrorResponse): Observable<never> {
    console.error('AuthService error:', error);
    const message = error.error?.error || error.message || 'Une erreur réseau est survenue';
    return throwError(() => new Error(message));
  }
}
