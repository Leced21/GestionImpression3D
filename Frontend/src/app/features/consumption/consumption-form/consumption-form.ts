import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { MaterialStock } from '../../../models/material-stock.model';
import { MaterialConsumptionService } from '../../../services/material-consumption.service';
import { MaterialStockService } from '../../../services/material-stock.service';

@Component({
  selector: 'app-consumption-form',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule, ReactiveFormsModule],
  templateUrl: './consumption-form.html',
  styleUrl: './consumption-form.css',
})
export class ConsumptionForm implements OnInit {
  consumptionForm!: FormGroup;
  materials: MaterialStock[] = [];
  selectedUnit = 'g';

  constructor(
    private fb: FormBuilder,
    private consumptionService: MaterialConsumptionService,
    private materialService: MaterialStockService,
    private router: Router,
    private cdr: ChangeDetectorRef
  ) { }
  ngOnInit(): void {
    this.initForm();
    this.loadMaterials();
  }
  initForm(): void {
    this.consumptionForm = this.fb.group({
      materialStockId: ['', Validators.required],
      quantity: ['', [Validators.required, Validators.min(0.1)]],
      type: ['Production', Validators.required],
      reason: [''],
      notes: ['']
    });
  }
  loadMaterials(): void {
    this.materialService.getAll().subscribe({
      next: (data) => {
        this.materials = data.filter(m =>m.isActive);
        this.cdr.detectChanges();
      },
      error: (error) => {
        console.error('Erreur lors du chargement des matériaux', error);
      },
    });
  }
  onMaterialChange(): void {
    const materialId = this.consumptionForm.get('materialStockId')?.value;
    const material = this.materials.find(m => m.id === parseInt(materialId));
    if (material) {
      this.selectedUnit = material.unitLabel;
    }
  }

  onSubmit(): void {
    if (this.consumptionForm.invalid) return;
    
    this.consumptionService.create(this.consumptionForm.value).subscribe({
      next: () => this.router.navigate(['/consommations']),
      error: (err) => console.error(err)
    });
  }

}
