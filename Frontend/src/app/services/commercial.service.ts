import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Commande } from '../models/cart.model';

@Injectable({ providedIn: 'root' })
export class CommercialService {
  private apiUrl = 'https://localhost:7096/api/Commercial';

  constructor(private http: HttpClient) {}

  getCommandes(): Observable<Commande[]> {
    return this.http.get<Commande[]>(`${this.apiUrl}/commandes`);
  }

  creerCommande(commande: Partial<Commande>): Observable<Commande> {
    return this.http.post<Commande>(`${this.apiUrl}/commandes`, commande);
  }

  updateCommandeStatut(id: number, statut: string): Observable<Commande> {
    return this.http.patch<Commande>(`${this.apiUrl}/commandes/${id}/statut`, { statut });
  }

  getChiffreAffaires(): Observable<{ total: number; mois: number; annee: number }> {
    return this.http.get<{ total: number; mois: number; annee: number }>(`${this.apiUrl}/chiffre-affaires`);
  }
}