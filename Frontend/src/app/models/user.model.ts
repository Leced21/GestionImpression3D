export interface User {
  id: number;
  email: string;
  nom: string;
  prenom: string;
  role: string;
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