import { Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { catchError, throwError, Observable } from 'rxjs';
import { Commande } from '../models/cart.model';
import { API_BASE_URL } from '../config/api.config';

@Injectable({ providedIn: 'root' })
export class CommercialService {
  private apiUrl = `${API_BASE_URL}/Commercial`;

  constructor(private http: HttpClient) {}

  getCommandes(): Observable<Commande[]> {
    return this.http.get<Commande[]>(`${this.apiUrl}/commandes`).pipe(catchError(this.handleError));
  }

  creerCommande(commande: Partial<Commande>): Observable<Commande> {
    return this.http.post<Commande>(`${this.apiUrl}/commandes`, commande).pipe(catchError(this.handleError));
  }

  updateCommandeStatut(id: number, statut: string): Observable<Commande> {
    return this.http.patch<Commande>(`${this.apiUrl}/commandes/${id}/statut`, JSON.stringify(statut), {
      headers: { 'Content-Type': 'application/json' },
    }).pipe(catchError(this.handleError));
  }

  getChiffreAffaires(): Observable<{ total: number; mois: number; annee: number }> {
    return this.http.get<{ total: number; mois: number; annee: number }>(`${this.apiUrl}/chiffre-affaires`).pipe(catchError(this.handleError));
  }

  private handleError(error: HttpErrorResponse): Observable<never> {
    console.error('CommercialService error:', error);
    const message = error.error?.error || error.message || 'Une erreur réseau est survenue';
    return throwError(() => new Error(message));
  }
}
