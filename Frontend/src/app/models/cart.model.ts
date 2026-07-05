export interface CartItem {
  pieceId: number;
  nom: string;
  reference: string;
  prix: number;
  quantite: number;
  stlFileName?: string;
}

export interface Commande {
  id: number;
  date: Date;
  client: string;
  email: string;
  telephone: string;
  adresse: string;
  items: CartItem[];
  total: number;
  statut: 'En attente' | 'Confirmée' | 'En production' | 'Expédiée' | 'Livrée';
}

// Correspond exactement au DTO Backend.Models.CommandeItem / CommandeRequest
// (les noms de propriétés doivent matcher le JSON camelCase attendu côté backend).
export interface CommandeItemRequest {
  pieceId: number;
  nom: string;
  reference: string;
  quantite: number;
  prixUnitaire: number;
}

export interface CommandeRequest {
  clientNom: string;
  clientEmail: string;
  clientTelephone: string;
  adresseLivraison: string;
  items: CommandeItemRequest[];
  total: number;
  notes?: string;
}

export interface CommandeCreatedResponse {
  message: string;
  commandeId: number;
  numeroCommande: string;
}
