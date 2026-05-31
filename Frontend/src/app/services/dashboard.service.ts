import { Injectable } from '@angular/core';
import { API_BASE_URL } from '../config/api.config';
import { HttpClient } from '@angular/common/http';
import { GlobalStats, MaterialConsumption, PrinterActivity, ProductionTrend } from '../models/dashboard.model';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class DashboardService {
  private apiUrl = `${API_BASE_URL}/dashboard`;

  constructor(private http: HttpClient){}

    getGlobalStats(): Observable<GlobalStats> {
    return this.http.get<GlobalStats>(`${this.apiUrl}/stats`);
  }

  getProductionTrend(days: number = 30): Observable<ProductionTrend[]> {
    return this.http.get<ProductionTrend[]>(`${this.apiUrl}/production-trend?days=${days}`);
  }

  getMaterialConsumption(days: number = 30): Observable<MaterialConsumption[]> {
    return this.http.get<MaterialConsumption[]>(`${this.apiUrl}/material-consumption?days=${days}`);
  }

  getPrintersActivity(): Observable<PrinterActivity[]> {
    return this.http.get<PrinterActivity[]>(`${this.apiUrl}/printers-activity`);
  }
}
