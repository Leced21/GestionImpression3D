import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, OnDestroy, OnInit } from '@angular/core';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { HttpEventType } from '@angular/common/http';
import { Piece, PieceStatus } from '../../models/piece.model';
import { PieceService } from '../../services/piece.service';
import { Subject, takeUntil } from 'rxjs';
import { ThreeDViewer } from '../three-d-viewer/three-d-viewer';
import { ExportService } from '../../services/export.service';
import { TechnicalPlanService } from '../../services/technical-plan.service';
import { PieceVersions } from '../piece-versions/piece-versions';
import { PricingService } from '../../services/pricing.service';
import { PiecePricing } from '../../models/pricing.model';

@Component({
  selector: 'app-piece-detail',
  imports: [CommonModule, RouterModule, ThreeDViewer, PieceVersions],
  standalone: true,
  templateUrl: './piece-detail.html',
  styleUrls: ['./piece-detail.css'],
})
export class PieceDetail implements OnInit, OnDestroy {
  piece: Piece | null = null;
  pricing: PiecePricing | null = null;
  prixRecommande: number = 0;
  activeTab: string = 'general';
  statuts: PieceStatus[] = Object.values(PieceStatus);
  PieceStatus = PieceStatus;
  versionNumber: number = 1;
  private destroy$ = new Subject<void>();

  uploading = false;
  uploadProgress = 0;
  uploadError = '';

