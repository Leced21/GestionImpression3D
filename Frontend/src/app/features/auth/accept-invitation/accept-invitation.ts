import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../../services/auth.service';
import { AdminService } from '../../../services/admin.service';
import { ActivatedRoute, Router } from '@angular/router';

@Component({
  selector: 'app-accept-invitation',
  standalone: true,
  imports: [CommonModule,FormsModule],
  templateUrl: './accept-invitation.html',
  styleUrls: ['./accept-invitation.css'],
})
export class AcceptInvitation implements OnInit{
  token = '';
  isValid = false;
  isValidating = true;
  isSubmitting = false;
  prenom = '';
  nom = '';
  password = '';
  confirmPassword = '';
  error = '';

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private adminService: AdminService,
  ) {}
    ngOnInit(): void {
    this.token = this.route.snapshot.queryParams['token'];
    if (this.token) {
      this.adminService.validateInvitation(this.token).subscribe({
        next: (data) => {
          this.isValid = data.isValid;
          this.isValidating = false;
        },
        error: () => {
          this.isValid = false;
          this.isValidating = false;
        }
      });
    } else {
      this.isValid = false;
      this.isValidating = false;
    }
  }

  onSubmit(): void {
    if (this.password !== this.confirmPassword) {
      this.error = 'Les mots de passe ne correspondent pas';
      return;
    }
    
    if (this.password.length < 6) {
      this.error = 'Le mot de passe doit contenir au moins 6 caractères';
      return;
    }
    
    this.isSubmitting = true;
    
    this.adminService.acceptInvitation(this.token, this.password, this.nom, this.prenom).subscribe({
      next: () => {
        this.router.navigate(['/login']);
      },
      error: (err) => {
        this.error = err.error?.error || 'Erreur lors de l\'inscription';
        this.isSubmitting = false;
      }
    });
  }
}


