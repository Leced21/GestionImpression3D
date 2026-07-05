export interface PrintProfile {
  id: number;
  nom: string;
  description: string;
  printerId: number;
  printerNom?: string;
  materiau: string;
  nozzleTemp: number;
  bedTemp: number;
  layerHeight: number;
  speed: number;
  infill: number;
  infillPattern: string;
  supports: boolean;
  supportType: string;
  materialMultiplier: number;
  isDefault: boolean;
  isActive: boolean;
  createdAt: Date;
}