import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { Piece, PieceStatus } from '../../models/piece.model';
import { PieceService } from '../../services/piece.service';
import { CartItem, CommandeRequest } from '../../models/cart.model';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { CommercialService } from '../../services/commercial.service';
import { ToastService } from '../../services/toast.service';

@Component({
  selector: 'app-commercial-catalog',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
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

  showCheckoutForm = false;
  isSubmittingCommande = false;
  checkoutInfo = {
    clientNom: '',
    clientEmail: '',
    clientTelephone: '',
    adresseLivraison: '',
    notes: ''
  };

  constructor(
    private pieceService: PieceService,
    private commercialService: CommercialService,
    private toast: ToastService,
    private cdr: ChangeDetectorRef
  ) { }

  ngOnInit(): void {
    this.loadPieces();
    this.loadCart();
  }

  loadPieces(): void {
    this.pieceService.getAll().subscribe({
      next: (data) => {
        this.pieces = data.filter(p => p.statut === PieceStatus.Commercialisable && p.prixVente > 0);
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
    this.showCheckoutForm = true;
  }

  annulerCheckout(): void {
    this.showCheckoutForm = false;
  }

  confirmerCommande(): void {
    if (this.cartItems.length === 0 || this.isSubmittingCommande) return;

    const { clientNom, clientEmail, clientTelephone, adresseLivraison } = this.checkoutInfo;
    if (!clientNom.trim() || !clientEmail.trim() || !clientTelephone.trim() || !adresseLivraison.trim()) {
      this.toast.warn('Merci de renseigner toutes les informations de livraison');
      return;
    }

    const request: CommandeRequest = {
      clientNom: clientNom.trim(),
      clientEmail: clientEmail.trim(),
      clientTelephone: clientTelephone.trim(),
      adresseLivraison: adresseLivraison.trim(),
      notes: this.checkoutInfo.notes.trim() || undefined,
      total: this.getTotalPanier(),
      items: this.cartItems.map(item => ({
        pieceId: item.pieceId,
        nom: item.nom,
        reference: item.reference,
        quantite: item.quantite,
        prixUnitaire: item.prix
      }))
    };

    this.isSubmittingCommande = true;
    this.commercialService.creerCommande(request).subscribe({
      next: (result) => {
        this.toast.success(`✅ Commande ${result.numeroCommande} créée avec succès`);
        this.cartItems = [];
        this.saveCart();
        this.checkoutInfo = { clientNom: '', clientEmail: '', clientTelephone: '', adresseLivraison: '', notes: '' };
        this.showCheckoutForm = false;
        this.isSubmittingCommande = false;
        this.toggleCart();
        this.cdr.detectChanges();
      },
      error: (err) => {
        this.toast.error(err.message || 'Impossible de créer la commande');
        this.isSubmittingCommande = false;
      }
    });
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


