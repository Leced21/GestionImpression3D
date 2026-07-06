import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { API_BASE_URL } from '../config/api.config';
import { Observable } from 'rxjs/internal/Observable';

@Injectable({
  providedIn: 'root',
})
export class ExportService {
  private apiUrl = `${API_BASE_URL}`;

  constructor(private http: HttpClient) {}

  exportProjetPdf(projectId: number): Observable<Blob> {
    return this.http.get(`${this.apiUrl}/Projet/${projectId}/pdf`, { 
      responseType: 'blob' 
    });
  }

  exportDevisPdf(projectId: number): Observable<Blob> {
    return this.http.get(`${this.apiUrl}/Projet/${projectId}/devis`, { 
      responseType: 'blob' 
    });
  }

  exportPiecePdf(pieceId: number): Observable<Blob> {
    return this.http.get(`${this.apiUrl}/Piece/${pieceId}/pdf`, {
      responseType: 'blob'
    });
  }

  exportFicheProduitPdf(pieceId: number): Observable<Blob> {
    return this.http.get(`${this.apiUrl}/Piece/${pieceId}/fiche-produit-pdf`, {
      responseType: 'blob'
    });
  }

  downloadPdf(blob: Blob, filename: string): void {
    const url = window.URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = filename;
    a.click();
    window.URL.revokeObjectURL(url);
  }

  // Export Excel
  exportPiecesExcel(): Observable<Blob> {
    return this.http.get(`${this.apiUrl}/Piece/export/excel`, { responseType: 'blob' });
  }

  exportProjetsExcel(): Observable<Blob> {
    return this.http.get(`${this.apiUrl}/projets/export/excel`, { responseType: 'blob' });
  }

  exportPrintJobsExcel(): Observable<Blob> {
    return this.http.get(`${this.apiUrl}/printjobs/export/excel`, { responseType: 'blob' });
  }

  exportCommandesExcel(): Observable<Blob> {
    return this.http.get(`${this.apiUrl}/commercial/commandes/export/excel`, { responseType: 'blob' });
  }

  exportStockExcel(): Observable<Blob> {
    return this.http.get(`${this.apiUrl}/materialstock/export/excel`, { responseType: 'blob' });
  }

  downloadFile(blob: Blob, fileName: string): void {
    const url = window.URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = fileName;
    link.click();
    window.URL.revokeObjectURL(url);
  }
}
