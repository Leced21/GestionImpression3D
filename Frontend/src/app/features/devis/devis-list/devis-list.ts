import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { RouterModule } from '@angular/router';
import { Devis, DevisStatistics, DevisStatus } from '../../../models/devis.model';
import { DevisService } from '../../../services/devis.service';

@Component({
  selector: 'app-devis-list',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './devis-list.html',
  styleUrl: './devis-list.css',
})
export class DevisList implements OnInit {
  devisList: Devis[] = [];
  statistics?: DevisStatistics;
  protected readonly DevisStatus = DevisStatus

  constructor(
    private devisService: DevisService,
    private cdr: ChangeDetectorRef
  ) { }

  ngOnInit(): void {
    this.loadDevis();
    this.loadStatistics();
  }

  loadDevis(): void {
    this.devisService.getAll().subscribe({
      next: (data) => {
        this.devisList = data,
          this.cdr.detectChanges()
      },
      error: (error) => {
        console.error('Erreur lors du chargement des devis', error);
      }
    })
  }

  loadStatistics(): void {
    this.devisService.getStatistics().subscribe({
      next: (statistics) => {
        this.statistics = statistics;
        this.cdr.detectChanges();
      },
      error: (error) => {
        console.error('Erreur lors du chargement des statistiques', error);
      },
    });
  }

  getStatusLabel(statut: DevisStatus | string): string {
    const labels: Record<string, string> = {
      'Brouillon': '📝 Brouillon',
      'Envoyé': '📧 Envoyé',
      'Accepté': '✅ Accepté',
      'Refusé': '❌ Refusé',
      'Expiré': '⏰ Expiré'
    };
    return labels[statut] || statut;
  }

  getStatusClass(statut: DevisStatus | string): string {
    return statut;
  }

  updateStatut(devis: Devis, statut: DevisStatus): void {
    this.devisService.updateStatut(devis.id, statut).subscribe({
      next: () => this.loadDevis()
    });
  }

  generatePdf(devis: Devis): void {
    this.devisService.generatePdf(devis.id).subscribe({
      next: (blob) => {
        const url = window.URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = `Devis_${devis.numeroDevis}.pdf`;
        link.click();
        window.URL.revokeObjectURL(url);
      },
      error: (err) => console.error(err)
    });
  }
}
