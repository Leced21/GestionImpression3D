import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, ElementRef, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { Piece, PieceStatus } from '../../models/piece.model';
import { PieceService } from '../../services/piece.service';
import { Subject, takeUntil } from 'rxjs';
import * as THREE from 'three';
import { STLLoader } from 'three/examples/jsm/loaders/STLLoader.js';
import { OrbitControls } from 'three/examples/jsm/controls/OrbitControls.js';
import { ThreeDViewer } from '../three-d-viewer/three-d-viewer';
import { ExportService } from '../../services/export.service';
import { PieceVersions } from '../piece-versions/piece-versions';

@Component({
  selector: 'app-piece-detail',
  imports: [CommonModule, RouterModule, ThreeDViewer, PieceVersions],
  standalone: true,
  templateUrl: './piece-detail.html',
  styleUrls: ['./piece-detail.css'],
})
export class PieceDetail implements OnInit, OnDestroy {
  piece: Piece | null = null;
  prixRecommande: number = 0;
  activeTab: string = 'general';
  statuts: PieceStatus[] = Object.values(PieceStatus);
  PieceStatus = PieceStatus;
  versionNumber: number = 1;
  private destroy$ = new Subject<void>();
  @ViewChild('canvas3d') canvasRef!: ElementRef<HTMLCanvasElement>;
  private scene!: THREE.Scene;
  private camera!: THREE.PerspectiveCamera;
  private renderer!: THREE.WebGLRenderer;
  private controls!: OrbitControls;
  private currentMesh: THREE.Mesh | null = null;
  private wireframeMode = false;
  private animationId!: number;

  uploading = false;
  uploadProgress = 0;
  uploadError = '';

