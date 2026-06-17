import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { ClientService } from '../../../services/client.service';
import { Client } from '../../../models/client.model';

@Component({
  selector: 'app-client-list',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './client-list.html',
  styleUrl: './client-list.css',
})
export class ClientList implements OnInit {
  clients: Client[] = [];
  searchTerm = '';
  constructor(
    private clientService: ClientService,
    private cdr: ChangeDetectorRef
  ) { }
  ngOnInit(): void {
    this.loadClients();
  }
  loadClients(): void {
    this.clientService.getAll().subscribe({
      next: (data) => {
        this.clients = data;
        this.cdr.detectChanges();
      },
      error: (error) => {
        console.error('Erreur lors du chargement des clients', error);
      },
    });
  }
  search(): void {
    if (this.searchTerm.length > 2) {
      this.clientService.search(this.searchTerm).subscribe({
        next: (data) => this.clients = data,
        error: (err) => console.error(err)
      });
    } else if (this.searchTerm === '') {
      this.loadClients();
    }
  }

  deleteClient(client: Client): void {
    if (confirm(`Supprimer le client "${client.nom}" ?`)) {
      this.clientService.delete(client.id).subscribe({
        next: () => this.loadClients(),
        error: (err) => console.error(err)
      });
    }
  }
}
