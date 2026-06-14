import { Injectable } from '@angular/core';
import { API_BASE_URL } from '../config/api.config';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs/internal/Observable';
import { Client } from '../models/client.model';

@Injectable({
  providedIn: 'root',
})
export class ClientService {
  private apiUrl = `${API_BASE_URL}/client`;
  constructor(
    private http: HttpClient
  ) { }
    getAll(): Observable<Client[]> {
    return this.http.get<Client[]>(this.apiUrl);
  }

  getCount(): Observable<{ count: number }> {
    return this.http.get<{ count: number }>(`${this.apiUrl}/count`);
  }

  search(q: string): Observable<Client[]> {
    return this.http.get<Client[]>(`${this.apiUrl}/search?q=${q}`);
  }

  getById(id: number): Observable<Client> {
    return this.http.get<Client>(`${this.apiUrl}/${id}`);
  }

  getByEmail(email: string): Observable<Client> {
    return this.http.get<Client>(`${this.apiUrl}/email/${email}`);
  }

  create(client: Partial<Client>): Observable<Client> {
    return this.http.post<Client>(this.apiUrl, client);
  }

  update(id: number, client: Partial<Client>): Observable<Client> {
    return this.http.put<Client>(`${this.apiUrl}/${id}`, client);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
