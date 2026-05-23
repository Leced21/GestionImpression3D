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

  downloadPdf(blob: Blob, filename: string): void {
    const url = window.URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = filename;
    a.click();
    window.URL.revokeObjectURL(url);
  }
}
