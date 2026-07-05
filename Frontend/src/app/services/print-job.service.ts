import { Injectable } from '@angular/core';
import { API_BASE_URL } from '../config/api.config';
import { HttpClient } from '@angular/common/http';
import { CreatePrintJobRequest, PrintJob, PrintJobStatistics } from '../models/print-job.model';
import { Observable } from 'rxjs/internal/Observable';

@Injectable({
  providedIn: 'root',
})
export class PrintJobService {
  private apiUrl = `${API_BASE_URL}/PrintJob`;
  constructor(
    private http: HttpClient
  ) {}

  getAll(): Observable<PrintJob[]> {
    return this.http.get<PrintJob[]>(this.apiUrl);
  }

  getStatistics(): Observable<PrintJobStatistics> {
    return this.http.get<PrintJobStatistics>(`${this.apiUrl}/statistics`);
  }

  getById(id: number): Observable<PrintJob> {
    return this.http.get<PrintJob>(`${this.apiUrl}/${id}`);
  }

  create(request  : CreatePrintJobRequest): Observable<PrintJob> {
    return this.http.post<PrintJob>(this.apiUrl, request);
  }

  assignPrinter(id: number, printerId: number, operatorId?: number): Observable<PrintJob> {
    return this.http.patch<PrintJob>(`${this.apiUrl}/${id}/assign`, { printerId, operatorId });
  }

  start(id: number): Observable<PrintJob> {
    return this.http.post<PrintJob>(`${this.apiUrl}/${id}/start`, {});
  }

  pause(id: number): Observable<PrintJob> {
    return this.http.post<PrintJob>(`${this.apiUrl}/${id}/pause`, {});
  }

  resume(id: number): Observable<PrintJob> {
    return this.http.post<PrintJob>(`${this.apiUrl}/${id}/resume`, {});
  }

  complete(id: number, actualDurationMinutes?: number, actualMaterialGrams?: number): Observable<PrintJob> {
    return this.http.post<PrintJob>(`${this.apiUrl}/${id}/complete`, { actualDurationMinutes, actualMaterialGrams });
  }

  fail(id: number, reason: string): Observable<PrintJob> {
    return this.http.post<PrintJob>(`${this.apiUrl}/${id}/fail`, JSON.stringify(reason), {
      headers: { 'Content-Type': 'application/json' },
    });
  }
  cancel(id: number): Observable<PrintJob> {
    return this.http.post<PrintJob>(`${this.apiUrl}/${id}/cancel`, {});
  }
  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

}
