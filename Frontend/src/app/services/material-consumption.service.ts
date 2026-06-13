import { Injectable } from '@angular/core';
import { API_BASE_URL } from '../config/api.config';
import { HttpClient } from '@angular/common/http';
import { ConsumptionStatistics, MaterialConsumption } from '../models/consumption.model';
import { Observable } from 'rxjs/internal/Observable';

@Injectable({
  providedIn: 'root',
})
export class MaterialConsumptionService {
  private apiUrl = `${API_BASE_URL}/materialconsumption`;

  constructor(private http: HttpClient) {}
  getAll(): Observable<MaterialConsumption[]> {
    return this.http.get<MaterialConsumption[]>(this.apiUrl);
  }
    getStatistics(start?: Date, end?: Date): Observable<ConsumptionStatistics> {
    let url = `${this.apiUrl}/statistics`;
    if (start || end) {
      const params = new URLSearchParams();
      if (start) params.append('start', start.toISOString());
      if (end) params.append('end', end.toISOString());
      url += `?${params.toString()}`;
    }
    return this.http.get<ConsumptionStatistics>(url);
  }

  getByMaterial(materialId: number): Observable<MaterialConsumption[]> {
    return this.http.get<MaterialConsumption[]>(`${this.apiUrl}/material/${materialId}`);
  }

  create(consumption: Partial<MaterialConsumption>): Observable<MaterialConsumption> {
    return this.http.post<MaterialConsumption>(this.apiUrl, consumption);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
