import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { MaterialStockService } from '../../../services/material-stock.service';

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
    private cdr: ChangeDetectorRef
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
      error: (err) => console.error(err)
    })
  }

  onSubmit(): void {
    if (this.materialForm.invalid) return;

    const material = this.materialForm.value;

    if (this.isEditMode && this.materialId) {
      // Pour l'édition, on utilise des endpoints séparés
      // Seuils
      this.materialStockService.updateThresholds(this.materialId, material.minThreshold, material.maxThreshold).subscribe();
      // Prix
      this.materialStockService.updatePrice(this.materialId, material.unitPrice).subscribe();
      // Redirection
      this.router.navigate(['/stock']);
    } else {
      this.materialStockService.create(material).subscribe({
        next: () => this.router.navigate(['/stock']),
        error: (err) => console.error(err)
      });
    }
  }
}


