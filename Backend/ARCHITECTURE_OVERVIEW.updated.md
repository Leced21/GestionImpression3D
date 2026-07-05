# Architecture de l'application GestionImpression3D

## 1. Vue d'ensemble

GestionImpression3D, également appelée **PrintFlow3D**, est une application full stack de gestion d'un atelier d'impression 3D. Elle couvre le cycle allant de la demande client et du devis jusqu'à la fabrication, la facturation et le suivi du parc de machines.

L'application repose sur :

- un frontend Angular composé de composants standalone ;
- une API REST ASP.NET Core ciblant .NET 10 ;
- une base SQL Server gérée avec Entity Framework Core ;
- une authentification JWT avec refresh token ;
- un portail client externe protégé par lien magique ;
- des notifications temps réel avec SignalR ;
- des exports Excel avec EPPlus et PDF avec QuestPDF ;
- une exécution conteneurisée avec Docker Compose ;
- une chaîne CI/CD GitLab couvrant les tests, les migrations et les déploiements.

## 2. Architecture générale

```text
Navigateur Angular
    |-- JWT interne ----------> API REST ASP.NET Core
    |-- JWT portail client ---> API portail client
    |-- SignalR --------------> NotificationHub
                                      |
Controllers -> Services -> Repositories -> Entity Framework Core -> SQL Server
                                      |
                              Stockage des fichiers STL
```

Le frontend ne communique pas directement avec la base. Les contrôleurs exposent les endpoints HTTP, les services portent les règles métier et les repositories isolent l'accès aux données.

## 3. Frontend Angular

### Structure

- `Frontend/src/main.ts` : démarrage de l'application.
- `Frontend/src/app/app.config.ts` : fournisseurs globaux et intercepteurs HTTP.
- `Frontend/src/app/app.routes.ts` : routes et chargement différé des composants.
- `Frontend/src/app/components/` : composants généraux et domaines historiques.
- `Frontend/src/app/features/` : fonctionnalités métier organisées par domaine.
- `Frontend/src/app/services/` : accès à l'API et services transverses.
- `Frontend/src/app/models/` : modèles TypeScript.
- `Frontend/src/app/guards/` : protection des routes internes et du portail.
- `Frontend/src/app/interceptors/` : gestion des jetons et des erreurs d'authentification.

### Domaines fonctionnels

- dashboard avec statistiques et indicateurs ;
- pièces, Kanban, historique, versions, téléversement, analyse et visualisation STL 3D ;
- projets et associations projet/pièce ;
- catalogue commercial, commandes, clients, devis et factures ;
- imprimantes, profils et travaux d'impression ;
- ordres de fabrication et traçabilité ;
- stocks, consommations et pertes de matière ;
- incidents et maintenances ;
- centre de notifications ;
- administration des utilisateurs, rôles, invitations et journaux d'audit ;
- paramètres utilisateur, thème clair/sombre et couleur principale.

### Routes principales

Les routes privées sont protégées par `AuthGuard`. Les écrans d'administration ajoutent un contrôle de rôle `Admin`.

- `/dashboard`
- `/pieces`, `/pieces/:id`, `/pieces/:id/versions`, `/kanban`
- `/projets`, `/projets/:id`
- `/commercial`, `/clients`, `/devis`
- `/printers`, `/print-jobs`, `/profils-impression`
- `/ordres`
- `/stock`, `/consommations`
- `/incidents`, `/maintenances`
- `/notifications`, `/settings`
- `/admin/users`, `/admin/audit-logs`
- `/login`, `/register`, `/accept-invitation`, `/unauthorized`
- `/portail/demande-acces`, `/portail/acces`, `/portail`

### Authentification interne

`AuthService` gère la connexion, l'inscription, la déconnexion et le renouvellement du jeton. `AuthInterceptor` ajoute le bearer token aux requêtes internes et tente un rafraîchissement lorsqu'il expire. La déconnexion appelle le backend afin de révoquer le refresh token côté serveur.