  versions = [
    { date: new Date(), comment: 'Version initiale' }
  ];

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private pieceService: PieceService,
    private cdr: ChangeDetectorRef,
    private exportService: ExportService
  ) { }

  ngOnInit(): void {
    const id = this.route.snapshot.params['id'];
    if (id) {
      this.loadPiece(id);
    }
  }
  ngAfterViewInit(): void { }
  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();

    // 1. Arrête l'animation
    if (this.animationId) {
      cancelAnimationFrame(this.animationId);
    }
    // 2. Nettoie les contrôles et le renderer
    if (this.renderer) {
      this.renderer.dispose();
      this.renderer.forceContextLoss();
    }
    if (this.controls) {
      this.controls.dispose();
    }
  }
  loadPiece(id: number): void {
    this.pieceService.getById(id).pipe(takeUntil(this.destroy$)).subscribe({
      next: (data) => {
        this.piece = data;
        this.loadPrixRecommande(id);
        this.extractVersionNumber();
        this.cdr.detectChanges(); // Assure que les changements sont pris en compte immédiatement
        if (this.piece?.stlFileName) {
          setTimeout(() => {
            if (this.canvasRef && this.canvasRef.nativeElement) {
              this.initThreeJS();
              this.loadStlFile();
            } else {
              console.warn('Canvas reference not found');
            }
          }, 50);
        }
      },
      error: (err) => console.error('Erreur chargement:', err)
    });
  }

  loadPrixRecommande(id: number): void {
    this.pieceService.getPrixRecommande(id).pipe(takeUntil(this.destroy$)).subscribe({
      next: (prix) => this.prixRecommande = prix,
      error: (err) => console.error('Erreur calcul prix:', err)
    });
  }

  extractVersionNumber(): void {
    if (this.piece?.reference) {
      const match = this.piece.reference.match(/v(\d+)/i);
      if (match) {
        this.versionNumber = parseInt(match[1]);
      }
    }
  }

  getCoutTotal(): number {
    if (!this.piece) return 0;
    return this.piece.coutMatiere + this.piece.coutMachine + this.piece.coutMainOeuvre;
  }

  getMarge(): number {
    if (!this.piece || !this.piece.prixVente) return 0;
    return this.piece.prixVente - this.getCoutTotal();
  }

  getMargePourcentage(): number {
    const coutTotal = this.getCoutTotal();
    return coutTotal > 0 ? (this.getMarge() / coutTotal) * 100 : 0;
  }

  getBadgeClass(statut: string): string {
    const classes: Record<string, string> = {
      'Brouillon': 'badge-Brouillon',
      'Conception': 'badge-Conception',
      'Prototypage': 'badge-Prototypage',
      'Validation': 'badge-Validation',
      'Production': 'badge-Production',
      'Commercialisable': 'badge-Commercialisable'
    };
    return classes[statut] || 'badge-Brouillon';
  }

  isStepCompleted(step: PieceStatus | string): boolean {
    if (!this.piece) return false;
    const currentIndex = this.statuts.indexOf(this.piece.statut);
    const stepIndex = this.statuts.indexOf(step as PieceStatus);
    return stepIndex < currentIndex;
  }

  nextStatus(): void {
    if (!this.piece) return;

    const currentIndex = this.statuts.indexOf(this.piece.statut);
    if (currentIndex < this.statuts.length - 1) {
      const nextStatut = this.statuts[currentIndex + 1];

      this.pieceService.updateStatus(this.piece.id, nextStatut).subscribe({
        next: () => {
          this.loadPiece(this.piece!.id); // Recharge les données pour mettre à jour l'affichage
        },
        error: (err) => console.error('Erreur changement statut:', err)
      });
    }
  }

  applyRecommendedPrice(): void {
    if (!this.piece) return;

    const updatedPiece = { ...this.piece, prixVente: this.prixRecommande };

    this.pieceService.update(this.piece.id, updatedPiece).subscribe({
      next: () => {
        this.loadPiece(this.piece!.id); // Recharge les données pour mettre à jour l'affichage
      },
      error: (err) => console.error('Erreur mise à jour prix:', err)
    });
  }

  deletePiece(): void {
    if (!this.piece) return;

    if (confirm(`Supprimer définitivement la pièce "${this.piece.nom}" ?`)) {
      this.pieceService.delete(this.piece.id).subscribe({
        next: () => {
          this.router.navigate(['/pieces']);
        },
        error: (err) => console.error('Erreur suppression:', err)
      });
    }
  }

  createNewVersion(): void {
    if (!this.piece) return;

    // Créer une nouvelle version en incrémentant le numéro
    const newVersionNumber = this.versionNumber + 1;
    const newReference = this.piece.reference.replace(/v\d+/, `v${newVersionNumber}`);

    const newPiece = {
      nom: this.piece.nom,
      reference: newReference,
      description: this.piece.description,
      coutMatiere: this.piece.coutMatiere,
      coutMachine: this.piece.coutMachine,
      coutMainOeuvre: this.piece.coutMainOeuvre,
      statut: PieceStatus.Brouillon
    };

    this.pieceService.create(newPiece).subscribe({
      next: (created) => {
        this.router.navigate(['/pieces', created.id]);
      },
      error: (err) => console.error('Erreur création version:', err)
    });
  }

  printPiece(): void {
    if (!this.piece) return;

    const printWindow = window.open('', '_blank');
    if (printWindow) {
      printWindow.document.write(`
        <html>
          <head>
            <title>${this.piece.nom} - Fiche technique</title>
            <style>
              body { font-family: Arial, sans-serif; padding: 2rem; }
              h1 { color: #1e293b; }
              .info { margin: 1rem 0; padding: 1rem; border: 1px solid #e2e8f0; border-radius: 0.5rem; }
              .label { font-weight: bold; color: #64748b; }
            </style>
          </head>
          <body>
            <h1>${this.piece.nom}</h1>
            <p>Référence: ${this.piece.reference}</p>
            <div class="info">
              <p><span class="label">Statut:</span> ${this.piece.statut}</p>
              <p><span class="label">Description:</span> ${this.piece.description || 'Aucune'}</p>
              <p><span class="label">Coût total:</span> ${this.getCoutTotal().toFixed(2)} €</p>
              <p><span class="label">Prix de vente:</span> ${this.piece.prixVente?.toFixed(2) || 'Non défini'} €</p>
              <p><span class="label">Date création:</span> ${new Date(this.piece.dateCreation).toLocaleDateString()}</p>
            </div>
          </body>
        </html>
      `);
      printWindow.document.close();
      printWindow.print();
    }
  }
  initThreeJS(): void {

    if (this.renderer) {
      return; // Déjà initialisé
    }
    const canvas = this.canvasRef.nativeElement;

    this.scene = new THREE.Scene();
    this.scene.background = new THREE.Color(0x1a1a2e);

    this.camera = new THREE.PerspectiveCamera(45, 1, 0.1, 1000);
    this.camera.position.set(2, 2, 2);
    this.camera.lookAt(0, 0, 0);

    this.renderer = new THREE.WebGLRenderer({ canvas, antialias: true });
    this.renderer.setSize(canvas.clientWidth || 400, canvas.clientHeight || 400);

    // Contrôles
    this.controls = new OrbitControls(this.camera, canvas);
    this.controls.enableDamping = true;
    this.controls.dampingFactor = 0.05;
    this.controls.autoRotate = true;
    this.controls.autoRotateSpeed = 2;

    // Lumières
    const ambientLight = new THREE.AmbientLight(0x404040);
    this.scene.add(ambientLight);

    const directionalLight = new THREE.DirectionalLight(0xffffff, 1);
    directionalLight.position.set(2, 3, 4);
    this.scene.add(directionalLight);

    const backLight = new THREE.DirectionalLight(0x404040, 0.5);
    backLight.position.set(-2, 1, -3);
    this.scene.add(backLight);

    // Grille et axes
    const gridHelper = new THREE.GridHelper(5, 20, 0x888888, 0x444444);
    this.scene.add(gridHelper);

    this.animate();
  }

  loadStlFile(): void {
    if (!this.piece?.stlFileName || !this.scene) return;

    const loader = new STLLoader();
    const url = this.pieceService.getStlUrl(this.piece.id);

    console.log("URL envoyée au STLLoader :", url);
    // 💡 RÉCUPÉRATION DU TOKEN : À adapter selon ta méthode de stockage (localStorage, injecté via AuthService, etc.)
    const token = localStorage.getItem('token') || sessionStorage.getItem('token');
    console.log("Token récupéré pour Three.js :", token);
    // 💡 INJECTION DU TOKEN : Si le token existe, on l'ajoute aux en-têtes de Three.js
    if (token) {
      loader.setRequestHeader({
        'Authorization': `Bearer ${token}`
      });
    } else {
      console.warn("Aucun token trouvé pour l'authentification Three.js");
    }

    loader.load(url, (geometry) => {
      // Supprimer l'ancien mesh
      if (this.currentMesh) {
        this.scene.remove(this.currentMesh);
      }

      // Centrer la géométrie
      geometry.computeBoundingBox();
      const center = geometry.boundingBox!.getCenter(new THREE.Vector3());
      geometry.translate(-center.x, -center.y, -center.z);

      // Créer le matériau
      const material = new THREE.MeshStandardMaterial({
        color: 0x3b82f6,
        roughness: 0.4,
        metalness: 0.6,
        flatShading: false,
        side: THREE.DoubleSide
      });

      this.currentMesh = new THREE.Mesh(geometry, material);
      this.scene.add(this.currentMesh);

      // Ajuster la caméra
      const boundingBox = geometry.boundingBox;
      const size = boundingBox!.getSize(new THREE.Vector3());
      const maxDim = Math.max(size.x, size.y, size.z);
      const distance = maxDim * 2;
      this.camera.position.set(distance, distance * 0.8, distance);
      this.camera.lookAt(0, 0, 0);
      this.controls.target.set(0, 0, 0);
      this.controls.update();
      this.cdr.detectChanges(); // Assure que les changements sont pris en compte immédiatement
    }, undefined, (error) => {
      console.error('Erreur chargement STL:', error);
    });
  }

  animate(): void {
    this.animationId = requestAnimationFrame(() => this.animate());
    this.controls.update(); // Met à jour autoRotate si activé
    this.renderer.render(this.scene, this.camera);
  }

  resetView(): void {
    this.camera.position.set(2, 2, 2);
    this.camera.lookAt(0, 0, 0);
    this.controls.target.set(0, 0, 0);
    this.controls.update();
  }

  toggleWireframe(): void {
    this.wireframeMode = !this.wireframeMode;
    if (this.currentMesh) {
      (this.currentMesh.material as THREE.MeshStandardMaterial).wireframe = this.wireframeMode;
    }
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      this.uploadFile(input.files[0]);
    }
  }

  onDragOver(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
  }

  onFileDrop(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    const files = event.dataTransfer?.files;
    if (files && files.length > 0) {
      this.uploadFile(files[0]);
    }
  }

  uploadFile(file: File): void {
    this.uploading = true;
    this.uploadProgress = 0;
    this.uploadError = '';

    // Simuler progression
    const interval = setInterval(() => {
      if (this.uploadProgress < 90) {
        this.uploadProgress += 10;
      }
    }, 200);

    this.pieceService.uploadStl(this.piece!.id, file).subscribe({
      next: (response) => {
        clearInterval(interval);
        this.uploadProgress = 100;
        setTimeout(() => {
          this.uploading = false;
          this.piece!.stlFileName = response.fileName;
          this.loadStlFile();
        }, 500);
      },
      error: (err) => {
        clearInterval(interval);
        this.uploading = false;
        this.uploadError = err.error?.error || 'Erreur lors de l\'upload';
      }
    });
  }

  retryUpload(): void {
    this.uploadError = '';
    // Re-sélectionner le fichier (à implémenter)
  }

  downloadStl(): void {
    if (this.piece?.stlFileName) {
      window.open(this.pieceService.getStlUrl(this.piece.id), '_blank');
    }
  }
  previousStatus(): void {
    if (!this.piece) return;
    const statuts = Object.values(PieceStatus);
    const index = statuts.indexOf(this.piece.statut);
    if (index > 0) {
      this.pieceService.updateStatus(this.piece.id, statuts[index - 1]).subscribe({
        next: (updated) => { this.piece = updated; }
      });
    }
  }
  rendreCommercialisable(): void {
    if (!this.piece) return;
    this.pieceService.updateStatus(this.piece.id, PieceStatus.Commercialisable).subscribe({
      next: (updated) => { this.piece = updated; }
    });
  }
  getStock(): number {
    return Math.floor(Math.random() * 100) + 10;
  }
  getTempsImpression(): string {
    const minutes = Math.round(this.getCoutTotal() * 15);
    const heures = Math.floor(minutes / 60);
    const mins = minutes % 60;
    return `${heures}h${mins.toString().padStart(2, '0')}`;
  }

  getPrixRecommande(): number {
    return this.getCoutTotal() * 1.3;
  }
  getElectricite(): number {
    return this.getCoutTotal() * 0.05;
  }
  getMatiereNom(): string {
    return 'PLA - 350g';
  }
  getStatusClass(statut: string): string {
    const classes: Record<string, string> = {
      'Brouillon': 'status-brouillon',
      'Conception': 'status-conception',
      'Prototypage': 'status-prototypage',
      'Validation': 'status-validation',
      'Production': 'status-production',
      'Commercialisable': 'status-commercialisable'
    };
    return classes[statut] || 'status-brouillon';
  }
  lancerImpression(): void {
    alert(`Impression lancée pour ${this.piece?.nom}`);
  }
  getStlUrl(): string {
    if (!this.piece?.stlFileName) return '';
    return this.pieceService.getStlUrl(this.piece.id);
  }
  exportPdf(): void {
    if (!this.piece) {
      console.warn("Impossible d'exporter : aucune pièce n'est chargée.");
      return;
    }
    this.exportService.exportPiecePdf(this.piece.id).subscribe({
      next: (blob) => {
        this.exportService.downloadPdf(blob, `Piece_${this.piece?.reference}.pdf`);
      },
      error: (err) => console.error('Erreur export PDF:', err)
    });
  }

  exportFicheProduitPdf(): void {
    if (!this.piece) {
      console.warn("Impossible d'exporter : aucune pièce n'est chargée.");
      return;
    }
    this.exportService.exportFicheProduitPdf(this.piece.id).subscribe({
      next: (blob) => {
        this.exportService.downloadPdf(blob, `FicheProduit_${this.piece?.reference}.pdf`);
      },
      error: (err) => console.error('Erreur export fiche produit:', err)
    });
  }
}


