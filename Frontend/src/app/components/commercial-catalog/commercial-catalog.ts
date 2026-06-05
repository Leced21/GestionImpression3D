import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { Piece } from '../../models/piece.model';
import { PieceService } from '../../services/piece.service';
import { CartItem } from '../../models/cart.model';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-commercial-catalog',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './commercial-catalog.html',
  styleUrls: ['./commercial-catalog.css'],
})
export class CommercialCatalog implements OnInit {

  pieces: Piece[] = [];
  filteredPieces: Piece[] = [];
  cartItems: CartItem[] = [];
  cartOpen = false;
  searchTerm = '';
  filtreCategorie = '';
  filtreMateriau = '';

  constructor(
    private pieceService: PieceService,
    private cdr: ChangeDetectorRef
  ) { }

  ngOnInit(): void {
    this.loadPieces();
    this.loadCart();
  }

  loadPieces(): void {
    this.pieceService.getAll().subscribe({
      next: (data) => {
        this.pieces = data.filter(p => p.statut === 'Commercialisable' && p.prixVente > 0);
        this.filteredPieces = [...this.pieces];
        this.cdr.detectChanges(); // Assure que les changements sont pris en compte immédiatement
        console.log('Pieces loaded:', this.pieces);
      }
    });
  }

  filterPieces(): void {
    this.filteredPieces = this.pieces.filter(piece => {
      const matchSearch = !this.searchTerm ||
        piece.nom.toLowerCase().includes(this.searchTerm.toLowerCase()) ||
        piece.reference.toLowerCase().includes(this.searchTerm.toLowerCase());
      const matchCategorie = !this.filtreCategorie || piece.categorie === this.filtreCategorie;
      const matchMateriau = !this.filtreMateriau || piece.materiau === this.filtreMateriau;
      return matchSearch && matchCategorie && matchMateriau;
    });
  }

  getStock(piece: Piece): number {
    // Simuler un stock (à remplacer par API)
    return piece.stock || 0;
  }

  getProductIcon(piece: Piece): string {
    const icons: Record<string, string> = {
      'Mécanique': '🔧',
      'Électronique': '⚡',
      'Décoration': '🎨',
      'Outillage': '🔨'
    };
    return icons[piece.categorie || 'Mécanique'] || '📦';
  }

  ajouterAuPanier(piece: Piece): void {
    const existing = this.cartItems.find(i => i.pieceId === piece.id);
    if (existing) {
      existing.quantite++;
    } else {
      this.cartItems.push({
        pieceId: piece.id,
        nom: piece.nom,
        reference: piece.reference,
        prix: piece.prixVente,
        quantite: 1,
        stlFileName: piece.stlFileName
      });
    }
    this.saveCart();
  }

  modifierQuantite(index: number, nouvelleQuantite: number): void {
    if (nouvelleQuantite <= 0) {
      this.retirerDuPanier(index);
    } else {
      this.cartItems[index].quantite = nouvelleQuantite;
      this.saveCart();
    }
  }

  retirerDuPanier(index: number): void {
    this.cartItems.splice(index, 1);
    this.saveCart();
  }

  getTotalPanier(): number {
    return this.cartItems.reduce((total, item) => total + (item.prix * item.quantite), 0);
  }

  toggleCart(): void {
    this.cartOpen = !this.cartOpen;
  }

  passerCommande(): void {
    if (this.cartItems.length === 0) return;

    const total = this.getTotalPanier();
    alert(`✅ Commande validée !\nTotal : ${total.toFixed(2)} €\n\nUn email de confirmation va vous être envoyé.`);

    this.cartItems = [];
    this.saveCart();
    this.toggleCart();
  }

  voirDetail(id: number): void {
    window.location.href = `/pieces/${id}`;
  }

  private saveCart(): void {
    localStorage.setItem('cart', JSON.stringify(this.cartItems));
  }

  private loadCart(): void {
    const saved = localStorage.getItem('cart');
    if (saved) {
      this.cartItems = JSON.parse(saved);
    }
  }
}


