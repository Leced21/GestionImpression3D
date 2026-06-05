import { Injectable } from '@angular/core';
import { API_BASE_URL } from '../config/api.config';
import { Observable } from 'rxjs/internal/Observable';
import { HttpClient } from '@angular/common/http';
import { AppNotification } from '../models/app-notification.model';

@Injectable({
  providedIn: 'root',
})
export class NotificationManagerService {
  private apiUrl = `${API_BASE_URL}/Notifications`;
  constructor(private http: HttpClient) {}

  getNotifications(): Observable<AppNotification[]> {
    return this.http.get<AppNotification[]>(this.apiUrl);
  }

  getUnreadCount(): Observable<{ count: number }> {
    return this.http.get<{ count: number }>(`${this.apiUrl}/count`);
  }

  markAsRead(id: number): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${id}/read`, {});
  }

  markAllAsRead(): Observable<{ count: number }> {
    return this.http.post<{ count: number }>(`${this.apiUrl}/read-all`, {});
  }

  deleteNotification(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  deleteAll(): Observable<{ success: boolean }> {
    return this.http.delete<{ success: boolean }>(`${this.apiUrl}/all`);
  }
}
