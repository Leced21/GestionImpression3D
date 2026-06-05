import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { Printer } from '../../../models/printer.model';
import { PrintJob } from '../../../models/print-job.model';
import { PieceService } from '../../../services/piece.service';
import { PrinterService } from '../../../services/printer.service';
import { PrintJobService } from '../../../services/print-job.service';

@Component({
  selector: 'app-print-job-detail',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './print-job-detail.html',
  styleUrls: ['./print-job-detail.css'],
})
export class PrintJobDetail implements OnInit {

  job: PrintJob | null = null;
  availablePrinters: Printer[] = [];
  selectedPrinterId: number | null = null;

  showCompleteModal: boolean = false;
  showFailModal: boolean = false;
  completeData = { duration: 0, material: 0 };
  failReason: string = '';

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private printJobService: PrintJobService,
    private printerService: PrinterService,
    private pieceService: PieceService,
    private cdr: ChangeDetectorRef
  ) { }

  ngOnInit(): void {
    const id = this.route.snapshot.params['id'];
    if (id) {
      this.loadJob(id);
      this.loadPrinters();
    }
  }

  loadJob(id: number): void {
    this.printJobService.getById(id).subscribe({
      next: (job) => {
        this.job = job;
        this.cdr.detectChanges();
      },
      error: (error) => {
        console.error('Error fetching print job:', error);
      }
    });
  }

  loadPrinters(): void {
    this.printerService.getAll().subscribe({
      next: (printers) => {
        this.availablePrinters = printers;
        this.cdr.detectChanges();
      },
      error: (error) => {
        console.error('Error fetching printers:', error);
      }
    });
  }
  get canAssign(): boolean {
    return this.job?.status === 'Pending' || this.job?.status === 'Queued';
  }

  get canStart(): boolean {
    return this.job?.status === 'Queued' && !!this.job.printerId;
  }

  get canPause(): boolean {
    return this.job?.status === 'Printing';
  }

  get canResume(): boolean {
    return this.job?.status === 'Paused';
  }

  get canComplete(): boolean {
    return this.job?.status === 'Printing';
  }

  get canFail(): boolean {
    return this.job?.status === 'Printing' || this.job?.status === 'Queued';
  }

  get canCancel(): boolean {
    return this.job?.status !== 'Completed' && this.job?.status !== 'Failed' && this.job?.status !== 'Cancelled';
  }

  get canDelete(): boolean {
    return this.job?.status === 'Pending' || this.job?.status === 'Cancelled' || this.job?.status === 'Failed';
  }

  getDurationString(minutes?: number): string {
    if (!minutes) return '-';
    const hours = Math.floor(minutes / 60);
    const mins = minutes % 60;
    return hours > 0 ? `${hours}h${mins.toString().padStart(2, '0')}` : `${mins}min`;
  }

  assignPrinter(): void {
    if (!this.job || !this.selectedPrinterId) return;

    this.printJobService.assignPrinter(this.job.id, this.selectedPrinterId).subscribe({
      next: (updated) => {
        this.job = updated;
        this.selectedPrinterId = null;
      },
      error: (err) => console.error(err)
    });
  }

  startJob(): void {
    if (!this.job) return;
    this.printJobService.start(this.job.id).subscribe({
      next: (updated) => this.job = updated,
      error: (err) => console.error(err)
    });
  }

  pauseJob(): void {
    if (!this.job) return;
    this.printJobService.pause(this.job.id).subscribe({
      next: (updated) => this.job = updated,
      error: (err) => console.error(err)
    });
  }

  resumeJob(): void {
    if (!this.job) return;
    this.printJobService.resume(this.job.id).subscribe({
      next: (updated) => this.job = updated,
      error: (err) => console.error(err)
    });
  }

  completeJob(): void {
    if (!this.job) return;
    this.completeData = {
      duration: this.job.estimatedDurationMinutes || 0,
      material: this.job.estimatedMaterialGrams
    };
    this.showCompleteModal = true;
  }

  closeCompleteModal(): void {
    this.showCompleteModal = false;
  }

  confirmComplete(): void {
    if (!this.job) return;

    this.printJobService.complete(
      this.job.id,
      this.completeData.duration,
      this.completeData.material
    ).subscribe({
      next: (updated) => {
        this.job = updated;
        this.closeCompleteModal();
      },
      error: (err) => console.error(err)
    });
  }

  failJob(): void {
    this.failReason = '';
    this.showFailModal = true;
  }

  closeFailModal(): void {
    this.showFailModal = false;
  }

  confirmFail(): void {
    if (!this.job || !this.failReason) return;

    this.printJobService.fail(this.job.id, this.failReason).subscribe({
      next: (updated) => {
        this.job = updated;
        this.closeFailModal();
      },
      error: (err) => console.error(err)
    });
  }

  cancelJob(): void {
    if (!this.job) return;
    if (confirm('Annuler ce job d\'impression ?')) {
      this.printJobService.cancel(this.job.id).subscribe({
        next: (updated) => this.job = updated,
        error: (err) => console.error(err)
      });
    }
  }

  deleteJob(): void {
    if (!this.job) return;
    if (confirm('Supprimer ce job d\'impression ?')) {
      this.printJobService.delete(this.job.id).subscribe({
        next: () => this.router.navigate(['/print-jobs']),
        error: (err) => console.error(err)
      });
    }
  }

  closeModalOnOverlay(event: MouseEvent): void {
    if ((event.target as HTMLElement).classList.contains('modal')) {
      this.closeCompleteModal();
      this.closeFailModal();
    }
  }
}


