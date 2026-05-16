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