### Portail client

Le portail client est séparé de l'espace interne :

1. le client demande un accès avec son adresse électronique ;
2. un lien magique à durée limitée est créé ;
3. le lien est consommé une seule fois ;
4. l'API délivre un JWT avec une audience dédiée au portail ;
5. `ClientPortalGuard` et `ClientPortalInterceptor` protègent les écrans et appels correspondants.

Le portail est volontairement en lecture seule. Un jeton client ne peut pas être utilisé sur les endpoints internes, et inversement.

### Apparence

Les paramètres d'apparence sont persistés par le backend. Le thème clair/sombre et la couleur principale sont appliqués globalement au moyen de variables CSS partagées par les composants.

## 4. Backend ASP.NET Core

### Structure en couches

- `Backend/Program.cs` : configuration de l'application et pipeline HTTP.
- `Backend/Controllers/` : endpoints REST et contrôles d'autorisation.
- `Backend/Services/` : logique métier et orchestration.
- `Backend/Repositories/` : requêtes et persistance.
- `Backend/Interface/` : contrats des services et repositories.
- `Backend/DTOs/` : contrats d'entrée et de sortie de l'API.
- `Backend/Models/` et `Backend/Enums/` : modèle métier.
- `Backend/Data/AppDbContext.cs` : configuration Entity Framework Core.
- `Backend/Migrations/` : évolution du schéma SQL Server.
- `Backend/Middleware/ExceptionHandlingMiddleware.cs` : traitement centralisé des erreurs.
- `Backend/Hubs/NotificationHub.cs` : diffusion des notifications SignalR.

### Principaux agrégats métier

- `Piece`, `PieceVersion`, `STLMetadata`
- `Projet`, `ProjetPiece`
- `Client`, `Commande`, `CommandeItem`, `CommandeLigne`
- `Devis`, `DevisLigne`, `Facture`, `FactureLigne`
- `OrdreFabrication`
- `Printer`, `PrintJob`, `PrintProfile`
- `PrintIncident`, `PrinterMaintenance`
- `MaterialStock`, `MaterialConsumption`
- `AppNotification`, `AuditLog`
- `User`, `Invitation`, `Permission`, `RolePermission`, `UserSettings`
- `ClientMagicLink`

### Règles métier importantes

- une commande est reliée à une entité `Client` plutôt qu'à des coordonnées libres ;
- un devis peut être rattaché à un projet et entièrement modifié ;
- l'acceptation d'un devis crée un ordre de fabrication lié pour conserver la traçabilité ;
- l'acceptation génère également une facture à partir des lignes du devis ;
- les devis et factures peuvent être exportés sous forme de véritables documents PDF ;
- les mouvements et pertes de matière sont historisés ;
- les opérations significatives alimentent le journal d'audit.

### API et sérialisation

L'API échange du JSON en `camelCase`, sérialise les enums sous forme de chaînes et ignore les cycles de références. Les erreurs non gérées sont normalisées par le middleware d'exception. Le header `Content-Disposition` est exposé par CORS pour permettre au frontend de récupérer le nom des fichiers exportés.

## 5. Sécurité

Deux schémas JWT sont configurés : le schéma interne par défaut et le schéma `ClientPortal`, avec une audience distincte. La validation contrôle l'émetteur, l'audience, la durée de vie et la signature, avec une tolérance d'expiration fixée à zéro.

Les protections complémentaires comprennent :

- une clé JWT obligatoire d'au moins 32 caractères ;
- des mots de passe hachés avec BCrypt ;
- la révocation des refresh tokens à la déconnexion ;
- une limitation globale des requêtes par adresse IP ;
- une politique plus stricte pour les endpoints d'authentification ;
- des liens magiques temporaires et à usage unique ;
- la prise en charge des adresses transmises par reverse proxy ;
- la journalisation des actions métier et administratives.

## 6. Persistance et fichiers

