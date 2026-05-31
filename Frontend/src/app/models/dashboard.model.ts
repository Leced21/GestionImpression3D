export interface GlobalStats {
  totalPieces: number;
  totalProjets: number;
  totalPrinters: number;
  activePrinters: number;
  printJobs: {
    totalJobs: number;
    pendingJobs: number;
    printingJobs: number;
    completedJobs: number;
    failedJobs: number;
    successRate: number;
  };
  materialStock: {
    totalMaterials: number;
    lowStockMaterials: number;
    totalValue: number;
  };
  lastUpdated: Date;
}

export interface ProductionTrend {
  date: Date;
  completed: number;
  failed: number;
}

export interface MaterialConsumption {
  name: string;
  type: string;
  quantity: number;
  unit: string;
}

export interface PrinterActivity {
  nom: string;
  status: string;
  totalPrintJobs: number;
  totalPrintHours: number;
  lastPrint: Date;
}