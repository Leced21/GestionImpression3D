import { Injectable } from '@angular/core';
import { API_BASE_URL } from '../config/api.config';
import { HttpClient } from '@angular/common/http';
import { CreateMaterialStockRequest, MaterialStatistics, MaterialStock, UpdateStockRequest } from '../models/material-stock.model';
import { Observable } from 'rxjs/internal/Observable';

@Injectable({
  providedIn: 'root',
})
export class MaterialStockService {
  private apiUrl = `${API_BASE_URL}/MaterialStock`;
  constructor(
    private http: HttpClient
  ) {}
  getAll(): Observable<MaterialStock[]> {
    return this.http.get<MaterialStock[]>(this.apiUrl);
  }
  getStatistics(): Observable<MaterialStatistics> {
    return this.http.get<MaterialStatistics>(`${this.apiUrl}/statistics`);
  }
    getLowStockAlerts(): Observable<MaterialStock[]> {
    return this.http.get<MaterialStock[]>(`${this.apiUrl}/alerts/low-stock`);
  }

  getById(id: number): Observable<MaterialStock> {
    return this.http.get<MaterialStock>(`${this.apiUrl}/${id}`);
  }

  create(request: CreateMaterialStockRequest): Observable<MaterialStock> {
    return this.http.post<MaterialStock>(this.apiUrl, request);
  }

  addStock(id: number, request: UpdateStockRequest): Observable<MaterialStock> {
    return this.http.post<MaterialStock>(`${this.apiUrl}/${id}/add`, request);
  }

  removeStock(id: number, request: UpdateStockRequest): Observable<MaterialStock> {
    return this.http.post<MaterialStock>(`${this.apiUrl}/${id}/remove`, request);
  }

  updateThresholds(id: number, minThreshold: number, maxThreshold: number): Observable<MaterialStock> {
    return this.http.patch<MaterialStock>(`${this.apiUrl}/${id}/thresholds`, { minThreshold, maxThreshold });
  }

  updatePrice(id: number, unitPrice: number): Observable<MaterialStock> {
    return this.http.patch<MaterialStock>(`${this.apiUrl}/${id}/price`, { unitPrice });
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
