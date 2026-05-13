import { CommonModule } from "@angular/common";
import { Component, OnInit } from "@angular/core";
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from "@angular/forms";
import { ActivatedRoute, Router, RouterModule } from "@angular/router";
import { PieceService } from "../../services/piece.service";
import { Piece } from "../../models/piece.model";

@Component({
  selector: 'app-piece-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './piece-form.html',
  styleUrls: ['./piece-form.css']
})
export class PieceForm implements OnInit {
  pieceForm!: FormGroup;
  isEditMode = false;
  pieceId?: number;
  isSubmitting = false;
  selectedFile: File | null = null;
  
  // Calculs en temps réel
  coutTotal = 0;
  prixRecommande = 0;

  constructor(
    private fb: FormBuilder,
    private pieceService: PieceService,
    private router: Router,
    private route: ActivatedRoute
  ) {}

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
      reference: [''],
      description: [''],
      coutMatiere: [0, [Validators.min(0)]],
      coutMachine: [0, [Validators.min(0)]],
      coutMainOeuvre: [0, [Validators.min(0)]],
      prixVente: [0, [Validators.min(0)]],
      statut: ['Brouillon'],
      stlFileName: ['']
    });
    
    // Écouter les changements de coûts
    this.pieceForm.valueChanges.subscribe(() => {
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
          stlFileName: piece.stlFileName
        });
        this.updateCoutTotal();
      },
      error: (err) => console.error('Erreur chargement:', err)
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
    
    this.isSubmitting = true;
    
    const formValue = this.pieceForm.value;
    
    // Générer une référence si vide
    if (!formValue.reference) {
      formValue.reference = `P-${Date.now()}`;
    }
    
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
          this.router.navigate(['/pieces', this.pieceId]);
        },
        error: (err) => {
          console.error('Erreur mise à jour:', err);
          this.isSubmitting = false;
        }
      });
    } else {
      // Mode création
      this.pieceService.create(formValue).subscribe({
        next: (newPiece) => {
          this.isSubmitting = false;
          
          // Si un fichier STL est sélectionné, l'uploader
          if (this.selectedFile) {
            this.uploadStl(newPiece.id, this.selectedFile);
          } else {
            this.router.navigate(['/pieces', newPiece.id]);
          }
        },
        error: (err) => {
          console.error('Erreur création:', err);
          this.isSubmitting = false;
        }
      });
    }
  }

  uploadStl(pieceId: number, file: File): void {
    // À implémenter quand l'upload STL sera disponible
    console.log('Upload STL à implémenter:', pieceId, file.name);
    this.router.navigate(['/pieces', pieceId]);
  }
}