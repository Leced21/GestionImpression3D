export enum PrintJobStatus {
  Pending = 'Pending',
  Queued = 'Queued',
  Printing = 'Printing',
  Paused = 'Paused',
  Completed = 'Completed',
  Failed = 'Failed',
  Cancelled = 'Cancelled'
}

export enum PrintJobPriority {
  Low = 'Low',
  Normal = 'Normal',
  High = 'High',
  Urgent = 'Urgent'
}

export interface PrintJob {
  id: number;
  jobNumber: string;
  pieceId: number;
  pieceName: string;
  pieceReference: string;
  printerId?: number;
  printerName?: string;
  operatorId?: number;
  operatorName?: string;
  quantity: number;
  quantityCompleted: number;
  status: string;
  priority: string;
  createdAt: Date;
  startedAt?: Date;
  completedAt?: Date;
  estimatedDurationMinutes?: number;
  actualDurationMinutes?: number;
  estimatedMaterialGrams: number;
  actualMaterialGrams: number;
  failureReason?: string;
  notes?: string;
  progressPercent: number;
  statusLabel: string;
  statusColor: string;
  priorityLabel: string;
}

export interface PrintJobStatistics {
  totalJobs: number;
  pendingJobs: number;
  queuedJobs: number;
  printingJobs: number;
  completedJobs: number;
  failedJobs: number;
  totalDurationMinutes: number;
  totalMaterialGrams: number;
  successRate: number;
}

export interface CreatePrintJobRequest {
  pieceId: number;
  quantity: number;
  priority: string;
  estimatedDurationMinutes: number;
  estimatedMaterialGrams: number;
  notes?: string;
}