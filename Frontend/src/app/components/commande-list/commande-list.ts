import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Commande, CommandeStatut } from '../../models/cart.model';
import { CommercialService } from '../../services/commercial.service';
import { ToastService } from '../../services/toast.service';

const STATUTS_ANNULABLES: CommandeStatut[] = ['EnAttente', 'Confirmée'];

const STATUT_LABELS: Record<CommandeStatut, string> = {
  EnAttente: 'En attente',
  Confirmée: 'Confirmée',
  EnProduction: 'En production',
  Expédiée: 'Expédiée',
  Livrée: 'Livrée',
  Annulée: 'Annulée'
};

@Component({
  selector: 'app-commande-list',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './commande-list.html',
  styleUrls: ['./commande-list.css'],
})
export class CommandeList implements OnInit {
  commandes: Commande[] = [];
  isLoading = false;
  expandedId: number | null = null;

  // "Annulée" n'est volontairement pas proposée ici : elle ne doit être atteinte
  // que via l'action dédiée "Annuler la commande" (peutAnnuler), pas par un
  // simple changement de statut qui contournerait la règle métier.
  readonly statuts: CommandeStatut[] = ['EnAttente', 'Confirmée', 'EnProduction', 'Expédiée', 'Livrée'];

  constructor(
    private commercialService: CommercialService,
    private toast: ToastService,
    private cdr: ChangeDetectorRef
  ) { }

  ngOnInit(): void {
    this.loadCommandes();
  }

  loadCommandes(): void {
    this.isLoading = true;
    this.commercialService.getCommandes().subscribe({
      next: (data) => {
        this.commandes = data;
        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        this.toast.error(err.message || 'Impossible de charger les commandes');
        this.isLoading = false;
        this.cdr.detectChanges();
      }
    });
  }

  toggleDetail(commande: Commande): void {
    this.expandedId = this.expandedId === commande.id ? null : commande.id;
  }

  peutAnnuler(commande: Commande): boolean {
    return STATUTS_ANNULABLES.includes(commande.statut);
  }

  changerStatut(commande: Commande, nouveauStatut: string): void {
    if (nouveauStatut === commande.statut) return;

    this.commercialService.updateCommandeStatut(commande.id, nouveauStatut).subscribe({
      next: (updated) => {
        commande.statut = updated.statut;
        commande.dateLivraison = updated.dateLivraison;
        this.toast.success(`Commande ${commande.numeroCommande} : statut mis à jour`);
        this.cdr.detectChanges();
      },
      error: (err) => this.toast.error(err.message || 'Impossible de changer le statut')
    });
  }

  annuler(commande: Commande): void {
    if (!confirm(`Annuler la commande ${commande.numeroCommande} ? Le stock réservé sera restitué.`)) return;

    this.commercialService.annulerCommande(commande.id).subscribe({
      next: () => {
        // La commande annulée n'est plus supprimée côté backend (historique conservé) :
        // on met juste à jour son statut localement au lieu de la retirer de la liste.
        commande.statut = 'Annulée';
        this.toast.success(`Commande ${commande.numeroCommande} annulée, stock restitué`);
        this.cdr.detectChanges();
      },
      error: (err) => this.toast.error(err.message || 'Impossible d\'annuler la commande')
    });
  }

  getStatutLabel(statut: CommandeStatut): string {
    return STATUT_LABELS[statut] || statut;
  }

  getStatutClass(statut: string): string {
    const classes: Record<string, string> = {
      'EnAttente': 'badge-warning',
      'Confirmée': 'badge-info',
      'EnProduction': 'badge-info',
      'Expédiée': 'badge-info',
      'Livrée': 'badge-success',
      'Annulée': 'badge-danger'
    };
    return classes[statut] || 'badge-secondary';
  }
}
