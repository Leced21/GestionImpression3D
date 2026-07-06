import { Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { API_BASE_URL } from '../config/api.config';

@Injectable({ providedIn: 'root' })
export class TechnicalPlanService {
  constructor(private http: HttpClient) {}

  /**
   * Télécharge le plan technique PDF d'une pièce
   */
  downloadPieceTechnicalPlan(pieceId: number): void {
    this.http.get(
      `${API_BASE_URL}/Piece/${pieceId}/technical-plan/pdf`,
      { responseType: 'blob' }
    )
    .pipe(
      catchError(this.handleError)
    )
    .subscribe(blob => {
      this.downloadFile(blob, `plan-technique-piece-${pieceId}.pdf`);
    });
  }

  /**
   * Télécharge les plans techniques PDF d'un projet complet
   */
  downloadProjectTechnicalPlans(projectId: number): void {
    this.http.get(
      `${API_BASE_URL}/Projet/${projectId}/technical-plans/pdf`,
      { responseType: 'blob' }
    )
    .pipe(
      catchError(this.handleError)
    )
    .subscribe(blob => {
      this.downloadFile(blob, `plans-techniques-projet-${projectId}.pdf`);
    });
  }

  private downloadFile(blob: Blob, filename: string): void {
    const url = window.URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = filename;
    link.click();
    window.URL.revokeObjectURL(url);
  }

  private handleError(error: HttpErrorResponse) {
    console.error('TechnicalPlanService error:', error);
    const message = error.error?.error || error.message || 'Erreur lors de la génération du plan';
    return throwError(() => new Error(message));
  }
}
