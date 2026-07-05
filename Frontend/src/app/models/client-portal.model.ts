export interface ClientPortalSession {
  token: string;
  expiration: string;
  clientId: number;
  clientNom: string;
}

export interface PortalLigne {
  id: number;
  pieceId?: number;
  description: string;
  quantite: number;
  prixUnitaire: number;
  total: number;
}

export interface PortalDevis {
  id: number;
  numeroDevis: string;
  clientId: number;
  projetId?: number;
  dateEmission: string;
  dateValidite: string;
  totalHT: number;
  tva: number;
  totalTTC: number;
  statut: string;
  notes?: string;
  lignes: PortalLigne[];
}

export interface PortalFacture {
  id: number;
  numeroFacture: string;
  devisId: number;
  clientId: number;
  dateEmission: string;
  dateEcheance: string;
  totalHT: number;
  tva: number;
  totalTTC: number;
  statut: string;
  notes?: string;
  lignes: PortalLigne[];
}

export interface PortalCommandeLigne {
  id: number;
  pieceId: number;
  nom: string;
  reference: string;
  quantite: number;
  prixUnitaire: number;
  total: number;
}

export interface PortalCommande {
  id: number;
  numeroCommande: string;
  clientId?: number;
  adresseLivraison: string;
  total: number;
  statut: string;
  dateCommande: string;
  dateLivraison?: string;
  notes?: string;
  lignes: PortalCommandeLigne[];
}
