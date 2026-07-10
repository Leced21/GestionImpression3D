// URL relative : en Docker, nginx proxifie /api vers le conteneur backend
// (peu importe le port hôte exposé) ; en local (ng serve), proxy.conf.json
// redirige /api vers http://localhost:5000.
export const API_BASE_URL = '/api';
export const AUTH_TOKEN_KEY = 'token';
export const CURRENT_USER_KEY = 'currentUser';
export const REFRESH_TOKEN_KEY = 'refreshToken';

// Portail client externe : session totalement distincte de l'auth interne (token/audience
// différents côté backend), stockée sous une clé séparée pour ne jamais se mélanger avec
// la session d'un utilisateur interne connecté dans le même navigateur.
export const CLIENT_PORTAL_SESSION_KEY = 'clientPortalSession';
