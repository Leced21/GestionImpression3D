import { Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { catchError, throwError, Observable } from 'rxjs';
import { Projet, ProjetPiece, ProjetStats } from '../models/projet.model';
import { API_BASE_URL } from '../config/api.config';

@Injectable({ providedIn: 'root' })
export class ProjetService {
  private apiUrl = `${API_BASE_URL}/Projet`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<Projet[]> {
    return this.http.get<Projet[]>(this.apiUrl).pipe(catchError(this.handleError));
  }

  getById(id: number): Observable<Projet> {
    return this.http.get<Projet>(`${this.apiUrl}/${id}`).pipe(catchError(this.handleError));
  }

  create(projet: Partial<Projet>): Observable<Projet> {
    return this.http.post<Projet>(this.apiUrl, projet).pipe(catchError(this.handleError));
  }

  update(id: number, projet: Projet): Observable<Projet> {
    return this.http.put<Projet>(`${this.apiUrl}/${id}`, projet).pipe(catchError(this.handleError));
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`).pipe(catchError(this.handleError));
  }

  ajouterPiece(projetId: number, pieceId: number, quantite: number): Observable<ProjetPiece> {
    return this.http.post<ProjetPiece>(`${this.apiUrl}/${projetId}/pieces`, { pieceId, quantite }).pipe(catchError(this.handleError));
  }

  retirerPiece(projetId: number, pieceId: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${projetId}/pieces/${pieceId}`).pipe(catchError(this.handleError));
  }

  getStats(projetId: number): Observable<ProjetStats> {
    return this.http.get<ProjetStats>(`${this.apiUrl}/${projetId}/stats`).pipe(catchError(this.handleError));
  }

  private handleError(error: HttpErrorResponse) {
    console.error('ProjetService error:', error);
    const message = error.error?.error || error.message || 'Erreur réseau';
    return throwError(() => new Error(message));
  }
}
