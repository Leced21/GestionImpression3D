import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { PrintIncidentService } from '../../../services/print-incident.service';
import { PrinterService } from '../../../services/printer.service';
import { PrintJobService } from '../../../services/print-job.service';
import { Printer } from '../../../models/printer.model';
import { PrintJob } from '../../../models/print-job.model';

@Component({
  selector: 'app-incident-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './incident-form.html',
  styleUrl: './incident-form.css',
})
export class IncidentForm implements OnInit {
  incidentForm!: FormGroup;
  printers: Printer[] = [];
  printJobs: PrintJob[] = [];
  isEditMode: boolean = false;
  incidentId?: number;


  constructor(
    private fb: FormBuilder,
    private incidentService: PrintIncidentService,
    private printerService: PrinterService,
    private printJobService: PrintJobService,
    private router: Router,
    private cdr: ChangeDetectorRef
  ) { }

  ngOnInit(): void {
    this.initForm();
    this.loadPrinters();
    this.loadPrintJobs();
  }

  initForm(): void {
    this.incidentForm = this.fb.group({
      title: ['', Validators.required],
      description: ['', Validators.required],
      severity: ['Moyenne', Validators.required],
      printerId: [''],
      printJobId: ['']
    });
  }
  loadPrinters(): void {
    this.printerService.getAll().subscribe({
      next: (data) => {
        this.printers = data;
        this.cdr.detectChanges();
      },
      error: (error) => {
        console.error('Erreur lors du chargement des imprimantes', error);
      },
    });
  }

  loadPrintJobs(): void {
    this.printJobService.getAll().subscribe({
      next: (data) => {
        this.printJobs = data;
        this.cdr.detectChanges();
      },
      error: (error) => {
        console.error('Erreur lors du chargement des travaux d\'impression', error);
      },
    });
  }

  onSubmit(): void {
    if (this.incidentForm.invalid) {
      this.incidentForm.markAllAsTouched();
      return;
    }

    const formValue = this.incidentForm.value;
    const incidentData = {
      ...formValue,
      printerId: formValue.printerId ? Number(formValue.printerId) : null,
      printJobId: formValue.printJobId ? Number(formValue.printJobId) : null
    };

    if (this.isEditMode && this.incidentId) {
      this.incidentService.update(this.incidentId, incidentData).subscribe({
        next: () => {
          this.router.navigate(['/incidents']);
        },
        error: (error) => {
          console.error('Erreur lors de la mise à jour de l\'incident', error);
        },
      });
    } else {
      this.incidentService.create(incidentData).subscribe({
        next: () => {
          this.router.navigate(['/incidents']);
        },
        error: (error) => {
          console.error('Erreur lors de la création de l\'incident', error);
        },
      });
    }
  }
}
