# Architecture de l'application GestionImpression3D

## 1) Vue d'ensemble

L'application est une solution full stack composée de :
- un frontend Angular standalone,
- un backend .NET 10 Web API,
- une base de données SQL Server gérée par Entity Framework Core,
- un flux REST/JSON entre frontend et backend,
- une authentification JWT avec refresh token,
- des notifications en temps réel via SignalR.

## 2) Frontend

### Structure principale
- `src/main.ts` : bootstrap de l'application avec `bootstrapApplication(App, appConfig)`.
- `src/app/app.ts` : composant racine.
- `src/app/app.config.ts` : configuration globale des providers.
- `src/app/app.routes.ts` : définition des routes.

### Gestion de l'authentification
- `src/app/services/auth.service.ts`
  - login / register
  - stockage de `token`, `refreshToken` et `currentUser` dans `localStorage`
  - détection d'expiration du token
  - refresh token via `POST /api/auth/refresh`
- `src/app/interceptors/auth.interceptor.ts`
  - ajoute le header `Authorization: Bearer <token>` aux requêtes API
  - tente le rafraîchissement du token en cas de 401 ou de token expiré
- `src/app/guards/auth.guard.ts`
  - protège les routes privées
  - contrôle l'accès en fonction des rôles

### Routing
Routes principales :
- `/dashboard`
- `/pieces`, `/pieces/:id`, `/kanban`
- `/projets`, `/projets/:id`
- `/printers`, `/print-jobs`
- `/stock`
- `/notifications`
- `/admin/users`, `/admin/audit-logs`
- `/login`, `/register`, `/unauthorized`

### Fonctionnalités UI
- Dashboard et visualisation des statistiques
- Gestion des pièces, projets, imprimantes, travaux d'impression et stock de matière
- Administration et gestion des utilisateurs
- Notifications temps réel
- Composants standalone réutilisables

## 3) Backend

### Point d'entrée
- `Backend/Program.cs`
  - configuration de l'API, des services, de l'authentification, du CORS et de SignalR
  - activation de Swagger en développement
  - définition du pipeline middleware

### Architecture en couches
- `Backend/Data/AppDbContext.cs` : contexte EF Core
- `Backend/Models/` : entités métier
- `Backend/Repositories/` : accès aux données
- `Backend/Services/` : logique métier
- `Backend/Controllers/` : API REST
- `Backend/Hubs/NotificationHub.cs` : notifications temps réel

### Authentification
- JWT Bearer configuré avec validation : issuer, audience, lifetime, signature
- support du refresh token côté backend
- `AuthController` expose les endpoints : `login`, `register`, `refresh`

### CORS
- politique `AllowAngular` pour `http://localhost:4200`
- autorise toutes les méthodes et en-têtes
- expose `Content-Disposition` pour les téléchargements

### SignalR
- `MapHub<NotificationHub>("/notificationHub")`
- permet au backend d'envoyer des notifications temps réel au frontend

## 4) Flux de données

1. L'utilisateur interagit avec l'UI Angular.
2. Le frontend appelle un service Angular.
3. `AuthInterceptor` ajoute le token JWT si nécessaire.
4. La requête HTTP atteint le backend.
5. Le backend valide le JWT et appelle le contrôleur.
6. Le contrôleur délègue au service métier.
7. Le service utilise un repository pour accéder à la DB via EF Core.
8. Le backend renvoie du JSON camelCase.
9. Le frontend affiche les données.
10. Si le token est expiré, le frontend peut appeler `/api/auth/refresh`.

## 5) Principaux composants métiers

- `Piece` : gestion des pièces et versions
- `Projet` : gestion des projets
- `Printer` : gestion des imprimantes
- `PrintJob` : gestion des travaux d'impression
- `MaterialStock` : gestion du stock
- `Dashboard` : statistiques et indicateurs
- `Notification` : centre de notifications
- `User` / `Admin` : gestion des utilisateurs et audit

## 6) Points d'amélioration prioritaires

- révoquer le refresh token côté backend lors du logout
- implémenter le refresh silencieux côté frontend
- stocker le refresh token en cookie HttpOnly sécurisé
- ajouter des tests unitaires pour `AuthService`
- couvrir le flux d'authentification avec des tests E2E
- ajouter un indicateur de chargement global
- basculer les mises à jour d'imprimantes sur WebSocket/SignalR
- ajouter un rate limiting côté backend

## 7) Conclusion

L'architecture est bien séparée en couches frontend / backend / persistence. Le flux d'authentification JWT + refresh token est déjà en place, et le backend expose une API REST cohérente avec le frontend Angular. Le prochain axe prioritaire est de durcir l'authentification (refresh silencieux, token HttpOnly, invalidation logout) et d'ajouter des tests pour sécuriser l'évolution.
