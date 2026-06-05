import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { PrinterService } from '../../../services/printer.service';

@Component({
  selector: 'app-printer-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './printer-form.html',
  styleUrls: ['./printer-form.css'],
})
export class PrinterForm implements OnInit {

  printerForm!: FormGroup;
  isEditMode = false;
  printerId?: number;

  constructor(
    private fb: FormBuilder,
    private printerService: PrinterService,
    private router: Router,
    private route: ActivatedRoute
  ) { }
  ngOnInit(): void {
    this.initForm();
    this.printerId = this.route.snapshot.params['id'];
    if (this.printerId) {
      this.isEditMode = true;
      this.loadPrinter();
    }
  }
  initForm(): void {
    this.printerForm = this.fb.group({
      nom: ['', Validators.required],
      reference: [''],
      model: ['', Validators.required],
      brand: ['', Validators.required],
      type: ['FDM', Validators.required],
      ipAddress: [''],
      maxPrintSizeX: [210],
      maxPrintSizeY: [210],
      maxPrintSizeZ: [210]
    });
  }

  loadPrinter(): void {
    this.printerService.getById(this.printerId!).subscribe({
      next: (printer) => {
        this.printerForm.patchValue(printer);
      }
    });
  }

  onSubmit(): void {
    if (this.printerForm.invalid) return;

    const printer = this.printerForm.value;

    if (this.isEditMode && this.printerId) {
      this.printerService.update(this.printerId, printer).subscribe({
        next: () => this.router.navigate(['/printers'])
      });
    } else {
      this.printerService.create(printer).subscribe({
        next: () => this.router.navigate(['/printers'])
      });
    }
  }
}


