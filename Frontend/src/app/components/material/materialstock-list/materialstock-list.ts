import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { RouterModule } from '@angular/router';
import { MaterialStatistics, MaterialStock } from '../../../models/material-stock.model';
import { MaterialStockService } from '../../../services/material-stock.service';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-materialstock-list',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './materialstock-list.html',
  styleUrls: ['./materialstock-list.css'],
})
export class MaterialstockList implements OnInit {
  materials: MaterialStock[] = [];
  filteredMaterials: MaterialStock[] = [];
  statistics?: MaterialStatistics;
  lowStockAlerts: MaterialStock[] = [];
  showLowStock = false;

  searchTerm = '';
  filterType = '';
  filterStatus = '';

  showAddModal = false;
  showRemoveModal = false;
  selectedMaterial: MaterialStock | null = null;
  stockQuantity = 0;
  stockNote = '';

  constructor(
    private materialStockService: MaterialStockService,
    private cdr: ChangeDetectorRef
  ) { }

  ngOnInit(): void {
    this.loadMaterials();
    this.loadStatistics();
    this.loadLowStockAlerts();
  }

  loadMaterials(): void {
    this.materialStockService.getAll().subscribe({
      next: (data) => {
        this.materials = data;
        this.filterMaterials();
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error(err);
      }
    });
  }

  loadStatistics(): void {
    this.materialStockService.getStatistics().subscribe({
      next: (data) => {
        this.statistics = data;
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error(err);
      }
    });
  }

  loadLowStockAlerts(): void {
    this.materialStockService.getLowStockAlerts().subscribe({
      next: (data) => {
        this.lowStockAlerts = data;
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error(err);
      }
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
  getStockStatus(material: MaterialStock): string {
    if (material.isCriticalStock) {
      return 'stock-critical';
    } else if (material.isLowStock) {
      return 'stock-low';
    } else {
      return 'stock-ok';
    }
  }
  getStockPercentage(material: MaterialStock): number {
    if (material.maxThreshold <= 0) return 0;
    return Math.min(100, (material.quantity / material.maxThreshold) * 100);
  }

  filterMaterials(): void {
    this.filteredMaterials = this.materials.filter(m => {
            const matchSearch = !this.searchTerm || 
        m.name.toLowerCase().includes(this.searchTerm.toLowerCase()) ||
        m.brand.toLowerCase().includes(this.searchTerm.toLowerCase());
      const matchType = !this.filterType || m.type === this.filterType;
      const matchStatus = !this.filterStatus || 
        (this.filterStatus === 'low' && m.isLowStock && !m.isCriticalStock) ||
        (this.filterStatus === 'critical' && m.isCriticalStock) ||
        (this.filterStatus === 'normal' && !m.isLowStock);
      return matchSearch && matchType && matchStatus;
    });
  }
  addStock(material: MaterialStock): void {
    this.selectedMaterial = material;
    this.stockQuantity = 0;
    this.stockNote = '';
    this.showAddModal = true;
  }

  removeStock(material: MaterialStock): void {
    this.selectedMaterial = material;
    this.stockQuantity = 0;
    this.stockNote = '';
    this.showRemoveModal = true;
  }

  restockMaterial(material: MaterialStock): void {
    this.addStock(material);
  }

  closeAddModal(): void {
    this.showAddModal = false;
    this.selectedMaterial = null;
  }

  closeRemoveModal(): void {
    this.showRemoveModal = false;
    this.selectedMaterial = null;
  }

  confirmAddStock(): void {
    if (!this.selectedMaterial || this.stockQuantity <= 0) return;
    
    this.materialStockService.addStock(this.selectedMaterial.id, {
      quantity: this.stockQuantity,
      note: this.stockNote
    }).subscribe({
      next: () => {
        this.loadMaterials();
        this.loadStatistics();
        this.loadLowStockAlerts();
        this.closeAddModal();
      },
      error: (err) => console.error(err)
    });
  }

  confirmRemoveStock(): void {
    if (!this.selectedMaterial || this.stockQuantity <= 0) return;
    
    this.materialStockService.removeStock(this.selectedMaterial.id, {
      quantity: this.stockQuantity,
      note: this.stockNote
    }).subscribe({
      next: () => {
        this.loadMaterials();
        this.loadStatistics();
        this.loadLowStockAlerts();
        this.closeRemoveModal();
      },
      error: (err) => console.error(err)
    });
  }

  closeModalOnOverlay(event: MouseEvent): void {
    if ((event.target as HTMLElement).classList.contains('modal')) {
      this.closeAddModal();
      this.closeRemoveModal();
    }
  }
}


