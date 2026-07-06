# GestionImpression3D - Quick Reference & API Examples

## 1. QUICK START - STL FILE WORKFLOW

### Step 1: Create a Piece
```bash
POST /api/Piece
Content-Type: application/json
Authorization: Bearer {token}

{
  "nom": "Bracket Assembly",
  "reference": "BR-001",
  "description": "Metal support bracket",
  "coutMatiere": 5.50,
  "coutMachine": 3.00,
  "coutMainOeuvre": 2.00,
  "prixVente": 25.00,
  "categorie": "Mécanique",
  "materiau": "PLA",
  "stock": 10,
  "estDisponible": true
}

Response: 201 Created
{
  "id": 1,
  "nom": "Bracket Assembly",
  "reference": "BR-001",
  "statut": "Brouillon",
  ...
}
```

### Step 2: Upload STL File
```bash
POST /api/Piece/1/upload-stl
Content-Type: multipart/form-data
Authorization: Bearer {token}

file: [binary STL file]

Response: 200 OK
{
  "fileName": "BR001_a1b2c3d4e5f6.stl",
  "filePath": "/uploads/BR001_a1b2c3d4e5f6.stl",
  "size": 524288
}
```

### Step 3: Analyze STL (Optional)
```bash
POST /api/Piece/1/analyze-stl
Content-Type: multipart/form-data
Authorization: Bearer {token}

file: [binary STL file]

Response: 200 OK
{
  "id": 1,
  "pieceId": 1,
  "fileName": "BR001_a1b2c3d4e5f6.stl",
  "fileSize": 524288,
  "volume": 45.23,
  "surfaceArea": 234.56,
  "boundingBoxX": 120.50,
  "boundingBoxY": 85.30,
  "boundingBoxZ": 95.20,
  "estimatedWeight": 56.08,
  "estimatedPrintTime": 1850.45,
  "triangleCount": 4856,
  "isWatertight": true,
  "hasErrors": false,
  "analyzedAt": "2026-07-06T10:30:45.000Z"
}
```

### Step 4: View in 3D
```javascript
// Frontend
<app-three-d-viewer 
  [stlUrl]="pieceService.getStlUrl(1)"
  [height]="'600px'">
</app-three-d-viewer>

// Service method
getStlUrl(id: number): string {
  return `${this.apiUrl}/${id}/stl`;
}

// Actual API call:
GET /api/Piece/1/stl
Authorization: Bearer {token}

Response: 200 OK
Content-Type: application/octet-stream
Content-Disposition: attachment; filename="BR001_a1b2c3d4e5f6.stl"
[binary STL data]
```

### Step 5: Export Documentation
```bash
# Export as PDF
GET /api/Piece/1/pdf
Authorization: Bearer {token}

Response: 200 OK
Content-Type: application/pdf
Content-Disposition: attachment; filename="Piece_BR-001.pdf"
[binary PDF data]

# Export as Excel
GET /api/Piece/export/excel
Authorization: Bearer {token}

Response: 200 OK
Content-Type: application/vnd.openxmlformats-officedocument.spreadsheetml.sheet
Content-Disposition: attachment; filename="Pieces_20260706.xlsx"
[binary Excel data]
```

---

## 2. THREE.JS VIEWER - COMPONENT USAGE

### Basic Integration
```typescript
// Import
import { ThreeDViewer } from './three-d-viewer/three-d-viewer';

// In component
@Component({
  selector: 'app-my-component',
  imports: [CommonModule, ThreeDViewer],
  template: `
    <app-three-d-viewer 
      [stlUrl]="currentSTLUrl"
      [height]="'700px'"
    ></app-three-d-viewer>
  `
})
export class MyComponent {
  currentSTLUrl = '/api/Piece/1/stl';
}
```

### Viewer Features & Controls

