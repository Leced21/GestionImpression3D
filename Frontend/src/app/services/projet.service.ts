import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Projet, ProjetPiece, ProjetStats } from '../models/projet.model';
import { API_BASE_URL } from '../config/api.config';

@Injectable({ providedIn: 'root' })
export class ProjetService {
  private apiUrl = `${API_BASE_URL}/projet`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<Projet[]> {
    return this.http.get<Projet[]>(this.apiUrl);
  }

  getById(id: number): Observable<Projet> {
    return this.http.get<Projet>(`${this.apiUrl}/${id}`);
  }

  create(projet: Partial<Projet>): Observable<Projet> {
    return this.http.post<Projet>(this.apiUrl, projet);
  }

  update(id: number, projet: Projet): Observable<Projet> {
    return this.http.put<Projet>(`${this.apiUrl}/${id}`, projet);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  ajouterPiece(projetId: number, pieceId: number, quantite: number): Observable<ProjetPiece> {
    return this.http.post<ProjetPiece>(`${this.apiUrl}/${projetId}/pieces`, { pieceId, quantite });
  }

  retirerPiece(projetId: number, pieceId: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${projetId}/pieces/${pieceId}`);
  }

  getStats(projetId: number): Observable<ProjetStats> {
    return this.http.get<ProjetStats>(`${this.apiUrl}/${projetId}/stats`);
  }
}