export enum PrinterStatus {
  Available = 'Available',
  Printing = 'Printing',
  Maintenance = 'Maintenance',
  Offline = 'Offline',
  Error = 'Error'
}

export enum PrinterType {
  FDM = 'FDM',
  SLA = 'SLA',
  SLS = 'SLS'
}

export interface Printer {
  id: number;
  nom: string;
  reference: string;
  model: string;
  brand: string;
  type: PrinterType;
  status: PrinterStatus;
  statusLabel: string;
  statusColor: string;
  ipAddress: string;
  maxPrintSizeX: number;
  maxPrintSizeY: number;
  maxPrintSizeZ: number;
  totalPrintHours: number;
  totalPrintJobs: number;
  lastMaintenance?: Date;
  lastPrint?: Date;
  isActive: boolean;
}

export interface PrinterStatistics {
  totalPrinters: number;
  availablePrinters: number;
  printingPrinters: number;
  maintenancePrinters: number;
  errorPrinters: number;
  totalPrintJobs: number;
  totalPrintHours: number;
}
