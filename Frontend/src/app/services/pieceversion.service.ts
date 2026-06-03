import { Injectable } from '@angular/core';
import { API_BASE_URL } from '../config/api.config';
import { HttpClient } from '@angular/common/http';
import { PieceVersion } from '../models/piece-version.model';
import { Observable } from 'rxjs/internal/Observable';

@Injectable({
  providedIn: 'root',
})
export class PieceVersionService {
  private apiUrl = `${API_BASE_URL}/PieceVersions`;
  constructor(private http: HttpClient) { }

  getVersionsByPiece(pieceId: number): Observable<PieceVersion[]> {
    return this.http.get<PieceVersion[]>(`${this.apiUrl}/piece/${pieceId}`);
  }

  getVersion(id: number): Observable<PieceVersion> {
    return this.http.get<PieceVersion>(`${this.apiUrl}/${id}`);
  }

  createVersion(pieceId: number, changeLog: string, isPrototype: boolean): Observable<PieceVersion> {
    return this.http.post<PieceVersion>(`${this.apiUrl}/piece/${pieceId}`, { changeLog, isPrototype });
  }

  promoteToProduction(versionId: number): Observable<PieceVersion> {
    return this.http.post<PieceVersion>(`${this.apiUrl}/${versionId}/promote`, {});
  }

  compareVersions(v1Id: number, v2Id: number): Observable<{ hasChanges: boolean }> {
    return this.http.get<{ hasChanges: boolean }>(`${this.apiUrl}/compare?v1=${v1Id}&v2=${v2Id}`);
  }
}
