import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { RouterModule } from '@angular/router';
import {
  CdkDragDrop,
  CdkDrag,
  CdkDropList,
  CdkDropListGroup,
  moveItemInArray,
  transferArrayItem
} from '@angular/cdk/drag-drop';
import { Piece } from '../../models/piece.model';
import { PieceService } from '../../services/piece.service';

@Component({
  selector: 'app-piece-kanban',
  standalone: true,
  imports: [CommonModule, RouterModule, CdkDropListGroup, CdkDropList, CdkDrag],
  templateUrl: './piece-kanban.html',
  styleUrl: './piece-kanban.css',
})
export class PieceKanban implements OnInit {
  allPieces: Piece[] = [];
  isLoading: boolean = true;
  private statutOrder = ['Brouillon', 'Conception', 'Prototypage', 'Validation', 'Production', 'Commercialisable'];

  contextMenuVisible: boolean = false;
  contextMenuX: number = 0;
  contextMenuY: number = 0;
  selectedPiece: Piece | null = null;
  constructor(
    private pieceService: PieceService,
    private cdr: ChangeDetectorRef
  ) { }

  ngOnInit(): void {
    this.loadPieces();
  }

  loadPieces(): void {
    this.isLoading = true;
    this.pieceService.getAll().subscribe({
      next: (data) => {
        this.allPieces = data;
        this.isLoading = false;
        this.cdr.detectChanges(); // Assure que les changements sont pris en compte immédiatement
      },
      error: (error) => {
        console.error('Error loading pieces:', error);
        this.isLoading = false;
        this.cdr.detectChanges(); // Assure que les changements sont pris en compte même en cas d'erreur
      }
    });
  }

  getPiecesByStatut(statut: string): Piece[] {
    return this.allPieces.filter(p => p.statut === statut);
  }

  getCountByStatut(statut: string): number {
    return this.getPiecesByStatut(statut).length;
  }

  getCoutTotal(piece: Piece): number {
    return piece.coutMatiere + piece.coutMachine + piece.coutMainOeuvre;
  }

  getConnectedLists(): string[] {
    return this.statutOrder.map(statut => `cdk-drop-list-${statut}`);
  }

  onDrop(event: CdkDragDrop<Piece[]>): void {
    if (event.previousContainer === event.container) {
      // Même colonne - réorganisation (pas nécessaire pour nous)
      moveItemInArray(event.container.data, event.previousIndex, event.currentIndex);
    } else {
      // Changement de colonne - changement de statut
      const piece = event.previousContainer.data[event.previousIndex];
      const newContainerId = event.container.id;
      const newStatut = this.getStatutFromContainerId(newContainerId);

      if (newStatut && this.isValidTransition(piece.statut, newStatut)) {
        // Mettre à jour localement pour réactivité
        transferArrayItem(
          event.previousContainer.data,
          event.container.data,
          event.previousIndex,
          event.currentIndex
        );

        // Mettre à jour le statut dans l'API
        this.pieceService.updateStatus(piece.id, newStatut).subscribe({
          next: (updatedPiece) => {
            piece.statut = updatedPiece.statut;
            this.cdr.detectChanges();
          },
          error: (err) => {
            console.error('Erreur mise à jour statut:', err);
            // Recharger en cas d'erreur
            this.loadPieces();
          }
        });
      } else {
        // Transition invalide - recharger
        this.loadPieces();
      }
    }
  }

  private getStatutFromContainerId(containerId: string): string | null {
    const match = containerId.match(/cdk-drop-list-(.+)/);
    if (match) {
      const statut = match[1];
      if (this.statutOrder.includes(statut)) {
        return statut;
      }
    }
    return null;
  }
  isValidTransition(currentStatut: string, newStatut: string): boolean {
    const currentIndex = this.statutOrder.indexOf(currentStatut);
    const newIndex = this.statutOrder.indexOf(newStatut);

    // On ne peut avancer que d'une étape ou reculer
    return Math.abs(newIndex - currentIndex) === 1 || newIndex > currentIndex;
  }
  openCardMenu(event: MouseEvent, piece: Piece): void {
    event.preventDefault();
    event.stopPropagation();

    this.selectedPiece = piece;
    this.contextMenuX = event.clientX;
    this.contextMenuY = event.clientY;
    this.contextMenuVisible = true;

    // Fermer le menu au clic ailleurs
    setTimeout(() => {
      document.addEventListener('click', this.closeContextMenu.bind(this));
    }, 0);
  }

  closeContextMenu(): void {
    this.contextMenuVisible = false;
    this.selectedPiece = null;
    document.removeEventListener('click', this.closeContextMenu.bind(this));
  }

  quickChangeStatus(newStatut: string): void {
    if (this.selectedPiece) {
      this.pieceService.updateStatus(this.selectedPiece.id, newStatut).subscribe({
        next: (updatedPiece) => {
          this.loadPieces();
          this.closeContextMenu();
        },
        error: (err) => console.error('Erreur:', err)
      });
    }
  }

  quickDelete(): void {
    if (this.selectedPiece && confirm(`Supprimer "${this.selectedPiece.nom}" ?`)) {
      this.pieceService.delete(this.selectedPiece.id).subscribe({
        next: () => {
          this.loadPieces();
          this.closeContextMenu();
        },
        error: (err) => console.error('Erreur:', err)
      });
    }
  }
}
