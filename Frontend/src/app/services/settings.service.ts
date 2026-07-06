import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs/internal/Observable';
import { API_BASE_URL } from '../config/api.config';

@Injectable({
  providedIn: 'root',
})
export class SettingsService  {
  private apiUrl = `${API_BASE_URL}/Settings`
  constructor(private http: HttpClient) {}

  getSettings(): Observable<any> {
    return this.http.get(`${this.apiUrl}`);
  }

  saveSettings(settings: any): Observable<any> {
    return this.http.post(`${this.apiUrl}`, settings);
  }

  toggleTwoFactor(): Observable<{ enabled: boolean }> {
    return this.http.post<{ enabled: boolean }>(`${this.apiUrl}/toggle-2fa`, {});
  }

  getSystemInfo(): Observable<any> {
    return this.http.get(`${this.apiUrl}/system-info`);
  }
}
