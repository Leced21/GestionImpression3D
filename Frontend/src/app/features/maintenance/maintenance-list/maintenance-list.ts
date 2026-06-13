import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { RouterModule } from '@angular/router';
import { MaintenanceStatus, PrinterMaintenance } from '../../../models/maintenance.model';
import { PrinterMaintenanceService } from '../../../services/printer-maintenance.service';

@Component({
  selector: 'app-maintenance-list',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './maintenance-list.html',
  styleUrl: './maintenance-list.css',
})
export class MaintenanceList implements OnInit {
  maintenances: PrinterMaintenance[] = [];
  activeTab = 'all';

  get filteredMaintenances(): PrinterMaintenance[] {
    switch (this.activeTab) {
      case 'upcoming': return this.maintenances.filter(m => m.status === MaintenanceStatus.Scheduled);
      case 'completed': return this.maintenances.filter(m => m.status === MaintenanceStatus.Completed);
      case 'inprogress': return this.maintenances.filter(m => m.status === MaintenanceStatus.InProgress);
      default: return this.maintenances;
    }
  }
  constructor(
    private maintenanceService: PrinterMaintenanceService,
    private cdr: ChangeDetectorRef
  ) { }
  ngOnInit(): void {
    this.loadMaintenances();
  }
  loadMaintenances(): void {
    this.maintenanceService.getAll().subscribe({
      next: (data) => {
        this.maintenances = data;
        this.cdr.detectChanges();
      },
      error: (error) => {
        console.error('Erreur lors du chargement des maintenances', error);
      },
    });
  }
  getTypeIcon(type: string): string {
    const icons: Record<string, string> = {
      'Preventive': '🔧',
      'Corrective': '⚠️',
      'Calibration': '📏',
      'Cleaning': '🧹'
    };
    return icons[type] || '🔧';
  }

  getTypeClass(type: string): string {
    return type;
  }

  getStatusClass(status: string): string {
    return status;
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

  completeMaintenance(maintenance: PrinterMaintenance): void {
    const performedBy = prompt('Nom de la personne ayant réalisé la maintenance:');
    if (performedBy) {
      this.maintenanceService.complete(maintenance.id, undefined, performedBy).subscribe({
        next: () => this.loadMaintenances(),
        error: (err) => console.error(err)
      });
    }
  }

  cancelMaintenance(maintenance: PrinterMaintenance): void {
    if (confirm('Annuler cette maintenance ?')) {
      this.maintenanceService.cancel(maintenance.id).subscribe({
        next: () => this.loadMaintenances(),
        error: (err) => console.error(err)
      });
    }
  }
}
