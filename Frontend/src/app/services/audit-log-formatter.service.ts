import { Injectable } from "@angular/core";
import { ActionType, AuditLog, EntityType } from "../models/audit-log.model";

@Injectable({ providedIn: 'root' })
export class AuditLogFormatterService {

    getActionLabel(action: ActionType): string {
    const labels: Record<ActionType, string> = {
      [ActionType.Create]: 'Création',
      [ActionType.Update]: 'Modification',
      [ActionType.Delete]: 'Suppression',
      [ActionType.StatusChange]: 'Changement de statut'
    };
    return labels[action] || 'Inconnu';
  }

  getActionIcon(action: ActionType): string {
    const icons: Record<ActionType, string> = {
      [ActionType.Create]: '➕',
      [ActionType.Update]: '✏️',
      [ActionType.Delete]: '🗑️',
      [ActionType.StatusChange]: '🔄'
    };
    return icons[action] || '📋';
  }

  getActionClass(action: ActionType): string {
    const classes: Record<ActionType, string> = {
      [ActionType.Create]: 'badge-success',
      [ActionType.Update]: 'badge-info',
      [ActionType.Delete]: 'badge-danger',
      [ActionType.StatusChange]: 'badge-warning'
    };
    return classes[action] || 'badge-secondary';
  }

  getEntityTypeLabel(entityType: EntityType): string {
    const labels: Record<EntityType, string> = {
      [EntityType.Piece]: 'Pièce',
      [EntityType.Projet]: 'Projet',
      [EntityType.PrintJob]: 'Impression',
      [EntityType.User]: 'Utilisateur',
      [EntityType.Printer]: 'Imprimante',
      [EntityType.MaterialStock]: 'Stock matière',
      [EntityType.OrdreFabrication]: 'Ordre de fabrication',
      [EntityType.PrintProfile]: "Profil d'impression",
      [EntityType.PrinterMaintenance]: 'Maintenance',
      [EntityType.MaterialConsumption]: 'Consommation matière',
      [EntityType.PrintIncident]: 'Incident',
      [EntityType.Client]: 'Client',
      [EntityType.Devis]: 'Devis',
      [EntityType.Invitation]: 'Invitation',
      [EntityType.Facture]: 'Facture'
    };
    return labels[entityType] || 'Inconnu';
  }

  getEntityIcon(entityType: EntityType): string {
    const icons: Record<EntityType, string> = {
      [EntityType.Piece]: '📦',
      [EntityType.Projet]: '📁',
      [EntityType.PrintJob]: '🖨️',
      [EntityType.User]: '👤',
      [EntityType.Printer]: '🖨️',
      [EntityType.MaterialStock]: '🧱',
      [EntityType.OrdreFabrication]: '📋',
      [EntityType.PrintProfile]: '⚙️',
      [EntityType.PrinterMaintenance]: '🛠️',
      [EntityType.MaterialConsumption]: '📊',
      [EntityType.PrintIncident]: '⚠️',
      [EntityType.Client]: '👥',
      [EntityType.Devis]: '📄',
      [EntityType.Invitation]: '✉️',
      [EntityType.Facture]: '💶'
    };
    return icons[entityType] || '📄';
  }

  formatChangeMessage(log: AuditLog): string {
    if (log.action === ActionType.Create) {
      return `Création de la ${this.getEntityTypeLabel(log.entityType)} "${log.entityName}"`;
    }
    if (log.action === ActionType.Delete) {
      return `Suppression de la ${this.getEntityTypeLabel(log.entityType)} "${log.entityName}"`;
    }
    if (log.action === ActionType.StatusChange) {
      return `Statut: ${log.oldValue} → ${log.newValue}`;
    }
    if (log.action === ActionType.Update && log.fieldName) {
      return `${log.fieldName}: ${log.oldValue} → ${log.newValue}`;
    }
    return 'Modification';
  }
}
