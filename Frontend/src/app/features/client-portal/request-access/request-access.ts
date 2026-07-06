import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { ClientPortalAuthService } from '../../../services/client-portal-auth.service';

@Component({
  selector: 'app-request-access',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './request-access.html',
  styleUrl: './request-access.css',
})
export class RequestAccess {
  email = '';
  isSubmitting = false;
  message = '';
  submitted = false;

  constructor(private clientPortalAuth: ClientPortalAuthService) {}

  onSubmit(): void {
    if (!this.email) return;

    this.isSubmitting = true;
    this.clientPortalAuth.requestAccess(this.email).subscribe({
      next: (response) => {
        this.message = response.message;
        this.submitted = true;
        this.isSubmitting = false;
      },
      error: () => {
        // Message volontairement identique en cas d'erreur réseau : on ne distingue
        // jamais "email inconnu" d'un vrai souci, pour ne rien révéler côté client.
        this.message = 'Si cet email correspond à un compte, un lien d\'accès vient d\'être envoyé.';
        this.submitted = true;
        this.isSubmitting = false;
      },
    });
  }
}
