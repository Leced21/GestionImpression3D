import { Injectable } from '@angular/core';
import { Observable } from 'rxjs/internal/Observable';
import { Piece } from '../models/piece.model';
import { HttpClient } from '@angular/common/http';
import { DashboardStat } from '../models/dashboardstat';

@Injectable({
  providedIn: 'root',
})
export class PieceService {
  private apiUrl = 'https://localhost:7096/api/Piece';

  constructor(private http: HttpClient) { }

  getAll(): Observable<Piece[]> {
    return this.http.get<Piece[]>(this.apiUrl);
  }

  getById(id: number): Observable<Piece> {
    return this.http.get<Piece>(`${this.apiUrl}/${id}`);
  }

  create(piece: Partial<Piece>): Observable<Piece> {
    return this.http.post<Piece>(this.apiUrl, piece);
  }

  updateStatus(id: number, statut: string): Observable<Piece> {
    return this.http.patch<Piece>(`${this.apiUrl}/${id}/statut`, JSON.stringify(statut), { headers: { 'Content-Type': 'application/json' } });
  }

  update(id: number, piece: Piece): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}`, piece);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  getPrixRecommande(id: number): Observable<number> {
    return this.http.get<number>(`${this.apiUrl}/${id}/prix-recommande`);
  }
  getDashboardStats(): Observable<DashboardStat> {
    return this.http.get<DashboardStat>(`${this.apiUrl}/dashboard/stats`);
  }
  // services/piece.service.ts - Ajouter ces méthodes

  uploadStl(id: number, file: File): Observable<any> {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post(`${this.apiUrl}/${id}/upload-stl`, formData);
  }

  getStlUrl(id: number): string {
    return `${this.apiUrl}/${id}/stl`;
  }
}
