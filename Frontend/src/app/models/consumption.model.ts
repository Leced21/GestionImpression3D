import { MaterialUnit } from "./material-stock.model";

export interface MaterialConsumption {
  id: number;
  materialStockId: number;
  materialName?: string;
  printJobId?: number;
  printJobNumber?: string;
  ordreFabricationId?: number;
  ordreReference?: string;
  quantity: number;
  unit: MaterialUnit;
  type: MaterialConsumptionType;
  reason?: string;
  consumedAt: Date;
  consumedBy?: number;
  consumedByName?: string;
  notes?: string;
}

export interface ConsumptionStatistics {
  totalConsumption: number;
  productionConsumption: number;
  wasteConsumption: number;
  testConsumption: number;
  maintenanceConsumption: number;
  consumptionByMaterial: Record<string, number>;
  consumptionByMonth: Record<string, number>;
}
export enum MaterialConsumptionType {
    Production = 'Production',
    Test = 'Test',
    Maintenance = 'Maintenance',
    Waste = 'Waste',
}