  versions = [
    { date: new Date(), comment: 'Version initiale' }
  ];

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private pieceService: PieceService,
    private cdr: ChangeDetectorRef,
    private exportService: ExportService,
    private technicalPlanService: TechnicalPlanService,
    private pricingService: PricingService
  ) { }

  ngOnInit(): void {
    const id = this.route.snapshot.params['id'];
    if (id) {
      this.loadPiece(id);
    }
  }
  ngAfterViewInit(): void { }
  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();

  }
  loadPiece(id: number): void {
    this.pieceService.getById(id).pipe(takeUntil(this.destroy$)).subscribe({
      next: (data) => {
        this.piece = data;
        this.loadPrixRecommande(id);
        this.loadPricing(id);
        this.extractVersionNumber();
        this.cdr.detectChanges(); // Assure que les changements sont pris en compte immédiatement
      },
      error: (err) => console.error('Erreur chargement:', err)
    });
  }

  loadPrixRecommande(id: number): void {
    this.pieceService.getPrixRecommande(id).pipe(takeUntil(this.destroy$)).subscribe({
      next: (prix) => this.prixRecommande = prix,
      error: (err) => console.error('Erreur calcul prix:', err)
    });
  }

  loadPricing(id: number): void {
    this.pricingService.getPiecePricing(id).pipe(takeUntil(this.destroy$)).subscribe({
      next: (pricing) => {
        this.pricing = pricing;
        this.prixRecommande = pricing.recommendedPriceHt;
      },
      error: (err) => console.error('Erreur calcul rentabilite V2:', err)
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

  isStepCompleted(step: PieceStatus | string): boolean {
    if (!this.piece) return false;
    const currentIndex = this.statuts.indexOf(this.piece.statut);
    const stepIndex = this.statuts.indexOf(step as PieceStatus);
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

    const updatedPiece = { ...this.piece, prixVente: this.getPrixRecommande() };

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
      statut: PieceStatus.Brouillon
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
  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      this.uploadFile(input.files[0]);
    }
  }

  onDragOver(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
  }

  onFileDrop(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    const files = event.dataTransfer?.files;
    if (files && files.length > 0) {
      this.uploadFile(files[0]);
    }
  }

  uploadFile(file: File): void {
    this.uploading = true;
    this.uploadProgress = 0;
    this.uploadError = '';

    this.pieceService.uploadStl(this.piece!.id, file).subscribe({
      next: (event) => {
        switch (event.type) {
          case HttpEventType.UploadProgress:
            if (event.total) {
              this.uploadProgress = Math.round((100 * event.loaded) / event.total);
            }
            break;
          case HttpEventType.Response:
            this.uploadProgress = 100;
            setTimeout(() => {
              this.uploading = false;
              this.piece!.stlFileName = event.body!.fileName;
              this.cdr.detectChanges();
            }, 300);
            break;
        }
      },
      error: (err) => {
        this.uploading = false;
        this.uploadError = err.error?.error || 'Erreur lors de l\'upload';
      }
    });
  }

  retryUpload(): void {
    this.uploadError = '';
    // Re-sélectionner le fichier (à implémenter)
  }

  downloadStl(): void {
    if (this.piece?.stlFileName) {
      window.open(this.pieceService.getStlUrl(this.piece.id), '_blank');
    }
  }
  previousStatus(): void {
    if (!this.piece) return;
    const statuts = Object.values(PieceStatus);
    const index = statuts.indexOf(this.piece.statut);
    if (index > 0) {
      this.pieceService.updateStatus(this.piece.id, statuts[index - 1]).subscribe({
        next: (updated) => { this.piece = updated; }
      });
    }
  }
  rendreCommercialisable(): void {
    if (!this.piece) return;
    this.pieceService.updateStatus(this.piece.id, PieceStatus.Commercialisable).subscribe({
      next: (updated) => { this.piece = updated; }
    });
  }
  getStock(): number {
    return this.piece?.stock ?? 0;
  }
  getTempsImpression(): string {
    const minutes = Math.round((this.pricing?.costing.printTimeHours ?? this.getCoutTotal() * 0.25) * 60);
    const heures = Math.floor(minutes / 60);
    const mins = minutes % 60;
    return `${heures}h${mins.toString().padStart(2, '0')}`;
  }

  getPrixRecommande(): number {
    return this.pricing?.recommendedPriceHt ?? this.prixRecommande ?? this.getCoutTotal() * 1.3;
  }
  getElectricite(): number {
    return this.pricing?.electricityCost ?? this.getCoutTotal() * 0.05;
  }
  getMatiereNom(): string {
    if (!this.pricing) {
      return `${this.piece?.materiau || 'PLA'}`;
    }

    return `${this.pricing.costing.materialName} - ${(this.pricing.costing.quantityKg * 1000).toFixed(0)}g`;
  }

  getCoutRevientV2(): number {
    return this.pricing?.costWithWaste ?? this.getCoutTotal();
  }

  getMargeV2(): number {
    return this.pricing?.realProfit ?? this.getMarge();
  }

  getMargePourcentageV2(): number {
    const marginRate = this.pricing?.realMarginRate ?? this.pricing?.grossMarginRate;
    return marginRate !== undefined && marginRate !== null ? marginRate * 100 : this.getMargePourcentage();
  }
  getStatusClass(statut: string): string {
    const classes: Record<string, string> = {
      'Brouillon': 'status-brouillon',
      'Conception': 'status-conception',
      'Prototypage': 'status-prototypage',
      'Validation': 'status-validation',
      'Production': 'status-production',
      'Commercialisable': 'status-commercialisable'
    };
    return classes[statut] || 'status-brouillon';
  }
  lancerImpression(): void {
    alert(`Impression lancée pour ${this.piece?.nom}`);
  }
  getStlUrl(): string {
    if (!this.piece?.stlFileName) return '';
    return this.pieceService.getStlUrl(this.piece.id);
  }
  exportPdf(): void {
    if (!this.piece) {
      console.warn("Impossible d'exporter : aucune pièce n'est chargée.");
      return;
    }
    this.exportService.exportPiecePdf(this.piece.id).subscribe({
      next: (blob) => {
        this.exportService.downloadPdf(blob, `Piece_${this.piece?.reference}.pdf`);
      },
      error: (err) => console.error('Erreur export PDF:', err)
    });
  }

  exportFicheProduitPdf(): void {
    if (!this.piece) {
      console.warn("Impossible d'exporter : aucune pièce n'est chargée.");
      return;
    }
    this.exportService.exportFicheProduitPdf(this.piece.id).subscribe({
      next: (blob) => {
        this.exportService.downloadPdf(blob, `FicheProduit_${this.piece?.reference}.pdf`);
      },
      error: (err) => console.error('Erreur export fiche produit:', err)
    });
  }

  exportSocialPng(format: 'square' | 'story'): void {
    if (!this.piece) {
      console.warn("Impossible d'exporter : aucune pièce n'est chargée.");
      return;
    }

    this.exportService.exportPieceSocialPng(this.piece.id, format).subscribe({
      next: (blob) => {
        this.exportService.downloadFile(blob, `Post_${this.piece?.reference}_${format}.png`);
      },
      error: (err) => console.error('Erreur export PNG réseaux sociaux:', err)
    });
  }

  exportSocialPdf(format: 'square' | 'story'): void {
    if (!this.piece) {
      console.warn("Impossible d'exporter : aucune pièce n'est chargée.");
      return;
    }

    this.exportService.exportPieceSocialPdf(this.piece.id, format).subscribe({
      next: (blob) => {
        this.exportService.downloadPdf(blob, `Post_${this.piece?.reference}_${format}.pdf`);
      },
      error: (err) => console.error('Erreur export PDF réseaux sociaux:', err)
    });
  }

  downloadTechnicalPlan(): void {
    if (!this.piece) {
      console.warn("Impossible d'exporter : aucune pièce n'est chargée.");
      return;
    }
    this.technicalPlanService.downloadPieceTechnicalPlan(this.piece.id);
  }
}


