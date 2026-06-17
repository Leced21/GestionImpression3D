import { Injectable } from '@angular/core';
import { API_BASE_URL } from '../config/api.config';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs/internal/Observable';
import { Devis, DevisStatistics, DevisStatus } from '../models/devis.model';

@Injectable({
  providedIn: 'root',
})
export class DevisService {
  private apiUrl = `${API_BASE_URL}/Devis`;
  constructor(
    private http: HttpClient
  ) { }
  getAll(): Observable<Devis[]> {
    return this.http.get<Devis[]>(this.apiUrl);
  }

  getStatistics(): Observable<DevisStatistics> {
    return this.http.get<DevisStatistics>(`${this.apiUrl}/statistics`);
  }

  getByClient(clientId: number): Observable<Devis[]> {
    return this.http.get<Devis[]>(`${this.apiUrl}/client/${clientId}`);
  }

  getById(id: number): Observable<Devis> {
    return this.http.get<Devis>(`${this.apiUrl}/${id}`);
  }

  create(devis: Partial<Devis>): Observable<Devis> {
    return this.http.post<Devis>(this.apiUrl, devis);
  }
  update(id: number, devis: Partial<Devis>): Observable<Devis> {
    return this.http.put<Devis>(`${this.apiUrl}/${id}`, devis);
  }

  updateStatut(id: number, statut: DevisStatus): Observable<Devis> {
    return this.http.patch<Devis>(`${this.apiUrl}/${id}/statut`, statut);
  }

  generatePdf(id: number): Observable<Blob> {
    return this.http.get(`${this.apiUrl}/${id}/pdf`, { responseType: 'blob' });
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
