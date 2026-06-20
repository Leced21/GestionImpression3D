import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { IncidentSeverity, IncidentStatistics, IncidentStatus, PrintIncident } from '../../../models/print-incident.model';
import { PrintIncidentService } from '../../../services/print-incident.service';

@Component({
  selector: 'app-incident-list',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './incident-list.html',
  styleUrl: './incident-list.css',
})
export class IncidentList implements OnInit {
  incidents: PrintIncident[] = [];
  filteredIncidents: PrintIncident[] = [];
  statistics?: IncidentStatistics;
  statusFilter = '';
  severityFilter = '';
  protected readonly IncidentStatus = IncidentStatus;
  protected readonly IncidentSeverity = IncidentSeverity;


  constructor(
    private incidentService: PrintIncidentService,
    private cdr: ChangeDetectorRef
  ) { }

  ngOnInit(): void {
    this.loadIncidents();
    this.loadStatistics();
  }

  loadIncidents(): void {
    this.incidentService.getAll().subscribe({
      next: (data) => {
        this.incidents = data;
        this.filterIncidents();
        this.cdr.detectChanges();
      },
      error: (error) => {
        console.error('Erreur lors du chargement des incidents', error);
      },
    });
  }

  loadStatistics(): void {
    this.incidentService.getStatistics().subscribe({
      next: (statistics) => {
        this.statistics = statistics;
        this.cdr.detectChanges();
      },
      error: (error) => {
        console.error('Erreur lors du chargement des statistiques', error);
      },
    });
  }
  filterIncidents(): void {
    this.filteredIncidents = this.incidents.filter(i => {
      const matchStatus = !this.statusFilter || i.status === this.statusFilter;
      const matchSeverity = !this.severityFilter || i.severity === this.severityFilter;
      return matchStatus && matchSeverity;
    });
  }

  getSeverityClass(severity: IncidentSeverity): string {
    return severity;
  }

  getStatusClass(status: IncidentStatus): string {
    return status.replace(' ', '-');
  }

  updateStatus(incident: PrintIncident, status: IncidentStatus): void {
    this.incidentService.updateStatus(incident.id, status).subscribe({
      next: () => this.loadIncidents()
    });
  }

  resolveIncident(incident: PrintIncident): void {
    const resolution = prompt('Décrivez la résolution:');
    if (resolution) {
      this.incidentService.resolve(incident.id, resolution).subscribe({
        next: () => this.loadIncidents()
      });
    }
  }

  refresh(): void {
    this.loadIncidents();
    this.loadStatistics();
  }
}