| Feature | Control | Keyboard |
|---------|---------|----------|
| **Rotate** | Left Mouse Drag | - |
| **Pan** | Middle Mouse Drag | Shift + Left Drag |
| **Zoom** | Mouse Wheel | - |
| **Perspective View** | Button Click | - |
| **Front View** | Button Click | - |
| **Top View** | Button Click | - |
| **Side View** | Button Click | - |
| **Isometric** | Button Click | - |
| **Wireframe Toggle** | Button Click | - |
| **Auto-Rotate** | Button Click | - |
| **Reset View** | Button Click | - |
| **Center Model** | Button Click | - |

### Dimension Display
```
📏 Dimensions réelles panel shows:
├─ Longueur (X): Width in mm
├─ Largeur (Y): Depth in mm
├─ Hauteur (Z): Height in mm
├─ Volume: Calculated in cm³
└─ Poids estimé: Weight in grams (density: 1.2g/cm³ for PLA)
```

---

## 3. PIECE LIFECYCLE & STATUS TRANSITIONS

### Piece Status Flow Diagram
```
┌──────────┐     ┌───────────┐     ┌───────────┐
│Brouillon │────>│ Conception│────>│Prototypage│
└──────────┘     └───────────┘     └───────────┘
                                           │
                                           ↓
                                      ┌───────────┐
                                      │ Validation│
                                      └───────────┘
                                           │
                                           ↓
                                      ┌───────────┐
                                      │ Production│
                                      └───────────┘
                                           │
                                           ↓
                                   ┌────────────────┐
                                   │Commercialisable│
                                   └────────────────┘
```

### Status Update Endpoint
```bash
PATCH /api/Piece/1/statut
Content-Type: application/json
Authorization: Bearer {token}

"Conception"  # Direct string value (not JSON object)

Response: 200 OK
{
  "id": 1,
  "statut": "Conception",
  "auditLog": {
    "oldStatus": "Brouillon",
    "newStatus": "Conception",
    "changedAt": "2026-07-06T10:45:30.000Z"
  }
}
```

---

## 4. PRINT JOB CREATION & TRACKING

### Create Print Job
```bash
POST /api/PrintJob
Content-Type: application/json
Authorization: Bearer {token}

{
  "pieceId": 1,
  "quantity": 5,
  "priority": "High",                    # High, Normal, Low
  "estimatedDurationMinutes": 480,
  "estimatedMaterialGrams": 280.4,
  "notes": "Customer requested accelerated production"
}

Response: 201 Created
{
  "id": 42,
  "jobNumber": "JOB-20260706104530000-7534",
  "pieceId": 1,
  "quantity": 5,
  "quantityCompleted": 0,
  "status": "Pending",
  "priority": "High",
  "createdAt": "2026-07-06T10:45:30.000Z",
  "startedAt": null,
  "completedAt": null,
  "estimatedDurationMinutes": 480,
  "estimatedMaterialGrams": 280.4
}
```

### Assign to Printer
```bash
PATCH /api/PrintJob/42/assign-printer
Content-Type: application/json
Authorization: Bearer {token}

{
  "printerId": 3,
  "operatorId": 12
}

Response: 200 OK
{
  "id": 42,
  "status": "Queued",
  "printerId": 3,
  "operatorId": 12,
  "message": "Job assigned to Ultimaker S5 (Operator: John Doe)"
}
```

### Start Printing
```bash
PATCH /api/PrintJob/42/status
Content-Type: application/json
Authorization: Bearer {token}

{
  "newStatus": "Printing"
}

Response: 200 OK
{
  "id": 42,
  "status": "Printing",
  "startedAt": "2026-07-06T11:00:00.000Z"
}
```

### Complete Job
```bash
PATCH /api/PrintJob/42/status
Content-Type: application/json
Authorization: Bearer {token}

{
  "newStatus": "Completed",
  "actualDurationMinutes": 475,
  "actualMaterialGrams": 278.5
}

Response: 200 OK
{
  "id": 42,
  "status": "Completed",
  "completedAt": "2026-07-06T18:55:00.000Z",
  "actualDurationMinutes": 475,
  "actualMaterialGrams": 278.5,
  "quantityCompleted": 5,
  "efficiency": {
    "timeVariance": -0.5,  # minutes saved
    "materialVariance": -1.9  # grams saved
  }
}
```

