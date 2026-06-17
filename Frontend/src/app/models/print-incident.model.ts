export interface PrintIncident {
  id: number;
  printJobId?: number;
  printJobNumber?: string;
  printerId?: number;
  printerName?: string;
  title: string;
  description: string;
  severity: IncidentSeverity;
  status: IncidentStatus;
  occurredAt: Date;
  resolvedAt?: Date;
  resolution?: string;
  reportedBy?: number;
  reportedByName?: string;
  resolvedBy?: number;
  resolvedByName?: string;
}

export interface IncidentStatistics {
  totalIncidents: number;
  openIncidents: number;
  inProgressIncidents: number;
  resolvedIncidents: number;
  closedIncidents: number;
  bySeverity: Record<string, number>;
  byPrinter: Record<string, number>;
  averageResolutionTimeHours: number;
}

export enum IncidentStatus {
  Ouvert = 'Ouvert',
  EnCours = 'En cours',
  Résolu = 'Résolu',
  Fermé = 'Fermé'
}

export enum IncidentSeverity {
  Critique = 'Critique',
  Haute = 'Haute',
  Moyenne = 'Moyenne',
  Basse = 'Basse'
}