SQL Server conserve les données métier et les paramètres utilisateur. Entity Framework Core gère le mapping, les relations, les contraintes et les migrations.

Les fichiers téléversés sont stockés dans le volume `uploads`. Les données SQL Server résident dans le volume `sqlserver-data`. La connexion SQL est configurée avec une stratégie de reconnexion. L'application peut attendre la disponibilité du serveur, créer la base manquante et appliquer les migrations uniquement lorsque `Database:ApplyMigrationsOnStartup` est explicitement activé.

## 7. Notifications, analyses et exports

- SignalR diffuse les notifications sur `/notificationHub`.
- Le centre de notifications permet leur consultation et leur gestion.
- `STLAnalyzerService` extrait les métadonnées utiles des modèles 3D.
- EPPlus produit les exports Excel.
- QuestPDF produit les devis et factures PDF.
- La compression des réponses HTTP est activée.

## 8. Observabilité et exploitation

L'API expose :

- `/` pour confirmer la disponibilité de l'API ;
- `/version` pour la version d'assembly et l'environnement ;
- `/health` pour vérifier l'application et l'accès à la base ;
- Swagger UI en environnement de développement.

La journalisation console et debug est activée. Les conteneurs utilisent une rotation des logs afin de limiter leur taille.

## 9. Tests

Le projet `TestProject` contient environ 193 méthodes de test déclarées. Il couvre notamment l'authentification, les refresh tokens, le rate limiting, les clients, le portail client, les pièces, les devis, les factures, les ordres de fabrication, les profils, les travaux et incidents d'impression ainsi que les paramètres utilisateur.

Le frontend contient également des tests Angular, notamment sur les payloads texte envoyés aux endpoints de changement de statut.

## 10. Conteneurisation et CI/CD

Docker Compose orchestre SQL Server, le backend et le frontend servi par Nginx. Des surcharges distinctes existent pour les environnements de développement, staging et production.

Le pipeline GitLab CI/CD réalise :

1. les tests backend et frontend ainsi que le build Angular ;
2. la construction et la publication des images Docker ;
3. l'application des migrations en développement et staging ;
4. le déploiement automatique en développement et staging ;
5. le déploiement manuel en production ;
6. la vérification des endpoints de santé ;
7. le rollback de production en cas d'échec.

Des scripts PowerShell complètent l'exploitation pour l'installation, le déploiement, les sauvegardes, les restaurations, les health checks et le rollback.

## 11. Flux métier principal

```text
Client
  -> Commande / demande
  -> Devis lié au client et éventuellement à un projet
  -> Acceptation du devis
       |-> création de la facture
       `-> création de l'ordre de fabrication
              -> travail d'impression
              -> imprimante et profil
              -> consommation de matière
              -> suivi des incidents et maintenances
```

Ce flux assure la continuité des données commerciales et de production, sans ressaisie des informations client ni perte du lien entre devis et fabrication.

## 12. Axes d'amélioration restants

- stocker les jetons sensibles dans des cookies `HttpOnly`, `Secure` et `SameSite` plutôt que dans `localStorage` ;
- remplacer l'expéditeur de lien magique basé sur les logs par un véritable fournisseur d'e-mails ;
- ajouter des tests end-to-end couvrant les parcours internes et le portail client ;
- enrichir l'observabilité avec des métriques et une corrélation des requêtes ;
- documenter plus précisément les permissions requises par endpoint ;
- automatiser les sauvegardes et tester régulièrement les restaurations ;
- compléter la documentation OpenAPI des contrats et codes d'erreur.

## 13. Conclusion

GestionImpression3D dispose désormais d'une architecture full stack structurée, testée et exploitable dans plusieurs environnements. Les principaux domaines — commerce, production, parc d'imprimantes, matière, maintenance et portail client — sont reliés par des règles métier explicites. Les priorités techniques portent maintenant surtout sur le durcissement du stockage des jetons, l'envoi réel des liens magiques, les tests end-to-end et l'observabilité.