---

## 5. PRINT PROFILES - MATERIAL PRESETS

### Create Print Profile
```bash
POST /api/PrintProfile
Content-Type: application/json
Authorization: Bearer {token}

{
  "nom": "Standard PLA - Ultimaker S5",
  "description": "Default PLA profile for production prints",
  "printerId": 3,
  "materiau": "PLA",
  "nozzleTemp": 200,
  "bedTemp": 60,
  "layerHeight": 0.2,
  "speed": 60,
  "infill": 20,
  "infillPattern": "Gyroid",
  "supports": true,
  "supportType": "Tree",
  "materialMultiplier": 1.0,
  "isDefault": true,
  "isActive": true
}

Response: 201 Created
{
  "id": 15,
  "nom": "Standard PLA - Ultimaker S5",
  "printerId": 3,
  "materiau": "PLA",
  "settings": {
    "nozzleTemp": 200,
    "bedTemp": 60,
    "layerHeight": 0.2,
    "speed": 60,
    "infill": 20,
    "supports": true
  }
}
```

### Get Printer & Profiles
```bash
GET /api/Printer/3
Authorization: Bearer {token}

Response: 200 OK
{
  "id": 3,
  "nom": "Ultimaker S5",
  "type": "Ultimaker S5 Pro Bundle",
  "statut": "Operational",
  "buildPlateX": 330,
  "buildPlateY": 240,
  "buildPlateZ": 300,
  "maxHotendTemp": 280,
  "maxBedTemp": 110,
  "printProfiles": [
    {
      "id": 15,
      "nom": "Standard PLA - Ultimaker S5",
      "materiau": "PLA",
      "isDefault": true,
      "isActive": true,
      "settings": {...}
    },
    {
      "id": 16,
      "nom": "PETG - High Quality",
      "materiau": "PETG",
      "isDefault": false,
      "isActive": true,
      "settings": {...}
    }
  ]
}
```

---

## 6. VERSION MANAGEMENT

### Create Version
```bash
POST /api/PieceVersions/piece/1
Content-Type: application/json
Authorization: Bearer {token}

{
  "changeLog": "Added support points, optimized infill pattern",
  "isPrototype": false
}

Response: 201 Created
{
  "id": 3,
  "pieceId": 1,
  "versionNumber": 3,
  "createdBy": "john.designer@company.com",
  "createdAt": "2026-07-06T12:30:45.000Z",
  "changeLog": "Added support points, optimized infill pattern",
  "isActive": true,
  "isPrototype": false
}
```

### Get Version History
```bash
GET /api/PieceVersions/piece/1
Authorization: Bearer {token}

Response: 200 OK
[
  {
    "id": 1,
    "versionNumber": 1,
    "nom": "Bracket Assembly",
    "createdBy": "System",
    "createdAt": "2026-06-15T08:00:00.000Z",
    "changeLog": "Version initiale",
    "isActive": false,
    "isPrototype": true
  },
  {
    "id": 2,
    "versionNumber": 2,
    "nom": "Bracket Assembly",
    "createdBy": "alice.designer@company.com",
    "createdAt": "2026-06-20T14:15:30.000Z",
    "changeLog": "Refined dimensions, improved surface finish",
    "isActive": false,
    "isPrototype": true
  },
  {
    "id": 3,
    "versionNumber": 3,
    "nom": "Bracket Assembly",
    "createdBy": "john.designer@company.com",
    "createdAt": "2026-07-06T12:30:45.000Z",
    "changeLog": "Added support points, optimized infill",
    "isActive": true,
    "isPrototype": false
  }
]
```

### Promote to Production
```bash
POST /api/PieceVersions/3/promote
Authorization: Bearer {token}

Response: 200 OK
{
  "id": 3,
  "versionNumber": 3,
  "isActive": true,
  "isPrototype": false,
  "message": "Version 3 promoted to production",
  "timestamp": "2026-07-06T13:00:00.000Z"
}
```

