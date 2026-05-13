import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms'; // Indispensable pour le [(ngModel)] de ton HTML
import { PieceService } from '../../services/piece.service';
import { Piece } from '../../models/piece.model';

@Component({
  selector: 'app-piece-list',
  // Ajout de FormsModule pour faire fonctionner la barre de recherche
  imports: [CommonModule, RouterModule, FormsModule],
  standalone: true,
  templateUrl: './piece-list.html',
  styleUrls: ['./piece-list.css'],
})
export class PieceList implements OnInit {
  pieces: Piece[] = [];
  searchTerm: string = '';
  statutFiltre: string = '';

  // Variables nécessaires pour ton HTML
  isLoading: boolean = false;
  stats: any = null;

  // Pagination
  currentPage: number = 1;
  itemsPerPage: number = 12;
  totalPages: number = 1;

  constructor(private pieceService: PieceService) { }

  ngOnInit(): void {
    this.loadPieces();
  }

  loadPieces() {
    this.isLoading = true; // Active le spinner dans le HTML
    this.pieceService.getAll().subscribe({
      next: (data) => {
        this.pieces = data;
        this.calculateStats(); // Calcule les chiffres pour les cartes statistiques
        this.isLoading = false;
        console.log('Pieces loaded:', this.pieces);
      },
      error: (err) => {
        console.error('Error fetching pieces:', err);
        this.isLoading = false;
      }
    });
  }

  // Logique pour les cartes de statistiques en haut de ton HTML
  calculateStats() {
    this.stats = {
      enConception: this.pieces.filter(p => p.statut === 'Conception').length,
      enPrototypage: this.pieces.filter(p => p.statut === 'Prototypage').length,
      enProduction: this.pieces.filter(p => p.statut === 'Production').length,
      commercialisables: this.pieces.filter(p => p.statut === 'Commercialisable').length,
      chiffreAffaires: this.pieces.reduce((acc, p) => acc + (p.prixVente || 0), 0)
    };
  }

  nextStatus(piece: Piece): void {
    const statuts = ['Brouillon', 'Conception', 'Prototypage', 'Validation', 'Production', 'Commercialisable'];
    const currentIndex = statuts.indexOf(piece.statut);
    if (currentIndex === -1 || currentIndex === statuts.length - 1) return;

    const nextStatut = statuts[currentIndex + 1];
    this.pieceService.updateStatus(piece.id, nextStatut).subscribe({
      next: () => {
        piece.statut = nextStatut;
        this.calculateStats(); // Met à jour les compteurs
      },
      error: (err) => console.error('Error updating status:', err)
    });
  }

  deletePiece(id: number): void {
    if (confirm('Êtes-vous sûr de vouloir supprimer cette pièce ?')) {
      this.pieceService.delete(id).subscribe({
        next: () => this.loadPieces(),
        error: (err) => console.error('Error deleting piece:', err)
      });
    }
  }

  getBadgeClass(statut: string): string {
    const classes: Record<string, string> = {
      'Brouillon': 'badge-gray',
      'Conception': 'badge-yellow',
      'Prototypage': 'badge-blue',
      'Validation': 'badge-green',
      'Production': 'badge-cyan',
      'Commercialisable': 'badge-purple'
    };
    return classes[statut] || 'badge-secondary';
  }

  getCoutTotal(piece: Piece): number {
    return (piece.coutMatiere || 0) + (piece.coutMainOeuvre || 0) + (piece.coutMachine || 0);
  }

  // Ton Getter qui gère tout (Filtre + Recherche + Pagination)
  get filteredPieces(): Piece[] {
    let result = [...this.pieces];

    if (this.searchTerm) {
      const term = this.searchTerm.toLowerCase();
      result = result.filter(p =>
        p.nom.toLowerCase().includes(term) ||
        p.reference.toLowerCase().includes(term) ||
        p.description?.toLowerCase().includes(term)
      );
    }

    if (this.statutFiltre) {
      result = result.filter(p => p.statut === this.statutFiltre);
    }

    this.totalPages = Math.ceil(result.length / this.itemsPerPage) || 1;
    const start = (this.currentPage - 1) * this.itemsPerPage;
    return result.slice(start, start + this.itemsPerPage);
  }

  onSearch(): void { this.currentPage = 1; }
  onFilterChange(): void { this.currentPage = 1; }
  goToPage(page: number): void { this.currentPage = page; }

  getPageNumbers(): number[] {
    const pages: number[] = [];
    for (let i = 1; i <= this.totalPages; i++) pages.push(i);
    return pages;
  }

  getMarge(piece: Piece): number {
    return (piece.prixVente || 0) - this.getCoutTotal(piece);
  }

  getMargePourcentage(piece: Piece): number {
    const coutTotal = this.getCoutTotal(piece);
    return coutTotal > 0 ? (this.getMarge(piece) / coutTotal) * 100 : 0;
  }
}