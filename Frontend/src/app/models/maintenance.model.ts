export interface PrinterMaintenance {
  id: number;
  printerId: number;
  printerName?: string;
  type: MaintenanceType;
  title: string;
  description: string;
  scheduledDate: Date;
  completedDate?: Date;
  status: MaintenanceStatus;
  durationMinutes: number;
  cost: number;
  performedBy?: string;
  notes?: string;
  createdAt: Date;
}

export interface MaintenanceStatistics {
  totalMaintenances: number;
  preventiveCount: number;
  correctiveCount: number;
  completedCount: number;
  pendingCount: number;
  totalCost: number;
  averageDurationMinutes: number;
  lastMaintenance?: Date;
  nextScheduled?: Date;
}

export enum MaintenanceStatus {
  Scheduled = 'Scheduled',
  InProgress = 'InProgress',
  Completed = 'Completed',
  Cancelled = 'Cancelled'
}

export enum MaintenanceType {
    Preventive = 'Preventive',
    Corrective = 'Corrective',
    Calibration = 'Calibration',
    Cleaning = 'Cleaning'
}
