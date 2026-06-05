import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { Printer, PrinterStatistics } from '../../../models/printer.model';
import { PrinterService } from '../../../services/printer.service';

@Component({
  selector: 'app-printer-list',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './printer-list.html',
  styleUrls: ['./printer-list.css'],
})
export class PrinterList implements OnInit {
  printers: Printer[] = [];
  statistics?: PrinterStatistics;
  showStatusModal: boolean = false;
  selectedPrinter: Printer | null = null;
  selectedStatus: string = '';

  statusOptions = [
    { value: 'Available', label: 'Disponible', color: '#10b981' },
    { value: 'Printing', label: 'En impression', color: '#3b82f6' },
    { value: 'Maintenance', label: 'Maintenance', color: '#f59e0b' },
    { value: 'Offline', label: 'Hors ligne', color: '#64748b' },
    { value: 'Error', label: 'Erreur', color: '#ef4444' }
  ];

  constructor(
    private printerService: PrinterService,
    private cdr: ChangeDetectorRef
  ) { }

  ngOnInit(): void {
    this.loadPrinters();
    this.loadStatistics();
  }

  loadPrinters(): void {
    this.printerService.getAll().subscribe({
      next: (printers) => {
        this.printers = printers;
        this.cdr.detectChanges();
      },
      error: (error) => {
        console.error('Error fetching printers:', error);
      }
    });
  }
  loadStatistics(): void {
    this.printerService.getStatistics().subscribe({
      next: (stats) => {
        this.statistics = stats;
        this.cdr.detectChanges();
      },
      error: (error) => {
        console.error('Error fetching printer statistics:', error);
      }
    });
  }
  getPrinterIcon(type: string): string {
    const icons: Record<string, string> = {
      'FDM': '🏭',
      'SLA': '🔮',
      'SLS': '⚙️'
    };
    return icons[type] || '🖨️';
  }

  changeStatus(printer: Printer): void {
    this.selectedPrinter = printer;
    this.selectedStatus = printer.status;
    this.showStatusModal = true;
  }
  closeStatusModal(): void {
    this.showStatusModal = false;
    this.selectedPrinter = null;
    this.selectedStatus = '';
  }

  closeModalOnOverlay(event: MouseEvent): void {
    if ((event.target as HTMLElement).classList.contains('modal')) {
      this.closeStatusModal();
    }
  }
  confirmStatusChange(): void {
    if (this.selectedPrinter && this.selectedStatus !== this.selectedPrinter.status) {
      this.printerService.updateStatus(this.selectedPrinter.id, this.selectedStatus).subscribe({
        next: () => {
          this.closeStatusModal();
          this.loadPrinters();
          this.loadStatistics();
        },
        error: (error) => {
          console.error('Error updating printer status:', error);
          this.closeStatusModal();
        }
      });
    } else {
      this.closeStatusModal();
    }
  }

  deletePrinter(id: number): void {
    if (confirm('Êtes-vous sûr de vouloir supprimer ce imprimantePrinter))?')) {
      this.printerService.delete(id).subscribe({
        next: () => {
          this.loadPrinters();
          this.loadStatistics();
        },
        error: (error) => {
          console.error('Error deleting printer:', error);
        }
      });
    }
  }
}


