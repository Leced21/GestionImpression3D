import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { IncidentStatus, PrintIncident } from '../../../models/print-incident.model';
import { PrintIncidentService } from '../../../services/print-incident.service';

@Component({
  selector: 'app-incident-detail',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './incident-detail.html',
  styleUrl: './incident-detail.css',
})
export class IncidentDetail implements OnInit{
  incident:PrintIncident | null = null;
  protected readonly IncidentStatus = IncidentStatus
  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private incidentService: PrintIncidentService,
    private cdr: ChangeDetectorRef
  ) { }
  ngOnInit(): void {
    const id = this.route.snapshot.params['id'];
    if (id) this.loadIncident(id);
  }
  loadIncident(id: number): void {
    this.incidentService.getById(id).subscribe({
      next: (incident) => {
        this.incident = incident;
        this.cdr.detectChanges();
      },
      error: (error) => {
        console.error('Error loading incident:', error);
      }
    });
  }
  updateStatus(status: IncidentStatus): void {
    this.incidentService.updateStatus(this.incident!.id, status).subscribe({
      next: () => this.loadIncident(this.incident!.id)
    });
  }

  resolveIncident(): void {
    const resolution = prompt('Décrivez la résolution:');
    if (resolution) {
      this.incidentService.resolve(this.incident!.id, resolution).subscribe({
        next: () => this.loadIncident(this.incident!.id)
      });
    }
  }

  deleteIncident(): void {
    if (confirm('Supprimer cet incident ?')) {
      this.incidentService.delete(this.incident!.id).subscribe({
        next: () => this.router.navigate(['/incidents'])
      });
    }
  }
}
