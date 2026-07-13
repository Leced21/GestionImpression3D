export enum PieceStatus {
  Brouillon = 'Brouillon',
  Conception = 'Conception',
  Prototypage = 'Prototypage',
  Validation = 'Validation',
  Production = 'Production',
  Commercialisable = 'Commercialisable'
}

export interface Piece {
  id: number;
  nom: string;
  reference: string;
  description: string;
  statut: PieceStatus;
  coutMatiere: number;
  coutMachine: number;
  coutMainOeuvre: number;
  prixVente: number;
  stlFileName: string;
  dateCreation: Date;
  dateModification?: Date;
  categorie?: string;      // Mecanique, Electronique, Decoration, Outillage
  materiau?: string;       // PLA, PETG, ABS, Resine
  stock?: number;
  imageUrl?: string;
  estDisponible?: boolean;

  // Champs de la fiche produit (aucune source automatique, saisis une fois ici)
  couleurs?: string;
  capaciteContenance?: string;
  normesCertifications?: string;
  instructionsUtilisation?: string;
  precautionsUsage?: string;
  publicCible?: string;
  conditionnement?: string;
  dimensionsColis?: string;
  poidsColisKg?: number;
  moqUnites?: number;
  delaiLivraisonJours?: number;
  pointsForts?: string;
  faq?: string;
  tarifsDegressifs?: string;
}
