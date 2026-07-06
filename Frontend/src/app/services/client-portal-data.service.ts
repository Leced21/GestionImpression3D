import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../config/api.config';
import { PortalCommande, PortalDevis, PortalFacture } from '../models/client-portal.model';

@Injectable({ providedIn: 'root' })
export class ClientPortalDataService {
  private apiUrl = `${API_BASE_URL}/client-portal`;

  constructor(private http: HttpClient) {}

  getDevis(): Observable<PortalDevis[]> {
    return this.http.get<PortalDevis[]>(`${this.apiUrl}/devis`);
  }

  getFactures(): Observable<PortalFacture[]> {
    return this.http.get<PortalFacture[]>(`${this.apiUrl}/factures`);
  }

  getCommandes(): Observable<PortalCommande[]> {
    return this.http.get<PortalCommande[]>(`${this.apiUrl}/commandes`);
  }

  downloadFacturePdf(id: number, numeroFacture: string): void {
    this.http.get(`${this.apiUrl}/factures/${id}/pdf`, { responseType: 'blob' }).subscribe({
      next: (blob) => {
        const url = window.URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = `Facture_${numeroFacture}.pdf`;
        link.click();
        window.URL.revokeObjectURL(url);
      },
      error: (err) => console.error('Erreur lors du téléchargement du PDF', err),
    });
  }
}
