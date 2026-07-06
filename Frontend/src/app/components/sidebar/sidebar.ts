import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, OnDestroy, OnInit } from '@angular/core';
import { RouterModule } from '@angular/router';
import { Projet } from '../../models/projet.model';
import { ProjetService } from '../../services/projet.service';
import { Subject, takeUntil } from 'rxjs';
import { AuthService } from '../../services/auth.service';
import { TranslatePipe } from '../../pipes/translate.pipe';

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [CommonModule, RouterModule, TranslatePipe],
  templateUrl: './sidebar.html',
  styleUrls: ['./sidebar.css'],
})
export class Sidebar implements OnInit, OnDestroy {
  recentProjets: Projet[] = [];
  isAdmin = false;
  canManageCommandes = false;
  private destroy$ = new Subject<void>();

  constructor(
    private projetService: ProjetService,
    private authService: AuthService,
    private cdr: ChangeDetectorRef
  ) {
    this.authService.currentUser$.subscribe(user => {
      const role = user?.role?.toLowerCase();
      this.isAdmin = role === 'admin';
      this.canManageCommandes = role === 'admin' || role === 'commercial';
    });
  }

  ngOnInit(): void {
    this.projetService.getAll().pipe(takeUntil(this.destroy$)).subscribe({
      next: (data) => {
        this.recentProjets = data.slice(0, 5);
        this.cdr.detectChanges();
      },
      error: (err) => console.error('Erreur chargement projets sidebar:', err)
    });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }
  
}


