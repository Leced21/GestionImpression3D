import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { RouterModule } from '@angular/router';
import { OrdreFabrication, OrdrePriorite, OrdreStatistics, OrdreStatus } from '../../../models/ordre-fabrication.model';
import { OrdreFabricationService } from '../../../services/ordre-fabrication.service';

@Component({
  selector: 'app-ordre-list',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './ordre-list.html',
  styleUrl: './ordre-list.css',
})
export class OrdreList implements OnInit {
  ordres: OrdreFabrication[] = [];
  statistics?: OrdreStatistics
  OrdreStatus = OrdreStatus;

  constructor(
    private ordreService: OrdreFabricationService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.loadOrdres();
    this.loadStatistics();
  }

  loadOrdres(): void {
    this.ordreService.getAll().subscribe({
      next: (data) => {
        this.ordres = data;
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Erreur lors du chargement des ordres de fabrication', err)
      }
    });
  }

  loadStatistics(): void {
    this.ordreService.getStatistics().subscribe({
      next: (data) => {
        this.statistics = data;
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Erreur lors du chargement des statistiques', err)
      }
    });
  }
  getStatusClass(statut: OrdreStatus): string {
    return `status-${(statut as string).toLowerCase().replace(' ', '-')}`;
  }

  getPriorityClass(priorite: OrdrePriorite): string {
    return `priority-${(priorite as string).toLowerCase()}`;
  }

  startProduction(ordre: OrdreFabrication): void {
    this.ordreService.startProduction(ordre.id).subscribe(() => this.loadOrdres());
  }

  completeProduction(ordre: OrdreFabrication): void {
    this.ordreService.completeProduction(ordre.id).subscribe(() => this.loadOrdres());
  }

  deleteOrdre(id: number): void {
    if (confirm('Supprimer cet ordre ?')) {
      this.ordreService.delete(id).subscribe(() => this.loadOrdres());
    }
  }
}
