import { Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Observable, catchError, throwError } from 'rxjs';
import { API_BASE_URL } from '../config/api.config';
import { PiecePricing, PricingSettings, ProfitabilityDashboard } from '../models/pricing.model';

@Injectable({
  providedIn: 'root'
})
export class PricingService {
  private apiUrl = `${API_BASE_URL}/pricing`;

  constructor(private http: HttpClient) {}

  getSettings(): Observable<PricingSettings> {
    return this.http.get<PricingSettings>(`${this.apiUrl}/settings`).pipe(catchError(this.handleError));
  }

  updateSettings(settings: PricingSettings): Observable<PricingSettings> {
    return this.http.put<PricingSettings>(`${this.apiUrl}/settings`, settings).pipe(catchError(this.handleError));
  }

  getPiecePricing(pieceId: number): Observable<PiecePricing> {
    return this.http.get<PiecePricing>(`${this.apiUrl}/pieces/${pieceId}`).pipe(catchError(this.handleError));
  }

  updatePieceCosting(pieceId: number, costing: Partial<PiecePricing['costing']>): Observable<PiecePricing> {
    return this.http.put<PiecePricing>(`${this.apiUrl}/pieces/${pieceId}`, costing).pipe(catchError(this.handleError));
  }

  getDashboard(): Observable<ProfitabilityDashboard> {
    return this.http.get<ProfitabilityDashboard>(`${this.apiUrl}/dashboard`).pipe(catchError(this.handleError));
  }

  private handleError(error: HttpErrorResponse): Observable<never> {
    const message = error.error?.error || error.message || 'Erreur lors du calcul de rentabilite';
    return throwError(() => new Error(message));
  }
}
