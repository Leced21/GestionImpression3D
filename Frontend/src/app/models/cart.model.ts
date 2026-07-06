export interface CartItem {
  pieceId: number;
  nom: string;
  reference: string;
  prix: number;
  quantite: number;
  stlFileName?: string;
}

// Correspond aux noms de l'enum Backend.Enums.CommandeStatus (sérialisé par son nom via
// JsonStringEnumConverter) : EnAttente/EnProduction n'ont pas d'espace, contrairement aux
// anciennes valeurs "En attente"/"En production" stockées avant la migration vers l'enum.
export type CommandeStatut = 'EnAttente' | 'Confirmée' | 'EnProduction' | 'Expédiée' | 'Livrée' | 'Annulée';

// Correspond exactement à Backend.Models.CommandeLigne.
export interface CommandeLigneResponse {
  id: number;
  commandeId: number;
  pieceId: number;
  nom: string;
  reference: string;
  quantite: number;
  prixUnitaire: number;
  total: number;
}

// Correspond exactement à Backend.Models.Commande.
export interface Commande {
  id: number;
  numeroCommande: string;
  clientId?: number;
  clientNom: string;
  clientEmail: string;
  clientTelephone: string;
  adresseLivraison: string;
  total: number;
  statut: CommandeStatut;
  dateCommande: string;
  dateLivraison?: string;
  notes?: string;
  lignes: CommandeLigneResponse[];
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
