import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, OnDestroy, OnInit } from '@angular/core';
import { BaseChartDirective } from 'ng2-charts';
import { RouterModule } from '@angular/router';
import { GlobalStats, PrinterActivity, ProductionTrend } from '../../models/dashboard.model';
import { PrinterStatus } from '../../models/printer.model';
import { ChartConfiguration, ChartData } from 'chart.js';
import { DashboardService } from '../../services/dashboard.service';
import { PrintJobService } from '../../services/print-job.service';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, RouterModule, BaseChartDirective],
  templateUrl: './dashboard.html',
  styleUrls: ['./dashboard.css'],
})
export class Dashboard implements OnInit, OnDestroy {
  stats?: GlobalStats;
  productionTrend: ProductionTrend[] = [];
  printersActivity: PrinterActivity[] = [];
  recentJobs: any[] = [];
  lastUpdated: Date = new Date();
  showTutorial = false;
  private readonly tutorialStorageKey = 'printflow3d_first_visit_tutorial_seen';
  private refreshInterval: any;

  // Configuration graphiques
  lineChartOptions: ChartConfiguration['options'] = {
    responsive: true,
    maintainAspectRatio: true,
    plugins: { legend: { position: 'top' } }
  };
  
  doughnutChartOptions: ChartConfiguration['options'] = {
    responsive: true,
    maintainAspectRatio: true,
    plugins: { legend: { position: 'right' } }
  };
  
  barChartOptions: ChartConfiguration['options'] = {
    responsive: true,
    maintainAspectRatio: true,
    plugins: { legend: { position: 'top' } }
  };

  productionChartData: ChartData<'line'> = { labels: [], datasets: [] };
  printerStatusChartData: ChartData<'doughnut'> = { labels: [], datasets: [] };
  materialChartData: ChartData<'bar'> = { labels: [], datasets: [] };

  constructor(
    private dashboardService: DashboardService, 
    private printJobService: PrintJobService,
    private cdr: ChangeDetectorRef) {}

  ngOnInit(): void {
    this.showTutorial = !localStorage.getItem(this.tutorialStorageKey);
    this.loadAllData();
    this.refreshInterval = setInterval(() => this.loadAllData(), 30000);
  }

  ngOnDestroy(): void {
    if (this.refreshInterval) clearInterval(this.refreshInterval);
  }

  loadAllData(): void {
    this.loadGlobalStats();
    this.loadProductionTrend();
    this.loadPrintersActivity();
    this.loadMaterialConsumption();
    this.loadRecentJobs();
  }

  loadGlobalStats(): void {
    this.dashboardService.getGlobalStats().subscribe({
      next: (data) => {
        this.stats = data;
        this.cdr.detectChanges();
      },
      error: (err) => console.error(err)
    });
  }

  loadProductionTrend(): void {
    this.dashboardService.getProductionTrend(30).subscribe({
      next: (data) => {
        this.productionTrend = data;
        this.productionChartData = {
          labels: data.map(d => new Date(d.date).toLocaleDateString()),
          datasets: [
            { label: 'Terminés', data: data.map(d => d.completed), borderColor: '#10b981', fill: false },
            { label: 'Échoués', data: data.map(d => d.failed), borderColor: '#ef4444', fill: false }
          ]
        };
        this.cdr.detectChanges();
      },
      error: (err) => console.error(err)
    });
  }

  loadPrintersActivity(): void {
    this.dashboardService.getPrintersActivity().subscribe({
      next: (data) => {
        this.printersActivity = data.map(printer => ({
          ...printer,
          status: this.normalizePrinterStatus(printer.status)
        } as PrinterActivity));

        const statusCount = {
          Available: this.printersActivity.filter(p => p.status === 'Available').length,
          Printing: this.printersActivity.filter(p => p.status === 'Printing').length,
          Maintenance: this.printersActivity.filter(p => p.status === 'Maintenance').length,
          Offline: this.printersActivity.filter(p => p.status === 'Offline').length
        };
        this.printerStatusChartData = {
          labels: ['Disponibles', 'En impression', 'Maintenance', 'Hors ligne'],
          datasets: [{ data: [statusCount.Available, statusCount.Printing, statusCount.Maintenance, statusCount.Offline], backgroundColor: ['#10b981', '#3b82f6', '#f59e0b', '#64748b'] }]
        };
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error(err);
        this.cdr.detectChanges();
      }
    });
  }

  loadMaterialConsumption(): void {
    this.dashboardService.getMaterialConsumption(30).subscribe({
      next: (data) => {
        this.materialChartData = {
          labels: data.map(item => item.name),
          datasets: [{
            label: 'Consommation (unit�s)',
            data: data.map(item => item.quantity),
            backgroundColor: ['#3b82f6', '#10b981', '#f59e0b', '#8b5cf6', '#ef4444', '#22c55e']
          }]
        };
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error(err);
      }
    });
  }

  loadRecentJobs(): void {
    this.printJobService.getAll().subscribe({
      next: (data) => {
        this.recentJobs = data.slice(0, 10),
        this.cdr.detectChanges();
      },
      error: (err) => console.error(err)
    });
  }

  refresh(): void {
    this.loadAllData();
    this.lastUpdated = new Date();
  }

  getStatusClass(status: string): string {
    return `status-${status}`;
  }

  private normalizePrinterStatus(status?: string): PrinterStatus {
    const normalized = status?.toString().trim().toLowerCase() ?? '';
    if (['available', 'disponible'].includes(normalized)) return PrinterStatus.Available;
    if (['printing', 'en impression'].includes(normalized)) return PrinterStatus.Printing;
    if (['maintenance'].includes(normalized)) return PrinterStatus.Maintenance;
    if (['offline', 'hors ligne'].includes(normalized)) return PrinterStatus.Offline;
    return PrinterStatus.Offline;
  }

  getDurationString(minutes?: number): string {
    if (!minutes) return '-';
    const hours = Math.floor(minutes / 60);
    const mins = minutes % 60;
    return hours > 0 ? `${hours}h${mins.toString().padStart(2, '0')}` : `${mins}min`;
  }

  closeTutorial(): void {
    localStorage.setItem(this.tutorialStorageKey, 'true');
    this.showTutorial = false;
  }
}




