import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component } from '@angular/core';
import { FormArray, FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { Client } from '../../../models/client.model';
import { Piece } from '../../../models/piece.model';
import { DevisService } from '../../../services/devis.service';
import { ClientService } from '../../../services/client.service';
import { PieceService } from '../../../services/piece.service';

@Component({
  selector: 'app-devis-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './devis-form.html',
  styleUrl: './devis-form.css',
})
export class DevisForm {
  devisForm!: FormGroup;
  isEditMode = false;
  devisId?: number;
  clients: Client[] = [];
  pieces: Piece[] = [];

  constructor(
    private fb: FormBuilder,
    private devisService: DevisService,
    private clientService: ClientService,
    private pieceService: PieceService,
    private route: ActivatedRoute,
    private router: Router,
    private cdr: ChangeDetectorRef
  ) { }
  ngOnInit(): void {
    this.devisForm = this.fb.group({
      clientId: ['', Validators.required],
      dateValidite: ['', Validators.required],
      tva: [20],
      notes: [''],
      conditions: [''],
      lignes: this.fb.array([])
    });
    this.loadClients(); this.loadPieces();
    this.devisId = this.route.snapshot.params['id'];
    if (this.devisId) { this.isEditMode = true; this.loadDevis(); }
    else this.addLigne();
  }
  get lignes() {
    return this.devisForm.get('lignes') as FormArray;
  }
  addLigne() {
    this.lignes.push(this.fb.group({
      pieceId: ['', Validators.required],
      description: ['', Validators.required],
      quantite: [1, Validators.min(1)],
      prixUnitaire: [0, Validators.min(0)]
    }));
  }
  removeLigne(index: number) {
    this.lignes.removeAt(index);
  }
  onPieceSelect(index: number) {
    const pieceId = this.lignes.at(index).get('pieceId')?.value;
    const piece = this.pieces.find(p => p.id === parseInt(pieceId));
    if (piece) {
      this.lignes.at(index).patchValue({
        description: piece.nom,
        prixUnitaire: piece.prixVente
      });
    }
  }
  getLigneTotal(index: number): number {
    const ligne = this.lignes.at(index).value;
    return (ligne.quantite || 0) * (ligne.prixUnitaire || 0);
  }
  getTotalHT(): number { let total = 0; for (let i = 0; i < this.lignes.length; i++) total += this.getLigneTotal(i); return total; }

  getTVA(): number { return this.getTotalHT() * (this.devisForm.get('tva')?.value || 0) / 100; }

  getTotalTTC(): number { return this.getTotalHT() + this.getTVA(); }
  loadClients() {
    this.clientService.getAll().subscribe({
      next: (data) => {
        this.clients = data;
        this.cdr.detectChanges();
      },
      error: (error) => {
        console.error('Erreur lors du chargement des clients', error);
      },
    });
  }
  loadPieces() {
    this.pieceService.getAll().subscribe({
      next: (data) => {
        this.pieces = data;
        this.cdr.detectChanges();
      },
      error: (error) => {
        console.error('Erreur lors du chargement des pièces', error);
      },
    });
  }
  loadDevis() { this.devisService.getById(this.devisId!).subscribe(data => { this.devisForm.patchValue(data); while (this.lignes.length) this.lignes.removeAt(0); data.lignes.forEach(l => this.lignes.push(this.fb.group(l))); }); }
  onSubmit() {
    if (this.devisForm.invalid)
      return;
    const devisData = this.devisForm.value;
    if (this.isEditMode && this.devisId) {
      this.devisService.update(this.devisId, devisData).subscribe({
        next: () => this.router.navigate(['/devis']),
        error: (err) => console.error(err)
      });
    } else {
      this.devisService.create(devisData).subscribe({
        next: () => this.router.navigate(['/devis']),
        error: (err) => console.error(err)
      });
    }
  }
}