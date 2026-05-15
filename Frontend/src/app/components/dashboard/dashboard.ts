import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { RouterModule } from '@angular/router';
import { Piece } from '../../models/piece.model';
import { PieceService } from '../../services/piece.service';
import { DashboardStat } from '../../models/dashboardstat';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.css',
})
export class Dashboard implements OnInit {
  pieces:Piece[] = [];
  stats: DashboardStat = {
    totalPieces:0,
    enConception:0,
    enPrototypage:0,
    enProduction:0,
    commercialisables:0,
    chiffreAffaires:0
  };

  evolutions = {
    conception:'+12%',
    impressions:'+8%',
    produites:'+15%',
    ca:'+10%'
  }
  constructor(
    private pieceService: PieceService,
    private cdr: ChangeDetectorRef
  ) { }
  ngOnInit(): void {
    this.loadPieces();
    this.loadStats();
  }

  loadPieces() : void {
    this.pieceService.getAll().subscribe({
      next: (data) => {
        this.pieces = data;
        this.updateStats();
        this.cdr.detectChanges(); // Assure que les changements sont pris en compte immédiatement
        console.log('Pieces loaded:', this.pieces);
      },
      error: (err) => console.error('Error fetching pieces:', err)
    });
  }

  loadStats() : void {
    this.pieceService.getDashboardStats().subscribe({
      next: (data) => {
        this.stats = data;
        this.cdr.detectChanges(); // Assure que les changements sont pris en compte immédiatement
        console.log('Stats loaded:', this.stats);
      },
      error: (err) => console.error('Error fetching stats:', err)
    });
  }

  updateStats(): void {
    this.stats.totalPieces = this.pieces.length;
    this.stats.enConception = this.pieces.filter(p => p.statut === 'Conception').length;
    this.stats.enPrototypage = this.pieces.filter(p => p.statut === 'Prototypage').length;
    this.stats.enProduction = this.pieces.filter(p => p.statut === 'Production').length;
    this.stats.commercialisables = this.pieces.filter(p => p.statut === 'Commercialisable').length;
    this.stats.chiffreAffaires = this.pieces
      .filter(p => p.statut === 'Commercialisable')
      .reduce((sum, p) => sum + (p.prixVente || 0), 0);
  }

  getCoutTotal(piece: Piece): number {
    return piece.coutMatiere + piece.coutMachine + piece.coutMainOeuvre;
  }

  getProgressWidth(piece: Piece): number {
    const statuts = ['Brouillon', 'Conception', 'Prototypage', 'Validation', 'Production', 'Commercialisable'];
    const index = statuts.indexOf(piece.statut);
    return (index / (statuts.length - 1)) * 100;
  }

  getBadgeClass(statut: string): string {
    const classes: Record<string, string> = {
      'Brouillon': 'badge-brouillon',
      'Conception': 'badge-conception',
      'Prototypage': 'badge-prototypage',
      'Validation': 'badge-validation',
      'Production': 'badge-production',
      'Commercialisable': 'badge-commercial'
    };
    return classes[statut] || 'badge-brouillon';
  }
}
