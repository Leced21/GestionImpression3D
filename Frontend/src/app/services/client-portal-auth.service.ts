import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, tap } from 'rxjs';
import { Router } from '@angular/router';
import { API_BASE_URL, CLIENT_PORTAL_SESSION_KEY } from '../config/api.config';
import { ClientPortalSession } from '../models/client-portal.model';

@Injectable({ providedIn: 'root' })
export class ClientPortalAuthService {
  private apiUrl = `${API_BASE_URL}/client-portal`;
  private sessionSubject = new BehaviorSubject<ClientPortalSession | null>(this.readStoredSession());
  public session$ = this.sessionSubject.asObservable();

  constructor(private http: HttpClient, private router: Router) {}

  private readStoredSession(): ClientPortalSession | null {
    const stored = localStorage.getItem(CLIENT_PORTAL_SESSION_KEY);
    if (!stored) return null;
    try {
      return JSON.parse(stored) as ClientPortalSession;
    } catch {
      localStorage.removeItem(CLIENT_PORTAL_SESSION_KEY);
      return null;
    }
  }

  requestAccess(email: string): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(`${this.apiUrl}/request-access`, { email });
  }

  consume(token: string): Observable<ClientPortalSession> {
    return this.http.post<ClientPortalSession>(`${this.apiUrl}/consume`, { token }).pipe(
      tap((session) => {
        localStorage.setItem(CLIENT_PORTAL_SESSION_KEY, JSON.stringify(session));
        this.sessionSubject.next(session);
      })
    );
  }

  logout(): void {
    localStorage.removeItem(CLIENT_PORTAL_SESSION_KEY);
    this.sessionSubject.next(null);
    this.router.navigate(['/portail/demande-acces']);
  }

  getToken(): string | null {
    return this.sessionSubject.value?.token ?? null;
  }

  getSession(): ClientPortalSession | null {
    return this.sessionSubject.value;
  }

  isLoggedIn(): boolean {
    const session = this.sessionSubject.value;
    if (!session) return false;
    return new Date(session.expiration).getTime() > Date.now();
  }
}
