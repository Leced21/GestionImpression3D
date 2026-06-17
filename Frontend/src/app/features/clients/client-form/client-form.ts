import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { ClientService } from '../../../services/client.service';

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

  constructor(
    private fb: FormBuilder,
    private clientService: ClientService,
    private route: ActivatedRoute,
    private router: Router,
    private cdr: ChangeDetectorRef
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
    if (this.clientForm.invalid) return;
    const clientData = this.clientForm.value;
    if (this.isEditMode && this.clientId) {
      this.clientService.update(this.clientId, clientData).subscribe({
        next: () => {
          this.router.navigate(['/clients']);
        },
        error: (err) => console.error(err)
      });
    }else{
      this.clientService.create(clientData).subscribe({
        next:()=>{
          this.router.navigate(['/clients']);
        },
        error:(err)=>console.error(err)
      }); 
    }
  }
}
