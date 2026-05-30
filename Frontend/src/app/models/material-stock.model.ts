export enum MaterialType {
  PLA = 'PLA',
  PETG = 'PETG',
  ABS = 'ABS',
  TPU = 'TPU',
  Nylon = 'Nylon',
  Resin = 'Resin',
  Other = 'Other'
}

export enum MaterialUnit {
  Grams = 'Grams',
  Kilograms = 'Kilograms',
  Meters = 'Meters',
  Liters = 'Liters'
}

export interface MaterialStock {
  id: number;
  name: string;
  type: string;
  typeLabel: string;
  brand: string;
  color: string;
  reference?: string;
  quantity: number;
  unit: string;
  unitLabel: string;
  minThreshold: number;
  maxThreshold: number;
  location?: string;
  supplier?: string;
  unitPrice: number;
  totalValue: number;
  lastRestockedAt?: Date;
  lastUsedAt?: Date;
  isLowStock: boolean;
  isCriticalStock: boolean;
  isActive: boolean;
  notes?: string;
  usageCount?: number;
}

export interface MaterialStatistics {
  totalMaterials: number;
  lowStockMaterials: number;
  criticalStockMaterials: number;
  outOfStockMaterials: number;
  totalValue: number;
  valueByType: Record<string, number>;
  countByType: Record<string, number>;
}

export interface CreateMaterialStockRequest {
  name: string;
  type: string;
  brand: string;
  color: string;
  reference?: string;
  quantity: number;
  unit: string;
  minThreshold: number;
  maxThreshold: number;
  location?: string;
  supplier?: string;
  unitPrice: number;
  notes?: string;
}

export interface UpdateStockRequest {
  quantity: number;
  note?: string;
}