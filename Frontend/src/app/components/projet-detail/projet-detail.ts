import { ChangeDetectorRef, Component, OnDestroy, OnInit } from '@angular/core';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { PieceService } from '../../services/piece.service';
import { Piece } from '../../models/piece.model';
import { Projet, ProjetPiece, ProjetStats } from '../../models/projet.model';
import { ProjetService } from '../../services/projet.service';
import { Subject, takeUntil } from 'rxjs';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';

@Component({
  selector: 'app-projet-detail',
  imports: [CommonModule, RouterModule, ReactiveFormsModule, FormsModule],
  templateUrl: './projet-detail.html',
  styleUrl: './projet-detail.css',
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
    private router: Router
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
        this.availablePieces = data.filter(p => p.statut === 'Commercialisable');
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

  getStatutLabel(statut: string): string {
    const labels: Record<string, string> = {
      'Brouillon': '📝 Brouillon',
      'EnCours': '🔄 En cours',
      'Termine': '✅ Terminé'
    };
    return labels[statut] || statut;
  }

  getBadgeClass(statut: string): string {
    return `badge-${statut}`;
  }

  deleteProjet(): void {
    if (confirm('Supprimer ce projet ?')) {
      this.projetService.delete(this.projet.id).pipe(takeUntil(this.destroy$)).subscribe({
        next: () => this.router.navigate(['/projets'])
      });
    }
  }
}
