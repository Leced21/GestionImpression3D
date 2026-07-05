export enum ProjetStatus {
  Brouillon = 'Brouillon',
  EnCours = 'EnCours',
  Termine = 'Termine'
}

export interface Projet {
  id: number;
  nom: string;
  reference: string;
  description: string;
  statut: ProjetStatus;
  dateCreation: Date;
  dateLivraisonPrevue?: Date;
  clientNom: string;
  clientEmail: string;
  budget: number;
  projetPieces?: ProjetPiece[];
}

export interface ProjetPiece {
  id: number;
  projetId: number;
  pieceId: number;
  quantite: number;
  dateAjout: Date;
  piece?: {
    id: number;
    nom: string;
    reference: string;
    prixVente: number;
  };
}

export interface ProjetStats {
  nombrePieces: number;
  quantiteTotale: number;
  coutTotal: number;
  prixTotal: number;
  marge: number;
}
