import { Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse, HttpHeaders } from '@angular/common/http';
import { catchError, throwError, Observable } from 'rxjs';
import { Piece } from '../models/piece.model';
import { DashboardStat } from '../models/dashboardstat';
import { API_BASE_URL } from '../config/api.config';

export interface UploadResponse {
  fileName: string;
  filePath: string;
  size: number;
}

@Injectable({
  providedIn: 'root',
})
export class PieceService {
  private apiUrl = `${API_BASE_URL}/Piece`;

  constructor(private http: HttpClient) { }

  getAll(): Observable<Piece[]> {
    return this.http.get<Piece[]>(this.apiUrl).pipe(catchError(this.handleError));
  }

  getById(id: number): Observable<Piece> {
    return this.http.get<Piece>(`${this.apiUrl}/${id}`).pipe(catchError(this.handleError));
  }

  create(piece: Partial<Piece>): Observable<Piece> {
    return this.http.post<Piece>(this.apiUrl, piece).pipe(catchError(this.handleError));
  }

  updateStatus(id: number, statut: string): Observable<Piece> {
    const headers = new HttpHeaders({ 'Content-Type': 'application/json' });
    return this.http.patch<Piece>(`${this.apiUrl}/${id}/statut`, JSON.stringify(statut), { headers }).pipe(catchError(this.handleError));
  }

  update(id: number, piece: Piece): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}`, piece).pipe(catchError(this.handleError));
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`).pipe(catchError(this.handleError));
  }

  getPrixRecommande(id: number): Observable<number> {
    return this.http.get<number>(`${this.apiUrl}/${id}/prix-recommande`).pipe(catchError(this.handleError));
  }

  getDashboardStats(): Observable<DashboardStat> {
    return this.http.get<DashboardStat>(`${this.apiUrl}/dashboard/stats`).pipe(catchError(this.handleError));
  }

  uploadStl(id: number, file: File): Observable<UploadResponse> {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post<UploadResponse>(`${this.apiUrl}/${id}/upload-stl`, formData).pipe(catchError(this.handleError));
  }

  getStlUrl(id: number): string {
    return `${this.apiUrl}/${id}/stl`;
  }

  private handleError(error: HttpErrorResponse): Observable<never> {
    console.error('PieceService error:', error);
    const message = error.error?.error || error.message || 'Une erreur réseau est survenue';
    return throwError(() => new Error(message));
  }
}
