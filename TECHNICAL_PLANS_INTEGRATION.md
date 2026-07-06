# Guide d'Intégration - Plans Techniques

## 📋 Résumé des modifications

Les plans techniques sont maintenant disponibles via :

### 🔌 Backend
- **Service**: `TechnicalPlanService`
- **Endpoints**:
  - `GET /api/Piece/{id}/technical-plan/pdf` - Plan technique d'une pièce
  - `GET /api/Projet/{id}/technical-plans/pdf` - Plans techniques d'un projet complet

### 🎨 Frontend
- **Service**: `TechnicalPlanService` (src/app/services/technical-plan.service.ts)
- **Méthodes**:
  - `downloadPieceTechnicalPlan(pieceId)` - Télécharge le plan d'une pièce
  - `downloadProjectTechnicalPlans(projectId)` - Télécharge tous les plans du projet

---

## 🔧 Comment intégrer dans vos composants

### 1. Pour afficher un bouton dans la page Détail d'une Pièce

#### Dans votre composant TypeScript :
```typescript
import { TechnicalPlanService } from '../services/technical-plan.service';

export class PieceDetailComponent implements OnInit {
  piece: Piece;

  constructor(
    private technicalPlanService: TechnicalPlanService,
    // ... autres injectés
  ) {}

  downloadPlan(): void {
    this.technicalPlanService.downloadPieceTechnicalPlan(this.piece.id);
  }
}
```

#### Dans votre template HTML :
```html
<button class="btn btn-primary" (click)="downloadPlan()">
  📋 Télécharger le plan technique
</button>
```

---

### 2. Pour afficher un bouton dans la page Détail d'un Projet

#### Dans votre composant TypeScript :
```typescript
import { TechnicalPlanService } from '../services/technical-plan.service';

export class ProjetDetailComponent implements OnInit {
  projet: Projet;

  constructor(
    private technicalPlanService: TechnicalPlanService,
    // ... autres injectés
  ) {}

  downloadPlans(): void {
    this.technicalPlanService.downloadProjectTechnicalPlans(this.projet.id);
  }
}
```

#### Dans votre template HTML :
```html
<button class="btn btn-success" (click)="downloadPlans()">
  📋 Télécharger tous les plans techniques
</button>
```

---

## 📄 Contenu des plans générés

### Plan technique d'une pièce
- ✅ Informations générales (désignation, matériau, catégorie, statut)
- ✅ Dimensions complètes (X, Y, Z en mm)
- ✅ Volume et poids estimés
- ✅ 3 vues orthogonales (face, côté, dessus) avec représentation ASCII
- ✅ Coûts de production (matière, machine, main-d'œuvre, prix de vente)
- ✅ Caractéristiques STL (surface, étanchéité, nombre de triangles, temps d'impression)

### Plans techniques d'un projet
- ✅ Un plan par pièce du projet
- ✅ Référence projet, référence pièce, quantité
- ✅ Mêmes informations que le plan simple
- ✅ Numérotation des pages (page X/Y)

---

## 🎯 Conditions d'accès

Les endpoints nécessitent l'authentification et les rôles :
- **Admin**, **Designer**, **ProductionManager**

```typescript
[Authorize(Roles = "Admin,Designer,ProductionManager")]
```

---

## 📦 Fichiers impactés

### Backend
- ✅ `Backend/Services/TechnicalPlanService.cs` - Service de génération
- ✅ `Backend/Interface/ITechnicalPlanService.cs` - Interface
- ✅ `Backend/Controllers/PieceController.cs` - Endpoint pièce
- ✅ `Backend/Controllers/ProjetController.cs` - Endpoint projet
- ✅ `Backend/Program.cs` - Enregistrement du service

### Frontend
- ✅ `Frontend/src/app/services/technical-plan.service.ts` - Service HTTP

---

## 🚀 Exemple complet : Ajouter un bouton Action dans une liste

```html
<table>
  <tbody>
    <tr *ngFor="let piece of pieces">
      <td>{{ piece.nom }}</td>
      <td>{{ piece.reference }}</td>
      <td>
        <button (click)="downloadPlan(piece.id)" class="btn btn-sm btn-outline-primary">
          📋 Plan
        </button>
      </td>
    </tr>
  </tbody>
</table>
```

```typescript
downloadPlan(pieceId: number): void {
  this.technicalPlanService.downloadPieceTechnicalPlan(pieceId);
}
```

---

## ⚠️ Points importants

1. **STL Metadata requis** : Chaque pièce doit avoir des métadonnées STL (uploadée et analysée)
2. **Format PDF** : Les plans sont générés en PDF A4
3. **Performance** : La génération est asynchrone et peut prendre quelques secondes pour les projets volumineux
4. **Gestion d'erreurs** : Le service affiche les erreurs dans la console et utilise une gestion d'erreurs appropriée

---

## 🔍 Dépannage

### Le téléchargement ne fonctionne pas
- ✅ Vérifiez que l'utilisateur a le bon rôle (Admin, Designer ou ProductionManager)
- ✅ Vérifiez que la pièce/projet existe
- ✅ Vérifiez que la métadonnée STL existe pour la pièce

### Le PDF ne contient pas les vues
- C'est normal ! Les vues sont des représentations ASCII simples calculées à partir du bounding box
- Pour des vues 3D réelles, vous auriez besoin d'une conversion Three.js complète (non implémentée dans cette première version)

---

## 🎨 Personnalisation

Pour personnaliser les plans :
1. Modifiez `TechnicalPlanService.cs` (style PDF, couleurs, disposition)
2. Ajoutez vos logos/en-têtes personnalisés dans `GeneratePlanPdf()`
3. Modifiez les dimensions des vues ASCII dans les méthodes `GenerateXxxViewAscii()`

