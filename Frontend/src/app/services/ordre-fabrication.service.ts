import { Injectable } from '@angular/core';
import { API_BASE_URL } from '../config/api.config';
import { Observable } from 'rxjs/internal/Observable';
import { OrdreFabrication, OrdreStatistics } from '../models/ordre-fabrication.model';
import { HttpClient } from '@angular/common/http';

@Injectable({
  providedIn: 'root',
})
export class OrdreFabricationService {
  private apiUrl = `${API_BASE_URL}/ordresfabrication`;
  constructor(private http: HttpClient) {}

  getAll(): Observable<OrdreFabrication[]> {
    return this.http.get<OrdreFabrication[]>(this.apiUrl);
  }

  getStatistics(): Observable<OrdreStatistics> {
    return this.http.get<OrdreStatistics>(`${this.apiUrl}/statistics`);
  }

  getById(id: number): Observable<OrdreFabrication> {
    return this.http.get<OrdreFabrication>(`${this.apiUrl}/${id}`);
  }

  create(ordre: Partial<OrdreFabrication>): Observable<OrdreFabrication> {
    return this.http.post<OrdreFabrication>(this.apiUrl, ordre);
  }

  update(id: number, ordre: Partial<OrdreFabrication>): Observable<OrdreFabrication> {
    return this.http.put<OrdreFabrication>(`${this.apiUrl}/${id}`, ordre);
  }

  updateStatut(id: number, statut: string): Observable<OrdreFabrication> {
    return this.http.patch<OrdreFabrication>(`${this.apiUrl}/${id}/statut`, statut);
  }

  startProduction(id: number): Observable<OrdreFabrication> {
    return this.http.post<OrdreFabrication>(`${this.apiUrl}/${id}/start`, {});
  }

  completeProduction(id: number): Observable<OrdreFabrication> {
    return this.http.post<OrdreFabrication>(`${this.apiUrl}/${id}/complete`, {});
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
