import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { PrinterService } from '../../../services/printer.service';
import { Printer } from '../../../models/printer.model';

@Component({
  selector: 'app-printer-detail',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './printer-detail.html',
  styleUrls: ['./printer-detail.css'],
})
export class PrinterDetail implements OnInit {
  printer : Printer | null = null;

  constructor(
    private route: ActivatedRoute,
    private printerService: PrinterService,
    private router: Router,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    const id = this.route.snapshot.params['id'];
    if (id) {
      this.loadPrinter(id);
    }
  }

  loadPrinter(id: number): void {
    this.printerService.getById(id).subscribe({
      next: (printer) => {
        this.printer = printer;
        this.cdr.detectChanges();
      },
      error: (error) => {
        console.error('Error fetching printer:', error);
        this.router.navigate(['/printers']);
      }
    });
  }
  getTypeLabel(type: string): string {
    const labels: Record<string, string> = {
      'FDM': '🏭 Dépôt de filament',
      'SLA': '🔮 Stéréolithographie (Résine)',
      'SLS': '⚙️ Frittage sélectif (Poudre)'
    };
    return labels[type] || 'Inconnu';
  }
  getVolume(): number {
    if (!this.printer) return 0;
    return this.printer.maxPrintSizeX * this.printer.maxPrintSizeY * this.printer.maxPrintSizeZ / 1000; // en litres
  }
   changeStatus(status: string): void {
    if (!this.printer) return;
    this.printerService.updateStatus(this.printer.id, status).subscribe({
      next: (updated) => {
        this.printer = updated;
        this.cdr.detectChanges();
      },
      error: (error) => console.error('Error updating printer status:', error)
    });
  }

  testConnection(): void {
    if (!this.printer?.ipAddress){
      alert('Aucune adresse IP configurée pour cette imprimante.');
      return;
    }
    alert(`Test de connexion à http://${this.printer.ipAddress}... (fonctionnalité à implémenter)`);
  }

  deletePrinter(): void {
    if (!this.printer) return;
    if (confirm('Êtes-vous sûr de vouloir supprimer cette imprimante ?')) {
      this.printerService.delete(this.printer.id).subscribe({
        next: () => this.router.navigate(['/printers']),
        error: (error) => console.error('Error deleting printer:', error)
      });
    }
  }
}


