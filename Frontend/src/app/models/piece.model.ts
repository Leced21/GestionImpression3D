export interface Piece {
  id: number;
  nom: string;
  reference: string;
  description: string;
  statut: string;
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