import { Component } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { take } from 'rxjs';
import { AuthService } from '../../services/auth.service';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './register.html',
  styleUrls: ['./register.css'],
})
export class Register {
  email: string = '';
  password: string = '';
  confirmPassword: string = '';
  nom: string = '';
  prenom: string = '';
  role: string = 'User'; // Par défaut, le rôle est "User"
  isLoading: boolean = false;
  errorMessage: string = '';
  successMessage: string = '';
  isAdminCreating: boolean = false; // Indique si un admin crée un nouvel utilisateur

  constructor(
    private authService: AuthService,
    private router: Router
  ) {
    // Vérifier si l'utilisateur connecté est admin (pour afficher le sélecteur de rôle)
    this.authService.currentUser$.pipe(take(1)).subscribe(user => {
      this.isAdminCreating = user?.role === 'Admin';
    });
  }

  onSubmit(): void {
    // Validation
    if (!this.email || !this.password || !this.nom || !this.prenom) {
      this.errorMessage = 'Tous les champs sont requis';
      return;
    }

    if (this.password !== this.confirmPassword) {
      this.errorMessage = 'Les mots de passe ne correspondent pas';
      return;
    }

    const passwordPattern = /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$/;
    if (!passwordPattern.test(this.password)) {
      this.errorMessage = 'Le mot de passe doit contenir au moins 8 caractères, une majuscule, une minuscule, un chiffre et un caractère spécial (@$!%*?&)';
      return;
    }

    this.isLoading = true;
    this.errorMessage = '';
    this.successMessage = '';

    this.authService.register({
      email: this.email,
      password: this.password,
      nom: this.nom,
      prenom: this.prenom,
      role: this.role // Envoyer le rôle sélectionné
    }).subscribe({
      next: () => {
        this.successMessage = 'Inscription réussie ! Redirection...';
        setTimeout(() => {
          this.router.navigate(['/dashboard']);
        }, 1500);
      },
      error: (err) => {
        this.errorMessage = err.message || 'Erreur lors de l\'inscription';
        this.isLoading = false;
      }
    });
  }
}


