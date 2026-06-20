import { Injectable } from '@angular/core';
import { API_BASE_URL } from '../config/api.config';
import { HttpClient } from '@angular/common/http';
import { MaintenanceStatistics, MaintenanceStatus, MaintenanceType, PrinterMaintenance } from '../models/maintenance.model';
import { Observable } from 'rxjs/internal/Observable';
import { map } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class PrinterMaintenanceService {
  private apiUrl = `${API_BASE_URL}/printermaintenance`;
  constructor(private http: HttpClient) { }

  getAll(): Observable<PrinterMaintenance[]> {
    return this.http.get<PrinterMaintenance[]>(this.apiUrl).pipe(
      map(maintenances => maintenances.map(maintenance => this.normalizeMaintenance(maintenance)))
    );
  }
  getById(id: number): Observable<PrinterMaintenance> {
    return this.http.get<PrinterMaintenance>(`${this.apiUrl}/${id}`).pipe(
      map(maintenance => this.normalizeMaintenance(maintenance))
    );
  }
  getUpcoming(days: number = 7): Observable<PrinterMaintenance[]> {
    return this.http.get<PrinterMaintenance[]>(`${this.apiUrl}/upcoming?days=${days}`).pipe(
      map(maintenances => maintenances.map(maintenance => this.normalizeMaintenance(maintenance)))
    );
  }

  getByPrinter(printerId: number): Observable<PrinterMaintenance[]> {
    return this.http.get<PrinterMaintenance[]>(`${this.apiUrl}/printer/${printerId}`).pipe(
      map(maintenances => maintenances.map(maintenance => this.normalizeMaintenance(maintenance)))
    );
  }

  getStatistics(printerId: number): Observable<MaintenanceStatistics> {
    return this.http.get<MaintenanceStatistics>(`${this.apiUrl}/printer/${printerId}/statistics`);
  }

  create(maintenance: Partial<PrinterMaintenance>): Observable<PrinterMaintenance> {
    return this.http.post<PrinterMaintenance>(this.apiUrl, maintenance).pipe(
      map(created => this.normalizeMaintenance(created))
    );
  }

  update(id: number, maintenance: Partial<PrinterMaintenance>): Observable<PrinterMaintenance> {
    return this.http.put<PrinterMaintenance>(`${this.apiUrl}/${id}`, maintenance).pipe(
      map(updated => this.normalizeMaintenance(updated))
    );
  }

  complete(id: number, notes?: string, performedBy?: string): Observable<PrinterMaintenance> {
    return this.http.post<PrinterMaintenance>(`${this.apiUrl}/${id}/complete`, { notes, performedBy }).pipe(
      map(maintenance => this.normalizeMaintenance(maintenance))
    );
  }

  cancel(id: number): Observable<PrinterMaintenance> {
    return this.http.post<PrinterMaintenance>(`${this.apiUrl}/${id}/cancel`, {}).pipe(
      map(maintenance => this.normalizeMaintenance(maintenance))
    );
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  private normalizeMaintenance(maintenance: any): PrinterMaintenance {
    return {
      ...maintenance,
      printerName: maintenance.printerName ?? maintenance.printer?.nom ?? maintenance.printer?.name ?? '',
      type: this.normalizeType(maintenance.type),
      status: this.normalizeStatus(maintenance.status),
      scheduledDate: maintenance.scheduledDate ? new Date(maintenance.scheduledDate) : maintenance.scheduledDate,
      completedDate: maintenance.completedDate ? new Date(maintenance.completedDate) : maintenance.completedDate,
      createdAt: maintenance.createdAt ? new Date(maintenance.createdAt) : maintenance.createdAt
    };
  }

  private normalizeStatus(status: unknown): MaintenanceStatus {
    const statuses = Object.values(MaintenanceStatus);
    if (typeof status === 'number') return statuses[status - 1] ?? MaintenanceStatus.Scheduled;
    if (typeof status === 'string' && /^\d+$/.test(status)) return statuses[Number(status) - 1] ?? MaintenanceStatus.Scheduled;
    if (statuses.includes(status as MaintenanceStatus)) return status as MaintenanceStatus;
    return MaintenanceStatus.Scheduled;
  }

  private normalizeType(type: unknown): MaintenanceType {
    const types = Object.values(MaintenanceType);
    if (typeof type === 'number') return types[type - 1] ?? MaintenanceType.Preventive;
    if (typeof type === 'string' && /^\d+$/.test(type)) return types[Number(type) - 1] ?? MaintenanceType.Preventive;
    if (types.includes(type as MaintenanceType)) return type as MaintenanceType;
    return MaintenanceType.Preventive;
  }
}
