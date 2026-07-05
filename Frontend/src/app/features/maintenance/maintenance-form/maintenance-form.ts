import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import {FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import {ActivatedRoute, Router, RouterModule } from '@angular/router';
import { PrinterService } from '../../../services/printer.service';
import { PrinterMaintenanceService } from '../../../services/printer-maintenance.service';
import { Printer } from '../../../models/printer.model';
import { MaintenanceType } from '../../../models/maintenance.model';
import { ToastService } from '../../../services/toast.service';

@Component({
  selector: 'app-maintenance-form',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule,ReactiveFormsModule],
  templateUrl: './maintenance-form.html',
  styleUrl: './maintenance-form.css',
})
export class MaintenanceForm implements OnInit{
  maintenanceForm!: FormGroup;
  isEditMode = false;
  maintenanceId?: number;
  printers: Printer[] = [];
  isSubmitting = false;

  constructor(
    private fb: FormBuilder,
    private maintenanceService: PrinterMaintenanceService,
    private printerService: PrinterService,
    private route: ActivatedRoute,
    private router: Router,
    private cdr: ChangeDetectorRef,
    private toast: ToastService
  ) { }
  ngOnInit(): void {
    this.initForm();
    this.loadPrinters();
    this.maintenanceId = Number(this.route.snapshot.params['id']);
    if (this.maintenanceId) {
      this.isEditMode = true;
      this.loadMaintenance();
    }
  }
  initForm(): void {
    this.maintenanceForm = this.fb.group({
      printerId: ['', Validators.required],
      type: ['Preventive', Validators.required],
      title: ['', Validators.required],
      description: [''],
      scheduledDate: ['', Validators.required],
      durationMinutes: [60, [Validators.required, Validators.min(1)]],
      cost: [0, [Validators.required, Validators.min(0)]],
      notes: ['']
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
  loadMaintenance(): void {
    if (!this.maintenanceId) return;
    this.maintenanceService.getById(this.maintenanceId).subscribe({
      next: (data) => {
        this.maintenanceForm.patchValue({
          printerId: data.printerId,
          type: data.type,
          title: data.title,
          description: data.description,
          scheduledDate: data.scheduledDate.toString().slice(0, 16),
          durationMinutes: data.durationMinutes,
          cost: data.cost,
          notes: data.notes
        });
        this.cdr.detectChanges();
      },
      error: (error) => {
        console.error('Erreur lors du chargement de la maintenance', error);
      },
    });
  }
  onSubmit(): void {
    if (this.maintenanceForm.invalid) {
      this.maintenanceForm.markAllAsTouched();
      this.toast.warn('Complète les champs obligatoires avant d’enregistrer.');
      return;
    }

    const formValue = this.maintenanceForm.value;
    const maintenanceData = {
      ...formValue,
      printerId: Number(formValue.printerId),
      type: this.toMaintenanceTypeValue(formValue.type),
      title: String(formValue.title ?? '').trim(),
      description: String(formValue.description ?? '').trim(),
      scheduledDate: new Date(formValue.scheduledDate).toISOString(),
      durationMinutes: Number(formValue.durationMinutes),
      cost: Number(formValue.cost),
      notes: formValue.notes ? String(formValue.notes).trim() : null
    };

    this.isSubmitting = true;
    if (this.isEditMode && this.maintenanceId) {
      this.maintenanceService.update(this.maintenanceId, maintenanceData).subscribe({
        next: () => {
          this.toast.success('Maintenance mise à jour');
          this.router.navigate(['/maintenances']);
        },
        error: (err) => {
          this.isSubmitting = false;
          this.toast.error(this.extractErrorMessage(err));
        }
      });
    } else {
      this.maintenanceService.create(maintenanceData).subscribe({
        next: () => {
          this.toast.success('Maintenance enregistrée');
          this.router.navigate(['/maintenances']);
        },
        error: (err) => {
          this.isSubmitting = false;
          this.toast.error(this.extractErrorMessage(err));
        }
      });
    }
  }

  private toMaintenanceTypeValue(type: MaintenanceType | string): number {
    const values = Object.values(MaintenanceType);
    const index = values.indexOf(type as MaintenanceType);
    return index >= 0 ? index + 1 : 1;
  }

  private extractErrorMessage(err: any): string {
    if (err?.error?.error) return err.error.error;
    if (err?.error?.title) return err.error.title;
    if (err?.status === 403) return 'Tu n’as pas les droits pour enregistrer une maintenance.';
    return 'Impossible d’enregistrer la maintenance.';
  }
}
