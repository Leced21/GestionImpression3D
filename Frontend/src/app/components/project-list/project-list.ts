import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, OnDestroy, OnInit } from '@angular/core';
import { RouterModule } from '@angular/router';
import { ProjetService } from '../../services/projet.service';
import { Projet, ProjetStatus } from '../../models/projet.model';
import { Subject, takeUntil } from 'rxjs';

@Component({
  selector: 'app-project-list',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './project-list.html',
  styleUrls: ['./project-list.css'],
})
export class ProjectList implements OnInit, OnDestroy {
  projets: Projet[] = [];
  private destroy$ = new Subject<void>();

  constructor(
    private projetService: ProjetService,
    private cdr: ChangeDetectorRef
  ) { }

  ngOnInit(): void {
    this.loadProjets();
  }

  loadProjets(): void {
    this.projetService.getAll().pipe(takeUntil(this.destroy$)).subscribe({
      next: (data) => {
        setTimeout(() => {
          this.projets = data;
          this.cdr.detectChanges(); // Assure que les changements sont pris en compte immédiatement
          console.log('Projets loaded:', this.projets)
        }, 500); // Simule un délai pour voir le spinner
      },
      error: (error) => {
        console.error('Error loading projets:', error);
      }
    });
  }

  getProjetsByStatus(status: ProjetStatus | string): Projet[] {
    return this.projets.filter(p => p.statut === status);
  }

  getStatusLabel(statut: ProjetStatus | string): string {
    const labels: Record<ProjetStatus, string> = {
      [ProjetStatus.Brouillon]: '📝 Brouillon',
      [ProjetStatus.EnCours]: '🔄 En cours',
      [ProjetStatus.Termine]: '✅ Terminé'
    };
    return labels[statut as ProjetStatus] || statut;
  }
  getBadgeClass(statut: string): string {
    return `badge-${statut}`;
  }

  supprimer(id: number): void {
    if (confirm('Supprimer ce projet ?')) {
      this.projetService.delete(id).pipe(takeUntil(this.destroy$)).subscribe(() => this.loadProjets());
    }
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }
}


