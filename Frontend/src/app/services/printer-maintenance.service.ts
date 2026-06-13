import { Injectable } from '@angular/core';
import { API_BASE_URL } from '../config/api.config';
import { HttpClient } from '@angular/common/http';
import { MaintenanceStatistics, PrinterMaintenance } from '../models/maintenance.model';
import { Observable } from 'rxjs/internal/Observable';

@Injectable({
  providedIn: 'root',
})
export class PrinterMaintenanceService {
  private apiUrl = `${API_BASE_URL}/printermaintenance`;
  constructor(private http: HttpClient) { }

  getAll(): Observable<PrinterMaintenance[]> {
    return this.http.get<PrinterMaintenance[]>(this.apiUrl);
  }
  getById(id: number): Observable<PrinterMaintenance> {
    return this.http.get<PrinterMaintenance>(`${this.apiUrl}/${id}`);
  }
  getUpcoming(days: number = 7): Observable<PrinterMaintenance[]> {
    return this.http.get<PrinterMaintenance[]>(`${this.apiUrl}/upcoming?days=${days}`);
  }

  getByPrinter(printerId: number): Observable<PrinterMaintenance[]> {
    return this.http.get<PrinterMaintenance[]>(`${this.apiUrl}/printer/${printerId}`);
  }

  getStatistics(printerId: number): Observable<MaintenanceStatistics> {
    return this.http.get<MaintenanceStatistics>(`${this.apiUrl}/printer/${printerId}/statistics`);
  }

  create(maintenance: Partial<PrinterMaintenance>): Observable<PrinterMaintenance> {
    return this.http.post<PrinterMaintenance>(this.apiUrl, maintenance);
  }

  update(id: number, maintenance: Partial<PrinterMaintenance>): Observable<PrinterMaintenance> {
    return this.http.put<PrinterMaintenance>(`${this.apiUrl}/${id}`, maintenance);
  }

  complete(id: number, notes?: string, performedBy?: string): Observable<PrinterMaintenance> {
    return this.http.post<PrinterMaintenance>(`${this.apiUrl}/${id}/complete`, { notes, performedBy });
  }

  cancel(id: number): Observable<PrinterMaintenance> {
    return this.http.post<PrinterMaintenance>(`${this.apiUrl}/${id}/cancel`, {});
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
