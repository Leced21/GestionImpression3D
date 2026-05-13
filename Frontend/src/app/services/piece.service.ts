import { Injectable } from '@angular/core';
import { Observable } from 'rxjs/internal/Observable';
import { Piece } from '../models/piece.model';
import { HttpClient } from '@angular/common/http';

@Injectable({
  providedIn: 'root',
})
export class PieceService {
  private apiUrl = 'http://localhost:7096/api/Pieces';

  constructor(private http: HttpClient) {}

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
    return this.http.patch<Piece>(`${this.apiUrl}/${id}`, { statut });
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  getPrixRecommande(id: number): Observable<number> {
    return this.http.get<number>(`${this.apiUrl}/${id}/prix-recommande`);
  }
}
