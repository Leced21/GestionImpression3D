import { Injectable } from '@angular/core';
import { API_BASE_URL } from '../config/api.config';
import { Observable } from 'rxjs/internal/Observable';
import { Invitation, Permission, User } from '../models/user.model';
import { HttpClient } from '@angular/common/http';

@Injectable({
  providedIn: 'root',
})
export class AdminService {
    private apiUrl = `${API_BASE_URL}/admin`;

  constructor(private http: HttpClient) {}

  // Utilisateurs
  getUsers(): Observable<User[]> {
    return this.http.get<User[]>(`${this.apiUrl}/users`);
  }

  updateUserRole(id: number, role: string): Observable<User> {
    return this.http.put<User>(`${this.apiUrl}/users/${id}/role`, { role });
  }

  activateUser(id: number): Observable<any> {
    return this.http.put(`${this.apiUrl}/users/${id}/activate`, {});
  }

  deactivateUser(id: number): Observable<any> {
    return this.http.put(`${this.apiUrl}/users/${id}/deactivate`, {});
  }

  deleteUser(id: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/users/${id}`);
  }

  // Invitations
  createInvitation(email: string, role: string): Observable<{ token: string; expiresAt: Date }> {
    return this.http.post<{ token: string; expiresAt: Date }>(`${this.apiUrl}/invitations`, { email, role });
  }

  getInvitations(): Observable<Invitation[]> {
    return this.http.get<Invitation[]>(`${this.apiUrl}/invitations`);
  }

  cancelInvitation(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/invitations/${id}`);
  }

  validateInvitation(token: string): Observable<{ isValid: boolean }> {
    return this.http.post<{ isValid: boolean }>(`${this.apiUrl}/invitations/validate`, token);
  }
  acceptInvitation(token: string, password: string, nom: string, prenom: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/invitations/accept`, {
      token,
      password,
      nom,
      prenom
    });
  }

  // Permissions
  getPermissions(): Observable<Permission[]> {
    return this.http.get<Permission[]>(`${this.apiUrl}/permissions`);
  }

  getUserPermissions(userId: number): Observable<string[]> {
    return this.http.get<string[]>(`${this.apiUrl}/users/${userId}/permissions`);
  }
}
