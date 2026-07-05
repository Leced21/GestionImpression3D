import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { MaterialStockService } from '../../../services/material-stock.service';
import { ToastService } from '../../../services/toast.service';
import { forkJoin } from 'rxjs';

@Component({
  selector: 'app-materialstock-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './materialstock-form.html',
  styleUrls: ['./materialstock-form.css'],
})
export class MaterialstockForm implements OnInit {
  materialForm!: FormGroup;
  isEditMode = false;
  materialId?: number;

  constructor(
    private fb: FormBuilder,
    private materialStockService: MaterialStockService,
    private router: Router,
    private route: ActivatedRoute,
    private cdr: ChangeDetectorRef,
    private toast: ToastService
  ) { }

  ngOnInit(): void {
    this.initForm();

    this.materialId = this.route.snapshot.params['id'];
    if (this.materialId) {
      this.isEditMode = true;
      this.loadMaterial();
    }
  }

  initForm(): void {
    this.materialForm = this.fb.group({
      name: ['', Validators.required],
      reference: [''],
      type: ['PLA', Validators.required],
      brand: ['', Validators.required],
      color: ['', Validators.required],
      quantity: [0, [Validators.required, Validators.min(0)]],
      unit: ['Grams', Validators.required],
      minThreshold: [0, [Validators.required, Validators.min(0)]],
      maxThreshold: [0, [Validators.min(0)]],
      location: [''],
      supplier: [''],
      unitPrice: [0, [Validators.required, Validators.min(0)]],
      notes: ['']
    });
  }

  loadMaterial(): void {
    this.materialStockService.getById(this.materialId!).subscribe({
      next: (material) => {
        this.materialForm.patchValue(material);
        this.cdr.detectChanges();
      },
      error: () => this.toast.error('Impossible de charger le stock matière')
    })
  }

  onSubmit(): void {
    if (this.materialForm.invalid) return;

    const material = this.materialForm.value;

    if (this.isEditMode && this.materialId) {
      forkJoin([
        this.materialStockService.updateThresholds(this.materialId, material.minThreshold, material.maxThreshold),
        this.materialStockService.updatePrice(this.materialId, material.unitPrice)
      ]).subscribe({
        next: () => {
          this.toast.success('Stock matière mis à jour');
          this.router.navigate(['/stock']);
        }
      });
    } else {
      this.materialStockService.create(material).subscribe({
        next: () => {
          this.toast.success('Stock matière créé');
          this.router.navigate(['/stock']);
        }
      });
    }
  }
}


