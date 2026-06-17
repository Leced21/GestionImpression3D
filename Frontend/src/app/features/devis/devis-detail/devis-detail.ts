import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { Devis, DevisStatus } from '../../../models/devis.model';
import { DevisService } from '../../../services/devis.service';

@Component({
  selector: 'app-devis-detail',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './devis-detail.html',
  styleUrl: './devis-detail.css',
})
export class DevisDetail implements OnInit {
  devis: Devis | null = null;
  protected readonly DevisStatus = DevisStatus

  constructor(
    private route: ActivatedRoute,
    private devisService: DevisService,
    private router: Router,
    private cdr: ChangeDetectorRef
  ) { }
  ngOnInit(): void {
    const id = this.route.snapshot.params['id'];
    if (id) this.loadDevis(id);
  }
  loadDevis(id: number): void {
    this.devisService.getById(id).subscribe({
      next: (devis) => {
        this.devis = devis;
        this.cdr.detectChanges();
      },
      error: (error) => {
        console.error('Error loading devis:', error);
      },
    });
  }
  getTVA(): number { return this.devis ? (this.devis.totalHT * this.devis.tva / 100) : 0; }

  updateStatut(statut: DevisStatus): void { this.devisService.updateStatut(this.devis!.id, statut).subscribe({ next: () => this.loadDevis(this.devis!.id) }); }

  generatePdf(): void { this.devisService.generatePdf(this.devis!.id).subscribe({ next: (blob) => { const url = window.URL.createObjectURL(blob); const link = document.createElement('a'); link.href = url; link.download = `Devis_${this.devis!.numeroDevis}.pdf`; link.click(); window.URL.revokeObjectURL(url); } }); }

  deleteDevis(): void { if (confirm('Supprimer ce devis ?')) this.devisService.delete(this.devis!.id).subscribe(() => this.router.navigate(['/devis'])); }
}
