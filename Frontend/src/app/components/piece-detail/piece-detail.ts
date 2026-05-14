import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { Piece } from '../../models/piece.model';
import { PieceService } from '../../services/piece.service';

@Component({
  selector: 'app-piece-detail',
  imports: [CommonModule, RouterModule],
  standalone: true,
  templateUrl: './piece-detail.html',
  styleUrl: './piece-detail.css',
})
export class PieceDetail implements OnInit {
  piece: Piece | null = null;
  prixRecommande: number = 0;
  statuts: string[] = ['Brouillon', 'Conception', 'Prototypage', 'Validation', 'Production', 'Commercialisable'];
  versionNumber: number = 1;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private pieceService: PieceService,
    private cdr: ChangeDetectorRef
  ) { }

  ngOnInit(): void {
    const id = this.route.snapshot.params['id'];
    if (id) {
      this.loadPiece(id);
    }
  }

  loadPiece(id: number): void {
    this.pieceService.getById(id).subscribe({
      next: (data) => {
        this.piece = data;
        this.loadPrixRecommande(id);
        this.extractVersionNumber();
        this.cdr.detectChanges(); // Assure que les changements sont pris en compte immédiatement
      },
      error: (err) => console.error('Erreur chargement:', err)
    });
  }

  loadPrixRecommande(id: number): void {
    this.pieceService.getPrixRecommande(id).subscribe({
      next: (prix) => this.prixRecommande = prix,
      error: (err) => console.error('Erreur calcul prix:', err)
    });
  }

  extractVersionNumber(): void {
    if (this.piece?.reference) {
      const match = this.piece.reference.match(/v(\d+)/i);
      if (match) {
        this.versionNumber = parseInt(match[1]);
      }
    }
  }

  getCoutTotal(): number {
    if (!this.piece) return 0;
    return this.piece.coutMatiere + this.piece.coutMachine + this.piece.coutMainOeuvre;
  }

  getMarge(): number {
    if (!this.piece || !this.piece.prixVente) return 0;
    return this.piece.prixVente - this.getCoutTotal();
  }

  getMargePourcentage(): number {
    const coutTotal = this.getCoutTotal();
    return coutTotal > 0 ? (this.getMarge() / coutTotal) * 100 : 0;
  }

  getBadgeClass(statut: string): string {
    const classes: Record<string, string> = {
      'Brouillon': 'badge-Brouillon',
      'Conception': 'badge-Conception',
      'Prototypage': 'badge-Prototypage',
      'Validation': 'badge-Validation',
      'Production': 'badge-Production',
      'Commercialisable': 'badge-Commercialisable'
    };
    return classes[statut] || 'badge-Brouillon';
  }

  isStepCompleted(step: string): boolean {
    if (!this.piece) return false;
    const currentIndex = this.statuts.indexOf(this.piece.statut);
    const stepIndex = this.statuts.indexOf(step);
    return stepIndex < currentIndex;
  }

  nextStatus(): void {
    if (!this.piece) return;

    const currentIndex = this.statuts.indexOf(this.piece.statut);
    if (currentIndex < this.statuts.length - 1) {
      const nextStatut = this.statuts[currentIndex + 1];

      this.pieceService.updateStatus(this.piece.id, nextStatut).subscribe({
        next: () => {
          this.loadPiece(this.piece!.id); // Recharge les données pour mettre à jour l'affichage
        },
        error: (err) => console.error('Erreur changement statut:', err)
      });
    }
  }

  applyRecommendedPrice(): void {
    if (!this.piece) return;

    const updatedPiece = { ...this.piece, prixVente: this.prixRecommande };

    this.pieceService.update(this.piece.id, updatedPiece).subscribe({
      next: () => {
        this.loadPiece(this.piece!.id); // Recharge les données pour mettre à jour l'affichage
      },
      error: (err) => console.error('Erreur mise à jour prix:', err)
    });
  }

  deletePiece(): void {
    if (!this.piece) return;

    if (confirm(`Supprimer définitivement la pièce "${this.piece.nom}" ?`)) {
      this.pieceService.delete(this.piece.id).subscribe({
        next: () => {
          this.router.navigate(['/pieces']);
        },
        error: (err) => console.error('Erreur suppression:', err)
      });
    }
  }

  createNewVersion(): void {
    if (!this.piece) return;

    // Créer une nouvelle version en incrémentant le numéro
    const newVersionNumber = this.versionNumber + 1;
    const newReference = this.piece.reference.replace(/v\d+/, `v${newVersionNumber}`);

    const newPiece = {
      nom: this.piece.nom,
      reference: newReference,
      description: this.piece.description,
      coutMatiere: this.piece.coutMatiere,
      coutMachine: this.piece.coutMachine,
      coutMainOeuvre: this.piece.coutMainOeuvre,
      statut: 'Brouillon'
    };

    this.pieceService.create(newPiece).subscribe({
      next: (created) => {
        this.router.navigate(['/pieces', created.id]);
      },
      error: (err) => console.error('Erreur création version:', err)
    });
  }

  printPiece(): void {
    if (!this.piece) return;

    const printWindow = window.open('', '_blank');
    if (printWindow) {
      printWindow.document.write(`
        <html>
          <head>
            <title>${this.piece.nom} - Fiche technique</title>
            <style>
              body { font-family: Arial, sans-serif; padding: 2rem; }
              h1 { color: #1e293b; }
              .info { margin: 1rem 0; padding: 1rem; border: 1px solid #e2e8f0; border-radius: 0.5rem; }
              .label { font-weight: bold; color: #64748b; }
            </style>
          </head>
          <body>
            <h1>${this.piece.nom}</h1>
            <p>Référence: ${this.piece.reference}</p>
            <div class="info">
              <p><span class="label">Statut:</span> ${this.piece.statut}</p>
              <p><span class="label">Description:</span> ${this.piece.description || 'Aucune'}</p>
              <p><span class="label">Coût total:</span> ${this.getCoutTotal().toFixed(2)} €</p>
              <p><span class="label">Prix de vente:</span> ${this.piece.prixVente?.toFixed(2) || 'Non défini'} €</p>
              <p><span class="label">Date création:</span> ${new Date(this.piece.dateCreation).toLocaleDateString()}</p>
            </div>
          </body>
        </html>
      `);
      printWindow.document.close();
      printWindow.print();
    }
  }
}