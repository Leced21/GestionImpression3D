import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { PrintProfile } from '../../../models/print-profile.model';
import { PrintProfileService } from '../../../services/print-profile.service';

@Component({
  selector: 'app-profile-detail',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './profile-detail.html',
  styleUrl: './profile-detail.css',
})
export class ProfileDetail implements OnInit{
  profile: PrintProfile | null = null;
  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private profileService: PrintProfileService,
    private cdr: ChangeDetectorRef
  ) { }
  ngOnInit(): void {
    const id = this.route.snapshot.params['id'];
    if (id) this.loadProfile(id);
  }
  loadProfile(id: number): void {
    this.profileService.getById(id).subscribe({
      next: (data) => {
        this.profile = data;
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Erreur lors du chargement du profil d\'impression', err)
      }
    });
  }
  setDefault(): void {
    this.profileService.setDefault(this.profile!.id).subscribe({
      next: (updated) => {
        this.loadProfile(this.profile!.id);
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Erreur lors de la définition du profil par défaut', err)
      }
    });
  }
  deleteProfile(): void {
    if (confirm('Supprimer ?')) {
      this.profileService.delete(this.profile!.id).subscribe({
        next: () => {
          this.router.navigate(['/profiles']);
          this.cdr.detectChanges();
        },
        error: (err) => {
          console.error('Erreur lors de la suppression du profil d\'impression', err)
        }
      });
    }
  }
}
