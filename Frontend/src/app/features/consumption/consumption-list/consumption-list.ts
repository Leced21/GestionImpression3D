import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { ConsumptionStatistics, MaterialConsumption } from '../../../models/consumption.model';
import { MaterialConsumptionService } from '../../../services/material-consumption.service';

@Component({
  selector: 'app-consumption-list',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './consumption-list.html',
  styleUrl: './consumption-list.css',
})
export class ConsumptionList implements OnInit {
  consumptions: MaterialConsumption[] = [];
  statistics?: ConsumptionStatistics;
  startDate = '';
  endDate = '';

  constructor(
    private consumptionService: MaterialConsumptionService,
    private cdr: ChangeDetectorRef
  ) { }

  ngOnInit(): void {
    this.loadConsumptions();
    this.loadStatistics();
  }
  loadConsumptions(): void {
    this.consumptionService.getAll().subscribe({
      next: (data) => {
        this.consumptions = data;
        this.cdr.detectChanges();
      },
      error: (error) => {
        console.error('Erreur lors du chargement des consommations', error);
      },
    });
  }
  loadStatistics(): void {
    const start = this.startDate ? new Date(this.startDate) : undefined;
    const end = this.endDate ? new Date(this.endDate) : undefined;
    this.consumptionService.getStatistics(start, end).subscribe({
      next: (statistics) => {
        this.statistics = statistics;
        this.cdr.detectChanges();
      },
      error: (error) => {
        console.error('Erreur lors du chargement des statistiques', error);
      },
    });
  }
  get consumptionByMaterial(): Record<string, number> {
    return this.statistics?.consumptionByMaterial || {};
  }
  getBarPercentage(value: number): number {
    const max = Math.max(...Object.values(this.consumptionByMaterial), 0.1);
    return (value / max) * 100;
  }

  getTypeClass(type: string): string {
    return type;
  }

  refresh(): void {
    this.loadConsumptions();
    this.loadStatistics();
  }
}
