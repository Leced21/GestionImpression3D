import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { RouterModule } from '@angular/router';
import { PrintProfileService } from '../../../services/print-profile.service';
import { PrintProfile } from '../../../models/print-profile.model';

@Component({
  selector: 'app-profile-list',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './profile-list.html',
  styleUrl: './profile-list.css',
})
export class ProfileList implements OnInit {
  profiles: PrintProfile[] = [];

  constructor(
    private printProfileService: PrintProfileService,
    private cdr: ChangeDetectorRef
  ) { }

  ngOnInit(): void {
    this.loadProfiles();
  }

  loadProfiles(): void {
    this.printProfileService.getAll().subscribe({
      next: (data) => {
        this.profiles = data;
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Erreur lors du chargement des profils d\'impression', err)
      }
    });
  }
  setDefault(profile: PrintProfile): void {
    this.printProfileService.setDefault(profile.id).subscribe({
      next: (updated) => {
        this.loadProfiles();
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Erreur lors de la définition du profil par défaut', err)
      }
    });
  }
  deleteProfile(id: number): void {
    if (confirm('Supprimer ?')) {
      this.printProfileService.delete(id).subscribe({
        next: () => {
          this.loadProfiles();
          this.cdr.detectChanges();
        },
        error: (err) => {
          console.error('Erreur lors de la suppression du profil d\'impression', err)
        }
      });
    }
  }
}

