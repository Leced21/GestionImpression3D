import { Component } from '@angular/core';
import { RouterModule } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-forgot-password',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './forgot-password.html',
  styleUrls: ['./forgot-password.css'],
})
export class ForgotPassword {
  email: string = '';
  isLoading: boolean = false;
  errorMessage: string = '';
  successMessage: string = '';

  constructor(private authService: AuthService) {}

  onSubmit(): void {
    if (!this.email) {
      this.errorMessage = 'L\'adresse e-mail est requise';
      return;
    }

    this.isLoading = true;
    this.errorMessage = '';
    this.successMessage = '';

    this.authService.forgotPassword({ email: this.email }).subscribe({
      next: (response) => {
        this.successMessage = response.message;
        this.isLoading = false;
      },
      error: (err) => {
        this.errorMessage = err.message || 'Erreur lors de la demande de réinitialisation';
        this.isLoading = false;
      }
    });
  }
}
