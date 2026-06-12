import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { OrdreFabrication, OrdreStatus } from '../../../models/ordre-fabrication.model';
import { OrdreFabricationService } from '../../../services/ordre-fabrication.service';

@Component({
  selector: 'app-ordre-detail',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './ordre-detail.html',
  styleUrl: './ordre-detail.css',
})
export class OrdreDetail implements OnInit {
  ordre: OrdreFabrication | null = null;
  OrdreStatus = OrdreStatus;
  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private ordreService: OrdreFabricationService,
    private cdr: ChangeDetectorRef
  ) { }
  ngOnInit(): void {
    const id = this.route.snapshot.params['id'];
    if (id) this.loadOrdre(id);
  }
  loadOrdre(id: number): void {
    this.ordreService.getById(id).subscribe({
      next: (data) => {
        this.ordre = data;
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Erreur lors du chargement de l\'ordre de fabrication', err)
      }
    });
  }
  getStatusClass(statut: string): string {
    return `status-${statut.replace(' ', '-')}`;
  }
  startProduction(): void {
    this.ordreService.startProduction(this.ordre!.id).subscribe(() => this.loadOrdre(this.ordre!.id));
  }
  completeProduction(): void {
    this.ordreService.completeProduction(this.ordre!.id).subscribe(() => this.loadOrdre(this.ordre!.id));
  }
  deleteOrdre(): void {
    if (confirm('Supprimer ?')) this.ordreService.delete(this.ordre!.id).subscribe(() => this.router.navigate(['/ordres']));
  }
}
