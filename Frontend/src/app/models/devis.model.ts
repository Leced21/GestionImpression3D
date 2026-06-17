export interface DevisLigne {
  id: number;
  pieceId?: number;
  pieceName?: string;
  description: string;
  quantite: number;
  prixUnitaire: number;
  total: number;
}

export interface Devis {
  id: number;
  numeroDevis: string;
  clientId: number;
  clientNom?: string;
  projetId?: number;
  projetNom?: string;
  dateEmission: Date;
  dateValidite: Date;
  totalHT: number;
  tva: number;
  totalTTC: number;
  statut: string;
  notes?: string;
  conditions?: string;
  lignes: DevisLigne[];
}

export interface DevisStatistics {
  totalDevis: number;
  brouillonCount: number;
  envoyesCount: number;
  acceptesCount: number;
  refusesCount: number;
  expiresCount: number;
  totalAmountAccepted: number;
  averageAmount: number;
}

export enum DevisStatus {
    Brouillon = 'Brouillon',
    Envoye = 'Envoyé',
    Accepte = 'Accepté',
    Refuse = 'Refusé',
    Expire = 'Expiré'
}