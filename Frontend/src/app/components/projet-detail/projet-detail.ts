import { ChangeDetectorRef, Component, OnDestroy, OnInit } from '@angular/core';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { PieceService } from '../../services/piece.service';
import { Piece, PieceStatus } from '../../models/piece.model';
import { Projet, ProjetPiece, ProjetStats, ProjetStatus } from '../../models/projet.model';
import { ProjetService } from '../../services/projet.service';
import { Subject, takeUntil } from 'rxjs';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { ExportService } from '../../services/export.service';
import { TechnicalPlanService } from '../../services/technical-plan.service';

@Component({
  selector: 'app-projet-detail',
  imports: [CommonModule, RouterModule, ReactiveFormsModule, FormsModule],
  templateUrl: './projet-detail.html',
  styleUrls: ['./projet-detail.css'],
})
export class ProjetDetail implements OnInit, OnDestroy {
  projet!: Projet;
  stats?: ProjetStats;
  availablePieces: Piece[] = [];
  selectedPieceId?: number;
  selectedQuantite: number = 1;
  private destroy$ = new Subject<void>();

  constructor(
    private projetService: ProjetService,
    private pieceService: PieceService,
    private cdr: ChangeDetectorRef,
    private route: ActivatedRoute,
    private router: Router,
    private exportService: ExportService,
    private technicalPlanService: TechnicalPlanService
  ) { }

  ngOnInit(): void {
    const projetId = this.route.snapshot.params['id'];
    if (projetId) {
      this.loadProjet(projetId);
      this.loadAvailablePieces();
    }
  }

  loadProjet(id: number): void {
    this.projetService.getById(id).pipe(takeUntil(this.destroy$)).subscribe({
      next: (data) => {
        this.projet = data;
        this.loadStats(id);
        this.cdr.detectChanges();
      },
      error: (err) => console.error('Error loading projet:', err)
    });
  }

  loadStats(id: number): void {
    this.projetService.getStats(id).pipe(takeUntil(this.destroy$)).subscribe({
      next: (data) => this.stats = data,
      error: (err) => console.error(err)
    });
  }

  loadAvailablePieces(): void {
    this.pieceService.getAll().pipe(takeUntil(this.destroy$)).subscribe({
      next: (data) => {
        this.availablePieces = data.filter(p => p.statut === PieceStatus.Commercialisable);
      },
      error: (err) => console.error(err)
    });
  }

  ajouterPiece(): void {
    if (!this.selectedPieceId) return;

    this.projetService.ajouterPiece(this.projet.id, this.selectedPieceId, this.selectedQuantite).pipe(takeUntil(this.destroy$)).subscribe({
      next: () => {
        this.loadProjet(this.projet.id);
        this.selectedPieceId = 0;
        this.selectedQuantite = 1;
      },
      error: (err) => console.error(err)
    });
  }

  modifierQuantite(item: ProjetPiece, nouvelleQuantite: number): void {
    if (nouvelleQuantite <= 0) return;

    // Pour simplifier, on retire et on réajoute avec la nouvelle quantité
    this.projetService.retirerPiece(this.projet.id, item.pieceId).pipe(takeUntil(this.destroy$)).subscribe({
      next: () => {
        this.projetService.ajouterPiece(this.projet.id, item.pieceId, nouvelleQuantite).pipe(takeUntil(this.destroy$)).subscribe({
          next: () => this.loadProjet(this.projet.id)
        });
      }
    });
  }

  retirerPiece(pieceId: number): void {
    if (confirm('Retirer cette pièce du projet ?')) {
      this.projetService.retirerPiece(this.projet.id, pieceId).pipe(takeUntil(this.destroy$)).subscribe({
        next: () => this.loadProjet(this.projet.id)
      });
    }
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  getStatutLabel(statut: ProjetStatus): string {
    const labels: Record<ProjetStatus, string> = {
      [ProjetStatus.Brouillon]: '📝 Brouillon',
      [ProjetStatus.EnCours]: '🔄 En cours',
      [ProjetStatus.Termine]: '✅ Terminé'
    };
    return labels[statut] || statut;
  }

  getBadgeClass(statut: ProjetStatus): string {
    return `badge-${statut}`;
  }

  deleteProjet(): void {
    if (confirm('Supprimer ce projet ?')) {
      this.projetService.delete(this.projet.id).pipe(takeUntil(this.destroy$)).subscribe({
        next: () => this.router.navigate(['/projets'])
      });
    }
  }
  exportPdf(): void {
    this.exportService.exportProjetPdf(this.projet.id).subscribe({
      next: (blob) => {
        this.exportService.downloadPdf(blob, `Projet_${this.projet.reference}.pdf`);
      },
      error: (err) => console.error('Erreur export PDF:', err)
    });
  }

  exportDevis(): void {
    this.exportService.exportDevisPdf(this.projet.id).subscribe({
      next: (blob) => {
        this.exportService.downloadPdf(blob, `Devis_${this.projet.reference}.pdf`);
      },
      error: (err) => console.error('Erreur export devis:', err)
    });
  }

  downloadTechnicalPlans(): void {
    this.technicalPlanService.downloadProjectTechnicalPlans(this.projet.id);
  }
}


