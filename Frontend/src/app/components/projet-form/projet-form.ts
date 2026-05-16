import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, OnDestroy, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { ProjetService } from '../../services/projet.service';
import { Subject, takeUntil } from 'rxjs';

@Component({
  selector: 'app-projet-form',
  standalone: true,
  imports: [CommonModule, RouterModule, ReactiveFormsModule],
  templateUrl: './projet-form.html',
  styleUrl: './projet-form.css',
})
export class ProjetForm implements OnInit, OnDestroy {

  projetForm !: FormGroup;
  isEditMode: boolean = false;
  projetId?: number;
  isSubmitting: boolean = false;
  submissionError?: string;
  private destroy$ = new Subject<void>();

  constructor(
    private fb: FormBuilder,
    private projetService: ProjetService,
    private router: Router,
    private route: ActivatedRoute,
    private cdr: ChangeDetectorRef
  ){}
  ngOnInit(): void {
    this.initForm();

    this.projetId = this.route.snapshot.params['id'];
    if (this.projetId) {
      this.isEditMode = true;
      this.loadProjet(this.projetId);
    }
  }

  initForm(): void {
    this.projetForm = this.fb.group({
      nom: ['', Validators.required],
      reference: [''],
      description: [''],
      clientNom: [''],
      clientEmail: ['', Validators.email],
      dateLivraisonPrevue: [''],
      budget: [0],
      statut: ['Brouillon']
    });
  }

  loadProjet(id: number): void {
    this.projetService.getById(id).pipe(takeUntil(this.destroy$)).subscribe({
      next: (projet) => {
        this.projetForm.patchValue({
          nom: projet.nom,
          reference: projet.reference,
          description: projet.description,
          clientNom: projet.clientNom,
          clientEmail: projet.clientEmail,
          dateLivraisonPrevue: projet.dateLivraisonPrevue?.toString().split('T')[0] || '',
          budget: projet.budget,
          statut: projet.statut
        });
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Erreur chargement:', err);
        this.submissionError = err?.message || 'Erreur lors du chargement du projet';
      }
    });
  }

  onSubmit(): void {
    if (this.projetForm.invalid) return;
    
    this.isSubmitting = true;
    const formValue = this.projetForm.value;
    
    if (this.isEditMode && this.projetId) {
      this.submissionError = undefined;
      this.projetService.update(this.projetId, formValue).pipe(takeUntil(this.destroy$)).subscribe({
        next: () => {
          this.isSubmitting = false;
          this.router.navigate(['/projets', this.projetId]);
        },
        error: (err) => {
          console.error('Erreur mise à jour:', err);
          this.submissionError = err?.message || 'Erreur lors de la mise à jour';
          this.isSubmitting = false;
        }
      });
    } else {
      this.submissionError = undefined;
      this.projetService.create(formValue).pipe(takeUntil(this.destroy$)).subscribe({
        next: (newProjet) => {
          this.isSubmitting = false;
          this.router.navigate(['/projets', newProjet.id]);
        },
        error: (err) => {
          console.error('Erreur création:', err);
          this.submissionError = err?.message || 'Erreur lors de la création';
          this.isSubmitting = false;
        }
      });
    }
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

}
