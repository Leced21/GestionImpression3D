export interface OrdreFabrication {
  id: number;
  reference: string;
  projetId: number;
  projetNom?: string;
  pieceId: number;
  pieceNom?: string;
  quantite: number;
  quantiteProduite: number;
  statut: OrdreStatus;
  priorite: OrdrePriorite;
  dateEcheance?: Date;
  dateCreation: Date;
  dateDebut?: Date;
  dateFin?: Date;
  notes?: string;
  progression: number;
  estEnRetard: boolean;
}

export interface OrdreStatistics {
  totalOrdres: number;
  enAttente: number;
  enCours: number;
  termines: number;
  annules: number;
  enRetard: number;
  quantiteTotale: number;
  quantiteProduite: number;
  tauxAvancement: number;
}

export enum OrdreStatus {
  EnAttente = 'EnAttente',
  EnCours = 'EnCours',
  Termine = 'Termine',
  Annule = 'Annule'
}

export enum OrdrePriorite {
    Basse = 'Basse',
    Normale = 'Normale',
    Haute = 'Haute',
    Urgente = 'Urgente'
}