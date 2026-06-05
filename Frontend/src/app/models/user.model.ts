export interface User {
  id: number;
  email: string;
  nom: string;
  prenom: string;
  role: string;
  isActive: boolean;
  dateCreation: Date;
  fullName: string;
}

export interface Invitation {
  id: number;
  email: string;
  token: string;
  role: string;
  expiresAt: Date;
  createdAt: Date;
  isUsed: boolean;
}

export interface Permission {
  id: number;
  name: string;
  description: string;
  category: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  email: string;
  password: string;
  nom: string;
  prenom: string;
  role: string; // Ajout du rôle lors de l'inscription
}

export interface AuthResponse {
  id: number;
  email: string;
  nom: string;
  prenom: string;
  role: string;
  token: string;
  expiration: Date;
}