### Compare Versions
```bash
GET /api/PieceVersions/compare?v1=2&v2=3
Authorization: Bearer {token}

Response: 200 OK
{
  "version1": {
    "id": 2,
    "versionNumber": 2,
    "coutMatiere": 5.40,
    "coutMachine": 3.00
  },
  "version2": {
    "id": 3,
    "versionNumber": 3,
    "coutMatiere": 5.50,
    "coutMachine": 3.10
  },
  "hasChanges": true,
  "differences": [
    {
      "field": "coutMatiere",
      "v1Value": 5.40,
      "v2Value": 5.50,
      "variance": 0.10
    },
    {
      "field": "coutMachine",
      "v1Value": 3.00,
      "v2Value": 3.10,
      "variance": 0.10
    }
  ]
}
```

---

## 7. EXPORT OPERATIONS

### Export Single Piece to PDF
```bash
GET /api/Piece/1/pdf
Authorization: Bearer {token}
Accept: application/pdf

Response: 200 OK
Content-Type: application/pdf
Content-Disposition: attachment; filename="Piece_BR-001.pdf"

[PDF contains:
 - Header: "3D Inspire - Fiche Technique"
 - Piece name, reference
 - General information (status, description, creation date)
 - Financial analysis (costs breakdown, selling price)
 - Characteristics (category, material, stock)
]
```

### Export All Pieces to Excel
```bash
GET /api/Piece/export/excel
Authorization: Bearer {token}
Accept: application/vnd.openxmlformats-officedocument.spreadsheetml.sheet

Response: 200 OK
Content-Type: application/vnd.openxmlformats-officedocument.spreadsheetml.sheet
Content-Disposition: attachment; filename="Pieces_20260706.xlsx"

[Excel contains:
 Columns: ID | Nom | Référence | Statut | Catégorie | Matériau | 
          Prix Vente | Coût Matière | Coût Machine | Coût MO | Stock | Date Création
 Rows: One per piece with formatted values
]
```

### Export Project Quotation to PDF
```bash
GET /api/Projet/5/devis
Authorization: Bearer {token}

Response: 200 OK
Content-Type: application/pdf
Content-Disposition: attachment; filename="Devis_Projet_5.pdf"

[PDF contains:
 - Header: "DEVIS" with project reference
 - Date, Client name, Client email
 - Service details table (parts, quantities, unit prices, subtotals)
 - Calculations: Total HT, VAT (20%), Total TTC
 - Terms & Conditions section
 - Validity: 30 days
]
```

---

## 8. MATERIAL CONSUMPTION TRACKING

### Log Material Consumption
```bash
POST /api/MaterialConsumption
Content-Type: application/json
Authorization: Bearer {token}

{
  "printJobId": 42,
  "materialType": "PLA",
  "colorCode": "Blue",
  "quantityUsedGrams": 278.5,
  "wastagePercentage": 2.5,
  "notes": "Successful print, minimal waste"
}

Response: 201 Created
{
  "id": 156,
  "printJobId": 42,
  "materialType": "PLA",
  "quantityUsedGrams": 278.5,
  "quantityWastedGrams": 7.2,
  "totalQuantityGrams": 285.7,
  "recordedAt": "2026-07-06T18:55:00.000Z",
  "recordedBy": "operator@company.com"
}
```

### Get Material Stock
```bash
GET /api/MaterialStock
Authorization: Bearer {token}

Response: 200 OK
[
  {
    "id": 1,
    "name": "PLA Blue",
    "type": "Filament",
    "brand": "MatterHackers",
    "color": "Blue",
    "quantity": 8500,
    "unit": "grams",
    "minThreshold": 2000,
    "unitPrice": 0.015,
    "totalValue": 127.50,
    "location": "Shelf A1",
    "supplier": "MatterHackers",
    "isLowStock": false
  },
  {
    "id": 2,
    "name": "PETG Red",
    "type": "Filament",
    "quantity": 1200,
    "minThreshold": 2000,
    "isLowStock": true  # ⚠️ Below minimum
  }
]
```

---

## 9. PRINT JOB STATUS CODES

