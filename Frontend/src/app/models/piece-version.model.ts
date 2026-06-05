export interface PieceVersion {
  id: number;
  pieceId: number;
  versionNumber: number;
  nom: string;
  description: string;
  coutMatiere: number;
  coutMachine: number;
  coutMainOeuvre: number;
  prixVente: number;
  stlFileName: string;
  changeLog?: string;
  createdBy: string;
  createdAt: Date;
  isActive: boolean;
  isPrototype: boolean;
}
