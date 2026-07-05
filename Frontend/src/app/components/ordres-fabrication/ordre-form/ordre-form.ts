import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { PieceService } from '../../../services/piece.service';
import { ProjetService } from '../../../services/projet.service';
import { OrdreFabricationService } from '../../../services/ordre-fabrication.service';
import { Piece } from '../../../models/piece.model';
import { Projet } from '../../../models/projet.model';

@Component({
  selector: 'app-ordre-form',
  standalone: true,
  imports: [CommonModule, RouterModule, ReactiveFormsModule],
  templateUrl: './ordre-form.html',
  styleUrl: './ordre-form.css',
})
export class OrdreForm implements OnInit {
  ordreForm!: FormGroup;
  isEditMode = false;
  ordreId?: number;
  projets: Projet[] = [];
  pieces: Piece[] = [];
  allPieces: Piece[] = [];

  constructor(
    private fb: FormBuilder,
    private ordreService: OrdreFabricationService,
    private projetService: ProjetService,
    private pieceService: PieceService,
    private router: Router,
    private route: ActivatedRoute,
    private cdr: ChangeDetectorRef
  ) { }
  ngOnInit(): void {
    this.initForm();
    this.loadProjets();
    this.loadPieces();
    this.ordreId = this.route.snapshot.params['id'];
    if (this.ordreId) {
      this.isEditMode = true;
      this.loadOrdre();
    }
  }
  initForm(): void {
    this.ordreForm = this.fb.group({
      projetId: ['', Validators.required],
      pieceId: ['', Validators.required],
      quantite: [1, [Validators.required, Validators.min(1)]],
      priorite: ['Normale'],
      dateEcheance: [''],
      notes: [''],
    });
  }
  loadProjets(): void {
    this.projetService.getAll().subscribe({
      next: (data) => {
        this.projets = data;
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Erreur lors du chargement des projets', err)
      }
    });
  }
  loadPieces(): void {
    this.pieceService.getAll().subscribe({
      next: (data) => {
        this.allPieces = data;
        this.pieces = data;
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Erreur lors du chargement des pièces', err)
      }
    });
  }
  loadOrdre(): void {
    if (!this.ordreId) return;
    this.ordreService.getById(this.ordreId).subscribe({
      next: (data) => {
        this.ordreForm.patchValue({
          projetId: data.projetId,
          pieceId: data.pieceId,
          quantite: data.quantite,
          priorite: data.priorite,
          dateEcheance: data.dateEcheance ? new Date(data.dateEcheance).toISOString().substring(0, 10) : '',
          notes: data.notes,
        });
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Erreur lors du chargement de l\'ordre de fabrication', err)
      }
    });
  }
  onProjetChange(): void {
    const projetId = this.ordreForm.get('projetId')?.value;
    if (projetId) {
      this.projetService.getById(projetId).subscribe({
        next: (projet) => {
          const pieceIds = projet.projetPieces?.map(pp => pp.pieceId) || [];
          this.pieces = this.allPieces.filter(p => pieceIds.includes(p.id));
          this.cdr.detectChanges();
        },
        error: (err) => {
          console.error('Erreur lors du chargement du projet', err)
        }
      });
    } else {
      this.loadPieces();
    }
  }
  onSubmit(): void {
    if (this.ordreForm.invalid) return;

    const formValue = this.ordreForm.getRawValue();
    const payload = {
      ...formValue,
      projetId: Number(formValue.projetId),
      pieceId: Number(formValue.pieceId),
      quantite: Number(formValue.quantite),
      dateEcheance: formValue.dateEcheance || null,
      notes: formValue.notes?.trim() || null,
    };

    if (this.isEditMode && this.ordreId) {
      this.ordreService.update(this.ordreId, payload).subscribe({
        next: () => this.router.navigate(['/ordres']),
        error: (err) => console.error('Erreur lors de la modification de l\'ordre', err.error?.errors ?? err)
      });
    } else {
      this.ordreService.create(payload).subscribe({
        next: () => this.router.navigate(['/ordres']),
        error: (err) => console.error('Erreur lors de la création de l\'ordre', err.error?.errors ?? err)
      });
    }
  }
}
