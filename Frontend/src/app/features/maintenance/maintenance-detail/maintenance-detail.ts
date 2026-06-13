import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { PrinterMaintenance } from '../../../models/maintenance.model';
import { PrinterMaintenanceService } from '../../../services/printer-maintenance.service';

@Component({
  selector: 'app-maintenance-detail',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './maintenance-detail.html',
  styleUrl: './maintenance-detail.css',
})
export class MaintenanceDetail implements OnInit {
  maintenance: PrinterMaintenance | null = null;

  constructor(
    private route: ActivatedRoute,
    private maintenanceService: PrinterMaintenanceService,
    private router: Router,
    private cdr: ChangeDetectorRef
  ) { }

  ngOnInit(): void {
    const id = this.route.snapshot.params['id'];
    if (id) this.loadMaintenance(id);
  }
  loadMaintenance(id: number): void {
    this.maintenanceService.getById(id).subscribe({
      next: (maintenance) => {
        this.maintenance = maintenance;
        this.cdr.detectChanges();
      },
      error: (error) => {
        console.error('Error loading maintenance:', error);
      }
    });
  }
    getStatusLabel(status: string): string {
    const labels: Record<string, string> = {
      'Scheduled': 'Planifiée',
      'InProgress': 'En cours',
      'Completed': 'Terminée',
      'Cancelled': 'Annulée'
    };
    return labels[status] || status;
  }

  completeMaintenance(): void {
    const performedBy = prompt('Nom de la personne ayant réalisé la maintenance:');
    if (performedBy) {
      this.maintenanceService.complete(this.maintenance!.id, undefined, performedBy).subscribe({
        next: () => this.loadMaintenance(this.maintenance!.id),
        error: (err) => console.error(err)
      });
    }
  }

  cancelMaintenance(): void {
    if (confirm('Annuler cette maintenance ?')) {
      this.maintenanceService.cancel(this.maintenance!.id).subscribe({
        next: () => this.loadMaintenance(this.maintenance!.id),
        error: (err) => console.error(err)
      });
    }
  }

  deleteMaintenance(): void {
    if (confirm('Supprimer cette maintenance ?')) {
      this.maintenanceService.delete(this.maintenance!.id).subscribe({
        next: () => this.router.navigate(['/maintenances']),
        error: (err) => console.error(err)
      });
    }
  }
}
