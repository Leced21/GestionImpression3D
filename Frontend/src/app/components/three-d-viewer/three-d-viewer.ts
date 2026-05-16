import { CommonModule } from '@angular/common';
import { AfterViewInit, Component, ElementRef, Input, OnChanges, OnDestroy, OnInit, SimpleChanges, ViewChild } from '@angular/core';
import * as THREE from 'three';
import { STLLoader } from 'three/examples/jsm/loaders/STLLoader.js';
import { OrbitControls } from 'three/examples/jsm/controls/OrbitControls.js';


type ViewType = 'perspective' | 'face' | 'dessus' | 'profil' | 'isometrique';
@Component({
  selector: 'app-three-d-viewer',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './three-d-viewer.html',
  styleUrls: ['./three-d-viewer.css'],
})
export class ThreeDViewer implements OnInit, AfterViewInit, OnChanges, OnDestroy {
  @ViewChild('canvas3d') canvasRef!: ElementRef<HTMLCanvasElement>;
  @Input() stlUrl: string = '';
  @Input() height: string = '500px';

  dimensions?: {
    longueur: number;
    largeur: number;
    hauteur: number;
    volume: number;
    poids: number;
  }

  private scene!: THREE.Scene;
  private camera!: THREE.PerspectiveCamera;
  private renderer!: THREE.WebGLRenderer;
  private controls!: OrbitControls;
  private currentMesh: THREE.Mesh | null = null;
  private boundingBox!: THREE.Box3;
  private animationId: number | null = null;
  private wireframeMode = false;

  currentView: ViewType = 'perspective';
  private viewPositions: Record<ViewType, THREE.Vector3> = {
    perspective: new THREE.Vector3(3, 2, 3),
    face: new THREE.Vector3(0, 0, 5),
    dessus: new THREE.Vector3(0, 5, 0),
    profil: new THREE.Vector3(5, 0, 0),
    isometrique: new THREE.Vector3(3, 3, 3)
  };

  private viewInitialized = false;

  ngOnInit(): void {
    // Le canvas n'est pas encore disponible ici.
  }

  ngAfterViewInit(): void {
    this.viewInitialized = true;
    this.initThreeJS();
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['stlUrl'] && !changes['stlUrl'].firstChange && this.scene) {
      this.loadStl();
    }
  }

  ngOnDestroy(): void {
    if (this.animationId) {
      cancelAnimationFrame(this.animationId);
    }
    if (this.renderer) {
      this.renderer.dispose();
    }
  }

  private initThreeJS(): void {
    if (!this.canvasRef) return;

    const canvas = this.canvasRef.nativeElement;
    const container = canvas.parentElement;
    const width = container?.clientWidth || 800;
    const height = parseInt(this.height) || 500;

    canvas.width = width;
    canvas.height = height;

    // Scene
    this.scene = new THREE.Scene();
    this.scene.background = new THREE.Color(0x1a1a2e);

    // Camera
    this.camera = new THREE.PerspectiveCamera(45, width / height, 0.1, 1000);
    this.camera.position.copy(this.viewPositions.perspective);

    // Renderer
    this.renderer = new THREE.WebGLRenderer({ canvas, antialias: true });
    this.renderer.setSize(width, height);

    // Controls
    this.controls = new OrbitControls(this.camera, canvas);
    this.controls.enableDamping = true;
    this.controls.autoRotate = false;
    this.controls.enableZoom = true;
    this.controls.enablePan = true;

    // Lumières
    const ambientLight = new THREE.AmbientLight(0x404060);
    this.scene.add(ambientLight);

    const directionalLight = new THREE.DirectionalLight(0xffffff, 1);
    directionalLight.position.set(2, 3, 2);
    this.scene.add(directionalLight);

    const backLight = new THREE.DirectionalLight(0x404040, 0.5);
    backLight.position.set(-2, 1, -3);
    this.scene.add(backLight);

    // Grille
    const gridHelper = new THREE.GridHelper(10, 20, 0x888888, 0x444444);
    this.scene.add(gridHelper);

    // Axes (optionnel)
    const axesHelper = new THREE.AxesHelper(5);
    this.scene.add(axesHelper);

    this.animate();

    if (this.stlUrl) {
      this.loadStl();
    }
  }

  private loadStl(): void {
    if (!this.stlUrl) return;

    const loader = new STLLoader();

    loader.load(this.stlUrl, (geometry) => {
      // Supprimer l'ancien mesh
      if (this.currentMesh) {
        this.scene.remove(this.currentMesh);
      }

      // Calculer les dimensions
      geometry.computeBoundingBox();
      this.boundingBox = geometry.boundingBox!;
      const size = this.boundingBox.getSize(new THREE.Vector3());

      // Centrer la géométrie
      const center = this.boundingBox.getCenter(new THREE.Vector3());
      geometry.translate(-center.x, -center.y, -center.z);

      // Calculer les dimensions en mm
      const longueur = Math.round(size.x * 100) / 100;
      const largeur = Math.round(size.y * 100) / 100;
      const hauteur = Math.round(size.z * 100) / 100;
      const volume = Math.round((longueur * largeur * hauteur) / 1000 * 100) / 100;
      const poids = Math.round(volume * 1.2 * 100) / 100; // Densité PLA ~1.2 g/cm³

      this.dimensions = { longueur, largeur, hauteur, volume, poids };

      // Créer le matériau
      const material = new THREE.MeshStandardMaterial({
        color: 0x3b82f6,
        roughness: 0.3,
        metalness: 0.7,
        flatShading: false,
        side: THREE.DoubleSide
      });

      this.currentMesh = new THREE.Mesh(geometry, material);
      this.scene.add(this.currentMesh);

      // Ajuster la caméra à la taille de l'objet
      const maxDim = Math.max(size.x, size.y, size.z);
      const distance = maxDim * 1.5;

      // Mettre à jour les positions des vues
      this.viewPositions = {
        perspective: new THREE.Vector3(distance, distance * 0.8, distance),
        face: new THREE.Vector3(0, 0, distance),
        dessus: new THREE.Vector3(0, distance, 0),
        profil: new THREE.Vector3(distance, 0, 0),
        isometrique: new THREE.Vector3(distance, distance, distance)
      };

      // Appliquer la vue actuelle
      this.setView(this.currentView);

    }, undefined, (error) => {
      console.error('Erreur chargement STL:', error);
    });
  }

  setView(view: ViewType): void {
    this.currentView = view;

    if (!this.currentMesh) return;

    this.camera.position.copy(this.viewPositions[view]);
    this.controls.target.set(0, 0, 0);
    this.controls.update();
  }

  resetView(): void {
    this.setView(this.currentView);
  }

  toggleWireframe(): void {
    this.wireframeMode = !this.wireframeMode;
    if (this.currentMesh) {
      (this.currentMesh.material as THREE.MeshStandardMaterial).wireframe = this.wireframeMode;
    }
  }

  toggleAutoRotate(): void {
    this.controls.autoRotate = !this.controls.autoRotate;
  }

  centerView(): void {
    this.controls.target.set(0, 0, 0);
    this.controls.update();
  }

  private animate(): void {
    const animateLoop = () => {
      this.controls.update();
      this.renderer.render(this.scene, this.camera);
      this.animationId = requestAnimationFrame(animateLoop);
    };
    animateLoop();
  }
}
