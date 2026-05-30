import { Injectable } from '@angular/core';
import { API_BASE_URL } from '../config/api.config';
import { HttpClient } from '@angular/common/http';
import { Printer, PrinterStatistics } from '../models/printer.model';
import { Observable } from 'rxjs/internal/Observable';

@Injectable({
  providedIn: 'root',
})
export class PrinterService {
  private apiUrl = `${API_BASE_URL}/Printer`;
  constructor(
    private http: HttpClient
  ) {}
  getAll(): Observable<Printer[]> {
    return this.http.get<Printer[]>(this.apiUrl);
  }

  getStatistics(): Observable<PrinterStatistics> {
    return this.http.get<PrinterStatistics>(`${this.apiUrl}/statistics`);
  }

  getById(id: number): Observable<Printer> {
    return this.http.get<Printer>(`${this.apiUrl}/${id}`);
  }

  create(printer: Partial<Printer>): Observable<Printer> {
    return this.http.post<Printer>(this.apiUrl, printer);
  }
  update(id: number, printer: Partial<Printer>): Observable<Printer> {
    return this.http.put<Printer>(`${this.apiUrl}/${id}`, printer);
  }
  updateStatus(id: number, status: string): Observable<Printer> {
    return this.http.patch<Printer>(`${this.apiUrl}/${id}/status`, { status });
  }
  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
