import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import {FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import {ActivatedRoute, Router, RouterModule } from '@angular/router';
import { PrinterService } from '../../../services/printer.service';
import { PrinterMaintenanceService } from '../../../services/printer-maintenance.service';
import { Printer } from '../../../models/printer.model';

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

  constructor(
    private fb: FormBuilder,
    private maintenanceService: PrinterMaintenanceService,
    private printerService: PrinterService,
    private route: ActivatedRoute,
    private router: Router,
    private cdr: ChangeDetectorRef
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
    if (this.maintenanceForm.invalid) return;
    const maintenanceData = this.maintenanceForm.value;
    if (this.isEditMode && this.maintenanceId) {
      this.maintenanceService.update(this.maintenanceId, maintenanceData).subscribe({
        next: () => this.router.navigate(['/maintenances']),
        error: (err) => console.error(err)
      });
    } else {
      this.maintenanceService.create(maintenanceData).subscribe({
        next: () => this.router.navigate(['/maintenances']),
        error: (err) => console.error(err)
      });
    }
  }
}
