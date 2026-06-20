import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs/internal/Observable';
import { API_BASE_URL } from '../config/api.config';
import { of } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class SettingsService  {
  private apiUrl = `${API_BASE_URL}/Settings`
  constructor(private http: HttpClient) {}

  getSettings(): Observable<any> {
    // En attendant l'API, retourner des valeurs par défaut
    return of({
      language: 'fr',
      timezone: 'Europe/Paris',
      dateFormat: 'DD/MM/YYYY',
      theme: 'light',
      primaryColor: '#3b82f6',
      emailNotifications: true,
      stockAlerts: true,
      productionAlerts: true,
      weeklyReports: false,
      twoFactorEnabled: false
    });
  }

  saveSettings(settings: any): Observable<any> {
    return this.http.post(`${this.apiUrl}`, settings);
  }
}
