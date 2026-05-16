import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, OnDestroy, OnInit } from '@angular/core';
import { RouterModule } from '@angular/router';
import { Projet } from '../../models/projet.model';
import { ProjetService } from '../../services/projet.service';
import { Subject, takeUntil } from 'rxjs';

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './sidebar.html',
  styleUrl: './sidebar.css',
})
export class Sidebar implements OnInit, OnDestroy {
  recentProjets: Projet[] = [];
  private destroy$ = new Subject<void>();

  constructor(
    private projetService: ProjetService,
    private cdr: ChangeDetectorRef
  ) {}

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
