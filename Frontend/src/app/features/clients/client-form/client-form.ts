import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { ClientService } from '../../../services/client.service';
import { ToastService } from '../../../services/toast.service';

@Component({
  selector: 'app-client-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './client-form.html',
  styleUrl: './client-form.css',
})
export class ClientForm implements OnInit {
  clientForm!: FormGroup;
  isEditMode: boolean = false;
  clientId?: number;
  isSubmitting = false;

  constructor(
    private fb: FormBuilder,
    private clientService: ClientService,
    private route: ActivatedRoute,
    private router: Router,
    private cdr: ChangeDetectorRef,
    private toast: ToastService
  ) { }
  ngOnInit(): void {
    this.clientForm = this.fb.group({
      nom: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]],
      telephone: [''],
      adresse: [''],
      codePostal: [''],
      ville: [''],
      pays: ['France'],
      siret: [''],
      tvaIntra: [''],
      notes: [''],
      isActive: [true]
    });
    this.clientId = this.route.snapshot.params['id'];
    if (this.clientId) {
      this.isEditMode = true;
      this.loadClient();
    }
  }
  loadClient(): void {
    if (!this.clientId) return;
    this.clientService.getById(this.clientId).subscribe({
      next: (data) => {
        this.clientForm.patchValue(data);
        this.cdr.detectChanges();
      },
      error: (error) => {
        console.error('Erreur lors du chargement du client', error);
      },
    });
  }
  onSubmit(): void {
    if (this.clientForm.invalid) {
      this.clientForm.markAllAsTouched();
      this.toast.warn('Complète le nom et l’email du client.');
      return;
    }

    const value = this.clientForm.value;
    const clientData = {
      ...value,
      nom: String(value.nom ?? '').trim(),
      email: String(value.email ?? '').trim().toLowerCase(),
      telephone: String(value.telephone ?? '').trim(),
      adresse: String(value.adresse ?? '').trim(),
      codePostal: String(value.codePostal ?? '').trim(),
      ville: String(value.ville ?? '').trim(),
      pays: String(value.pays || 'France').trim(),
      siret: value.siret ? String(value.siret).trim() : null,
      tvaIntra: value.tvaIntra ? String(value.tvaIntra).trim() : null,
      notes: value.notes ? String(value.notes).trim() : null
    };

    this.isSubmitting = true;
    if (this.isEditMode && this.clientId) {
      this.clientService.update(this.clientId, clientData).subscribe({
        next: () => {
          this.toast.success('Client mis à jour');
          this.router.navigate(['/clients']);
        },
        error: (err) => {
          this.isSubmitting = false;
          this.toast.error(this.extractErrorMessage(err));
        }
      });
    }else{
      this.clientService.create(clientData).subscribe({
        next:()=>{
          this.toast.success('Client créé');
          this.router.navigate(['/clients']);
        },
        error:(err)=>{
          this.isSubmitting = false;
          this.toast.error(this.extractErrorMessage(err));
        }
      }); 
    }
  }

  private extractErrorMessage(err: any): string {
    if (err?.error?.error) return err.error.error;
    if (err?.error?.title) return err.error.title;
    if (err?.status === 403) return 'Tu n’as pas les droits pour créer ou modifier un client.';
    return 'Impossible d’enregistrer le client.';
  }
}
