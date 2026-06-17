import { Injectable } from '@angular/core';
import { API_BASE_URL } from '../config/api.config';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs/internal/Observable';
import { IncidentStatistics, IncidentStatus, PrintIncident } from '../models/print-incident.model';

@Injectable({
  providedIn: 'root',
})
export class PrintIncidentService {
  private apiUrl = `${API_BASE_URL}/PrintIncident`;
  constructor(
    private http: HttpClient
  ) { }
  getAll(): Observable<PrintIncident[]> {
    return this.http.get<PrintIncident[]>(this.apiUrl);
  }

  getStatistics(start?: Date, end?: Date): Observable<IncidentStatistics> {
    let url = `${this.apiUrl}/statistics`;
    const params = new URLSearchParams();
    if (start) params.append('start', start.toISOString());
    if (end) params.append('end', end.toISOString());
    if (params.toString()) url += `?${params.toString()}`;
    return this.http.get<IncidentStatistics>(url);
  }

  getByPrinter(printerId: number): Observable<PrintIncident[]> {
    return this.http.get<PrintIncident[]>(`${this.apiUrl}/printer/${printerId}`);
  }

  getById(id: number): Observable<PrintIncident> {
    return this.http.get<PrintIncident>(`${this.apiUrl}/${id}`);
  }

  create(incident: Partial<PrintIncident>): Observable<PrintIncident> {
    return this.http.post<PrintIncident>(this.apiUrl, incident);
  }
  update(id: number, incident: Partial<PrintIncident>): Observable<PrintIncident> {
    return this.http.put<PrintIncident>(`${this.apiUrl}/${id}`, incident);
  }

  updateStatus(id: number, status: IncidentStatus): Observable<PrintIncident> {
    return this.http.patch<PrintIncident>(`${this.apiUrl}/${id}/status`, status);
  }

  resolve(id: number, resolution: string): Observable<PrintIncident> {
    return this.http.post<PrintIncident>(`${this.apiUrl}/${id}/resolve`, { resolution });
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
