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
  categorie?: string;      // Mécanique, Électronique, Décoration, Outillage
  materiau?: string;       // PLA, PETG, ABS, Résine
  stock?: number;
  imageUrl?: string;
  estDisponible?: boolean;
}
