import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { MaterialStockService } from '../../../services/material-stock.service';
import { MaterialStock } from '../../../models/material-stock.model';

@Component({
  selector: 'app-materialstock-detail',
  standalone: true,
  imports: [CommonModule,RouterModule,FormsModule],
  templateUrl: './materialstock-detail.html',
  styleUrl: './materialstock-detail.css',
})
export class MaterialstockDetail implements OnInit {
  material: MaterialStock | null = null;
  
  showAddModal = false;
  showRemoveModal = false;
  showThresholdModal = false;
  showPriceModal = false;
  
  stockQuantity = 0;
  stockNote = '';
  minThreshold = 0;
  maxThreshold = 0;
  unitPrice = 0;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private materialStockService: MaterialStockService
  ) {}

  ngOnInit(): void {
    const id = this.route.snapshot.params['id'];
    if (id) {
      this.loadMaterial(id);
    }
  }

  loadMaterial(id: number): void {
    this.materialStockService.getById(id).subscribe({
      next: (data) => {
        this.material = data;
        this.minThreshold = data.minThreshold;
        this.maxThreshold = data.maxThreshold;
        this.unitPrice = data.unitPrice;
      },
      error: (err) => console.error(err)
    });
  }

  getMaterialIcon(type: string): string {
    const icons: Record<string, string> = {
      'PLA': '🧵',
      'PETG': '🧶',
      'ABS': '🔧',
      'TPU': '🪢',
      'Resin': '💧',
      'Nylon': '🧵'
    };
    return icons[type] || '📦';
  }

  getColorValue(color: string): string {
    const colors: Record<string, string> = {
      'Blanc': '#ffffff',
      'Noir': '#000000',
      'Rouge': '#ef4444',
      'Bleu': '#3b82f6',
      'Vert': '#10b981',
      'Jaune': '#f59e0b',
      'Rose': '#ec4899',
      'Gris': '#6b7280'
    };
    return colors[color] || '#cbd5e1';
  }

  getStockStatus(): string {
    if (!this.material) return '';
    if (this.material.isCriticalStock) return 'Stock critique';
    if (this.material.isLowStock) return 'Stock bas';
    return 'Stock OK';
  }

  getStockPercentage(): number {
    if (!this.material || this.material.maxThreshold <= 0) return 0;
    return Math.min(100, (this.material.quantity / this.material.maxThreshold) * 100);
  }

  confirmAddStock(): void {
    if (!this.material || this.stockQuantity <= 0) return;
    
    this.materialStockService.addStock(this.material.id, {
      quantity: this.stockQuantity,
      note: this.stockNote
    }).subscribe({
      next: () => {
        this.loadMaterial(this.material!.id);
        this.closeAddModal();
      },
      error: (err) => console.error(err)
    });
  }

  confirmRemoveStock(): void {
    if (!this.material || this.stockQuantity <= 0) return;
    
    this.materialStockService.removeStock(this.material.id, {
      quantity: this.stockQuantity,
      note: this.stockNote
    }).subscribe({
      next: () => {
        this.loadMaterial(this.material!.id);
        this.closeRemoveModal();
      },
      error: (err) => console.error(err)
    });
  }

  confirmUpdateThresholds(): void {
    if (!this.material) return;
    
    this.materialStockService.updateThresholds(this.material.id, this.minThreshold, this.maxThreshold).subscribe({
      next: () => {
        this.loadMaterial(this.material!.id);
        this.closeThresholdModal();
      },
      error: (err) => console.error(err)
    });
  }

  confirmUpdatePrice(): void {
    if (!this.material) return;
    
    this.materialStockService.updatePrice(this.material.id, this.unitPrice).subscribe({
      next: () => {
        this.loadMaterial(this.material!.id);
        this.closePriceModal();
      },
      error: (err) => console.error(err)
    });
  }

  deleteMaterial(): void {
    if (!this.material) return;
    if (confirm(`Supprimer le matériau "${this.material.name}" ?`)) {
      this.materialStockService.delete(this.material.id).subscribe({
        next: () => this.router.navigate(['/stock']),
        error: (err) => console.error(err)
      });
    }
  }

  closeAddModal(): void {
    this.showAddModal = false;
    this.stockQuantity = 0;
    this.stockNote = '';
  }

  closeRemoveModal(): void {
    this.showRemoveModal = false;
    this.stockQuantity = 0;
    this.stockNote = '';
  }

  closeThresholdModal(): void {
    this.showThresholdModal = false;
  }

  closePriceModal(): void {
    this.showPriceModal = false;
  }

  closeModalOnOverlay(event: MouseEvent): void {
    if ((event.target as HTMLElement).classList.contains('modal')) {
      this.closeAddModal();
      this.closeRemoveModal();
      this.closeThresholdModal();
      this.closePriceModal();
    }
  }
}