```
Pending     → Initial state, waiting for printer assignment
Queued      → Assigned to printer, waiting in queue
Printing    → Currently printing
Paused      → Print paused by operator
Completed   → Print finished successfully
Failed      → Print failed (check failureReason)
Cancelled   → Print cancelled by user
```

---

## 10. ERROR RESPONSES

### Common HTTP Status Codes
```
200 OK                   → Successful operation
201 Created              → Resource created successfully
400 Bad Request          → Invalid input or validation error
401 Unauthorized         → Missing or invalid token
403 Forbidden            → User lacks required permissions
404 Not Found            → Resource not found
409 Conflict             → Invalid state transition
500 Internal Server Error → Server error
```

### Example Error Response
```json
{
  "error": "Fichier trop volumineux. Taille maximale: 100 Mo",
  "statusCode": 400,
  "timestamp": "2026-07-06T10:45:30.000Z",
  "path": "/api/Piece/1/upload-stl"
}
```

---

## 11. AUTHENTICATION & AUTHORIZATION

### Login & Get Token
```bash
POST /api/Auth/login
Content-Type: application/json

{
  "username": "designer@company.com",
  "password": "SecurePassword123!"
}

Response: 200 OK
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresIn": 3600,
  "currentUser": {
    "id": 5,
    "username": "designer@company.com",
    "roles": ["Designer", "User"]
  }
}
```

### Role-Based Access
```
Admin                → All operations
Designer             → Create/modify pieces, upload STL, versions
ProductionManager    → Create/assign print jobs, manage printing
Operator             → Execute print jobs, log consumption
Viewer               → Read-only access (viewing models, stats)
User                 → Default role for authenticated users
```

### Authorization Headers
```bash
# Include token in all requests
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

---

## 12. FILE SIZE LIMITS & SPECIFICATIONS

| Item | Limit | Note |
|------|-------|------|
| STL Upload | 100 MB | Max file size |
| STL Format | Binary/ASCII | Both supported |
| File Types | .stl, .step, .3mf | Extensions allowed |
| Recommended Size | < 10 MB | For smooth 3D viewer |
| Max Triangles | 1,000,000 | Practical limit |
| Metadata Storage | Unlimited | In database |

---

## 13. COMMON WORKFLOWS

### Workflow 1: Design → Print → Track
```
1. Designer creates Piece ✓
2. Designer uploads STL file ✓
3. System analyzes geometry ✓
4. Designer reviews in 3D viewer ✓
5. Designer changes status to "Conception" ✓
6. Designer creates v2 of piece ✓
7. Production Manager creates PrintJob ✓
8. Production Manager assigns to printer ✓
9. Operator starts printing ✓
10. System tracks material consumption ✓
11. Operator completes job ✓
```

### Workflow 2: Multi-Piece Project → Quotation
```
1. Commercial creates Project ✓
2. Commercial adds pieces via ProjetPiece ✓
3. System calculates total cost ✓
4. Commercial exports Quotation PDF ✓
5. Client receives proposal ✓
```

### Workflow 3: Version Control & Approval
```
1. Designer creates Piece v1 (prototype) ✓
2. Designer uploads initial STL ✓
3. Designer creates Piece v2 with changes ✓
4. Designer creates Piece v3 (refined) ✓
5. Designer promotes v3 to production ✓
6. Production uses v3 for all print jobs ✓
7. System can compare versions on demand ✓
```

---

## 14. PERFORMANCE TIPS

1. **Large STL Files**
   - Optimize mesh before upload (remove internal triangles)
   - Consider splitting into multiple pieces
   - Use binary format instead of ASCII

2. **3D Viewer**
   - Models < 1MB load instantly
   - Models > 50MB may have performance issues
   - Multiple viewers on same page: disable auto-rotate

3. **Database Queries**
   - Export operations create temporary data
   - Use pagination for large lists
   - Index commonly filtered fields

4. **File Storage**
   - Monitor /uploads directory size
   - Implement cleanup for deleted pieces
   - Use CDN for static file serving

---

This quick reference guide provides practical examples and workflows for common operations in GestionImpression3D.
