import { Injectable } from '@angular/core';
import { Observable } from 'rxjs/internal/Observable';
import { PrintProfile } from '../models/print-profile.model';
import { API_BASE_URL } from '../config/api.config';
import { HttpClient } from '@angular/common/http';

@Injectable({
  providedIn: 'root',
})
export class PrintProfileService {
  private apiUrl = `${API_BASE_URL}/printprofiles`;
    constructor(private http: HttpClient) {}

  getAll(): Observable<PrintProfile[]> {
    return this.http.get<PrintProfile[]>(this.apiUrl);
  }

  getById(id: number): Observable<PrintProfile> {
    return this.http.get<PrintProfile>(`${this.apiUrl}/${id}`);
  }

  getByPrinter(printerId: number): Observable<PrintProfile[]> {
    return this.http.get<PrintProfile[]>(`${this.apiUrl}/printer/${printerId}`);
  }

  create(profile: Partial<PrintProfile>): Observable<PrintProfile> {
    return this.http.post<PrintProfile>(this.apiUrl, profile);
  }

  update(id: number, profile: Partial<PrintProfile>): Observable<PrintProfile> {
    return this.http.put<PrintProfile>(`${this.apiUrl}/${id}`, profile);
  }

  setDefault(id: number): Observable<PrintProfile> {
    return this.http.post<PrintProfile>(`${this.apiUrl}/${id}/set-default`, {});
  }

  duplicate(id: number, newName: string): Observable<PrintProfile> {
    return this.http.post<PrintProfile>(`${this.apiUrl}/${id}/duplicate`, newName);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
