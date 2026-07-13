import { CommonModule } from "@angular/common";
import { ChangeDetectorRef, Component, OnDestroy, OnInit } from "@angular/core";
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from "@angular/forms";
import { ActivatedRoute, Router, RouterModule } from "@angular/router";
import { Subject, takeUntil } from "rxjs";
import { PieceService } from "../../services/piece.service";
import { Piece } from "../../models/piece.model";
import { ToastService } from "../../services/toast.service";

@Component({
  selector: 'app-piece-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './piece-form.html',
  styleUrls: ['./piece-form.css']
})
export class PieceForm implements OnInit, OnDestroy {
  pieceForm!: FormGroup;
  isEditMode = false;
  pieceId?: number;
  isSubmitting = false;
  submissionError: string = '';
  selectedFile: File | null = null;
  private destroy$ = new Subject<void>();

  // Calculs en temps réel
  coutTotal = 0;
  prixRecommande = 0;

  constructor(
    private fb: FormBuilder,
    private pieceService: PieceService,
    private router: Router,
    private route: ActivatedRoute,
    private cdr: ChangeDetectorRef,
    private toast: ToastService
  ) { }

  ngOnInit(): void {
    this.initForm();

    // Vérifier si on est en mode édition
    this.pieceId = this.route.snapshot.params['id'];
    if (this.pieceId) {
      this.isEditMode = true;
      this.loadPiece(this.pieceId);
    }
  }

  initForm(): void {
    this.pieceForm = this.fb.group({
      nom: ['', Validators.required],
      reference: ['', [Validators.pattern(/^[A-Z]{3}-\d{3}$/)]],
      description: [''],
      coutMatiere: [0, [Validators.min(0)]],
      coutMachine: [0, [Validators.min(0)]],
      coutMainOeuvre: [0, [Validators.min(0)]],
      prixVente: [0, [Validators.min(0)]],
      statut: ['Brouillon'],
      stlFileName: [''],
      categorie: ['Mecanique'],
      materiau: ['PLA'],
      stock: [0],
      estDisponible: [true],
      couleurs: [''],
      capaciteContenance: [''],
      normesCertifications: [''],
      instructionsUtilisation: [''],
      precautionsUsage: [''],
      publicCible: [''],
      conditionnement: [''],
      dimensionsColis: [''],
      poidsColisKg: [null],
      moqUnites: [null],
      delaiLivraisonJours: [null],
      pointsForts: [''],
      faq: [''],
      tarifsDegressifs: ['']
    });

    // Écouter les changements de coûts
    this.pieceForm.valueChanges.pipe(takeUntil(this.destroy$)).subscribe(() => {
      this.updateCoutTotal();
    });
  }

  loadPiece(id: number): void {
    this.pieceService.getById(id).subscribe({
      next: (piece: Piece) => {
        this.pieceForm.patchValue({
          nom: piece.nom,
          reference: piece.reference,
          description: piece.description,
          coutMatiere: piece.coutMatiere,
          coutMachine: piece.coutMachine,
          coutMainOeuvre: piece.coutMainOeuvre,
          prixVente: piece.prixVente,
          statut: piece.statut,
          stlFileName: piece.stlFileName,
          categorie: piece.categorie,
          materiau: piece.materiau,
          stock: piece.stock,
          estDisponible: piece.estDisponible,
          couleurs: piece.couleurs,
          capaciteContenance: piece.capaciteContenance,
          normesCertifications: piece.normesCertifications,
          instructionsUtilisation: piece.instructionsUtilisation,
          precautionsUsage: piece.precautionsUsage,
          publicCible: piece.publicCible,
          conditionnement: piece.conditionnement,
          dimensionsColis: piece.dimensionsColis,
          poidsColisKg: piece.poidsColisKg,
          moqUnites: piece.moqUnites,
          delaiLivraisonJours: piece.delaiLivraisonJours,
          pointsForts: piece.pointsForts,
          faq: piece.faq,
          tarifsDegressifs: piece.tarifsDegressifs
        });
        this.updateCoutTotal();
        this.cdr.detectChanges(); // Assure que les changements sont pris en compte immédiatement
      },
      error: () => this.toast.error('Impossible de charger la pièce')
    });
  }

  updateCoutTotal(): void {
    const values = this.pieceForm.value;
    this.coutTotal = (values.coutMatiere || 0) + (values.coutMachine || 0) + (values.coutMainOeuvre || 0);
    this.prixRecommande = this.coutTotal * 1.3;
  }

  getCoutTotal(): number {
    return this.coutTotal;
  }

  getPrixRecommande(): number {
    return this.prixRecommande;
  }

  appliquerPrixRecommande(): void {
    this.pieceForm.patchValue({ prixVente: Math.round(this.prixRecommande * 100) / 100 });
  }

  get marge(): number {
    const prixVente = this.pieceForm.get('prixVente')?.value || 0;
    return prixVente - this.coutTotal;
  }

  get margePourcentage(): number {
    return this.coutTotal > 0 ? (this.marge / this.coutTotal) * 100 : 0;
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      this.selectedFile = input.files[0];
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
      this.selectedFile = files[0];
    }
  }

  removeFile(event: Event): void {
    event.stopPropagation();
    this.selectedFile = null;
    this.pieceForm.patchValue({ stlFileName: '' });
  }

  onSubmit(): void {
    if (this.pieceForm.invalid) return;

    this.submissionError = '';
    this.isSubmitting = true;

    const formValue = this.pieceForm.value;
    // Si vide, le backend génère automatiquement une référence à partir de la
    // catégorie (ex: MEC-001) ; sinon elle doit suivre le format XXX-000.

    if (this.isEditMode && this.pieceId) {
      // Mode édition
      const updatedPiece: Piece = {
        id: this.pieceId,
        ...formValue,
        dateCreation: new Date()
      };

      this.pieceService.update(this.pieceId, updatedPiece).subscribe({
        next: () => {
          this.isSubmitting = false;
          this.toast.success('Pièce mise à jour');
          this.router.navigate(['/pieces', this.pieceId]);
        },
        error: (err) => {
          this.submissionError = err?.message || 'Erreur lors de la mise à jour';
          this.isSubmitting = false;
        }
      });
    } else {
      // Mode création
      this.pieceService.create(formValue).subscribe({
        next: (newPiece) => {
          this.isSubmitting = false;
          this.toast.success('Pièce créée');

          // Si un fichier STL est sélectionné, l'uploader
          if (this.selectedFile) {
            this.uploadStl(newPiece.id, this.selectedFile);
          } else {
            this.router.navigate(['/pieces', newPiece.id]);
          }
        },
        error: (err) => {
          this.submissionError = err?.message || 'Erreur lors de la création';
          this.isSubmitting = false;
        }
      });
    }
  }

  uploadStl(pieceId: number, file: File): void {
    this.toast.info(`Fichier ${file.name} sélectionné. Upload STL à finaliser.`);
    this.router.navigate(['/pieces', pieceId]);
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }
}
