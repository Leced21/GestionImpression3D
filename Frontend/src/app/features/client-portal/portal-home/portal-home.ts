import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { ClientPortalAuthService } from '../../../services/client-portal-auth.service';
import { ClientPortalDataService } from '../../../services/client-portal-data.service';
import { PortalCommande, PortalDevis, PortalFacture } from '../../../models/client-portal.model';

type PortalTab = 'devis' | 'factures' | 'commandes';

@Component({
  selector: 'app-portal-home',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './portal-home.html',
  styleUrl: './portal-home.css',
})
export class PortalHome implements OnInit {
  activeTab: PortalTab = 'devis';

  devisList: PortalDevis[] = [];
  factures: PortalFacture[] = [];
  commandes: PortalCommande[] = [];

  expandedId: number | null = null;
  isLoading = true;

  constructor(
    private clientPortalAuth: ClientPortalAuthService,
    private portalData: ClientPortalDataService,
    private cdr: ChangeDetectorRef
  ) {}

  get clientNom(): string {
    return this.clientPortalAuth.getSession()?.clientNom ?? '';
  }

  ngOnInit(): void {
    this.portalData.getDevis().subscribe({
      next: (data) => { this.devisList = data; this.cdr.detectChanges(); },
      error: (err) => console.error('Erreur lors du chargement des devis', err),
    });
    this.portalData.getFactures().subscribe({
      next: (data) => { this.factures = data; this.cdr.detectChanges(); },
      error: (err) => console.error('Erreur lors du chargement des factures', err),
    });
    this.portalData.getCommandes().subscribe({
      next: (data) => {
        this.commandes = data;
        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Erreur lors du chargement des commandes', err);
        this.isLoading = false;
      },
    });
  }

  setTab(tab: PortalTab): void {
    this.activeTab = tab;
    this.expandedId = null;
  }

  toggleExpand(id: number): void {
    this.expandedId = this.expandedId === id ? null : id;
  }

  downloadFacturePdf(facture: PortalFacture, event: Event): void {
    event.stopPropagation();
    this.portalData.downloadFacturePdf(facture.id, facture.numeroFacture);
  }

  logout(): void {
    this.clientPortalAuth.logout();
  }

  devisStatusLabel(statut: string): string {
    const labels: Record<string, string> = {
      Brouillon: '📝 Brouillon',
      Envoyé: '📧 Envoyé',
      Accepté: '✅ Accepté',
      Refusé: '❌ Refusé',
      Expiré: '⏰ Expiré',
    };
    return labels[statut] || statut;
  }

  factureStatusLabel(statut: string): string {
    const labels: Record<string, string> = {
      Émise: '📄 Émise',
      Payée: '✅ Payée',
      Annulée: '❌ Annulée',
    };
    return labels[statut] || statut;
  }

  commandeStatusLabel(statut: string): string {
    const labels: Record<string, string> = {
      EnAttente: '⏳ En attente',
      Confirmée: '✅ Confirmée',
      EnProduction: '🛠️ En production',
      Expédiée: '📦 Expédiée',
      Livrée: '🎉 Livrée',
      Annulée: '❌ Annulée',
    };
    return labels[statut] || statut;
  }
}
