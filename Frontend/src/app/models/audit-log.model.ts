// models/audit-log.model.ts
export enum ActionType {
  Create = 1,
  Update = 2,
  Delete = 3,
  StatusChange = 4
}

export enum EntityType {
  Piece = 1,
  Projet = 2,
  PrintJob = 3,
  User = 4,
  Printer = 5,
  MaterialStock = 6,
  OrdreFabrication = 7,
  PrintProfile = 8,
  PrinterMaintenance = 9,
  MaterialConsumption = 10,
  PrintIncident = 11,
  Client = 12,
  Devis = 13,
  Invitation = 14,
  Facture = 15,
  Commande = 16
}

export interface AuditLog {
  id: number;
  userId?: number;
  userEmail?: string;
  action: ActionType;
  entityType: EntityType;
  entityId: number;
  entityName?: string;
  fieldName?: string;
  oldValue?: string;
  newValue?: string;
  ipAddress?: string;
  timestamp: Date;
}

export interface AuditLogFilter {
  userId?: number;
  userEmail?: string;
  entityType?: EntityType;
  entityId?: number;
  startDate?: Date;
  endDate?: Date;
  action?: ActionType;
}
