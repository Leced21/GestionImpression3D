import { Injectable } from "@angular/core";
import { API_BASE_URL } from "../config/api.config";
import { HttpClient } from "@angular/common/http";
import { AuditLog, EntityType } from "../models/audit-log.model";
import { Observable } from "rxjs/internal/Observable";

@Injectable({ providedIn: 'root' })
export class AuditLogApiService {
    private apiUrl = `${API_BASE_URL}/admin`;

    constructor(
        private http: HttpClient
    ) { }
    getRecentLogs(limit: number = 100): Observable<AuditLog[]> {
        return this.http.get<AuditLog[]>(`${this.apiUrl}/audit-logs?limit=${limit}`);
    }
    getLogsByEntity(entityType: EntityType, entityId: number): Observable<AuditLog[]> {
        return this.http.get<AuditLog[]>(`${this.apiUrl}/audit-logs?entityType=${entityType}&entityId=${entityId}`);
    }
    getLogsByUser(userId: number): Observable<AuditLog[]> {
        return this.http.get<AuditLog[]>(`${this.apiUrl}/audit-logs?userId=${userId}`);
    }
    getLogsByDateRange(startDate: Date, endDate: Date): Observable<AuditLog[]> {
        const start = startDate.toISOString();
        const end = endDate.toISOString();
        return this.http.get<AuditLog[]>(`${this.apiUrl}/audit-logs?startDate=${start}&endDate=${end}`);
    }
}
