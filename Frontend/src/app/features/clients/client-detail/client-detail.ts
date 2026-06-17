import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { ClientService } from '../../../services/client.service';
import { Client } from '../../../models/client.model';

@Component({
  selector: 'app-client-detail',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './client-detail.html',
  styleUrl: './client-detail.css',
})
export class ClientDetail implements OnInit {
  client: Client | null = null;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private clientService: ClientService,
    private cdr: ChangeDetectorRef
  ) { }

  ngOnInit(): void {
    const id = this.route.snapshot.params['id'];
    if (id) this.loadClient(id);
  }
  loadClient(id: number): void {
    this.clientService.getById(id).subscribe({
      next: (client) => {
        this.client = client;
        this.cdr.detectChanges();
      },
      error: (error) => {
        console.error('Error loading client:', error);
      },
    });
  }
  deleteClient(): void {
    if (confirm(`Supprimer le client "${this.client?.nom}" ?`)) {
      this.clientService.delete(this.client!.id).subscribe({
        next: () => this.router.navigate(['/clients'])
      });
    }
  }
}
