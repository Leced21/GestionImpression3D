import { Component, OnDestroy, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Subscription } from 'rxjs';
import { AuthService } from '../../services/auth.service';
import { OnboardingService } from '../../services/onboarding.service';

interface OnboardingStep {
  icon: 'welcome' | 'dashboard' | 'pieces' | 'devis' | 'production' | 'done';
  title: string;
  description: string;
}

@Component({
  selector: 'app-onboarding-tour',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './onboarding-tour.html',
  styleUrls: ['./onboarding-tour.css'],
})
export class OnboardingTour implements OnInit, OnDestroy {
  visible = false;
  currentIndex = 0;
  direction: 'next' | 'prev' = 'next';
  contentVisible = true;
  private userId: number | null = null;
  private sub = new Subscription();

  readonly steps: OnboardingStep[] = [
    {
      icon: 'welcome',
      title: 'Bienvenue sur 3D Inspire',
      description: 'Ton espace de gestion pour piloter tes impressions 3D de bout en bout : pièces, projets, devis et production.',
    },
    {
      icon: 'dashboard',
      title: 'Le tableau de bord',
      description: 'D\'un coup d\'œil : chiffre d\'affaires, pièces en production, imprimantes disponibles. Ton point de départ à chaque connexion.',
    },
    {
      icon: 'pieces',
      title: 'Pièces & projets',
      description: 'Crée tes pièces à partir de fichiers STL, regroupe-les en projets, et suis leur avancement du prototypage à la commercialisation.',
    },
    {
      icon: 'devis',
      title: 'Devis & facturation',
      description: 'Génère des devis pour tes clients ; une fois acceptés, l\'ordre de fabrication et la facture se créent automatiquement.',
    },
    {
      icon: 'production',
      title: 'Production & imprimantes',
      description: 'Planifie les impressions, suis l\'état de tes imprimantes et le stock de matériaux en temps réel.',
    },
    {
      icon: 'done',
      title: 'C\'est parti !',
      description: 'Tu peux relancer ce guide à tout moment via le bouton "?" en haut de l\'écran. Bonne impression !',
    },
  ];

  constructor(
    private authService: AuthService,
    private onboardingService: OnboardingService
  ) {}

  ngOnInit(): void {
    this.sub.add(
      this.authService.currentUser$.subscribe(user => {
        this.userId = user?.id ?? null;
        if (user) {
          this.onboardingService.showIfFirstVisit(user.id);
        }
      })
    );

    this.sub.add(
      this.onboardingService.visible$.subscribe(visible => {
        this.visible = visible;
        if (visible) {
          this.currentIndex = 0;
          this.direction = 'next';
        }
      })
    );
  }

  ngOnDestroy(): void {
    this.sub.unsubscribe();
  }

  get isLastStep(): boolean {
    return this.currentIndex === this.steps.length - 1;
  }

  next(): void {
    if (this.isLastStep) {
      this.finish();
      return;
    }
    this.goTo(this.currentIndex + 1);
  }

  previous(): void {
    if (this.currentIndex === 0) return;
    this.goTo(this.currentIndex - 1);
  }

  goTo(index: number): void {
    if (index === this.currentIndex) return;
    this.direction = index > this.currentIndex ? 'next' : 'prev';
    // Démonte puis remonte le contenu de l'étape pour relancer les animations
    // CSS (elles ne se rejouent pas automatiquement sur un simple changement
    // de texte/attribut, seulement quand le nœud DOM est recréé).
    this.contentVisible = false;
    setTimeout(() => {
      this.currentIndex = index;
      this.contentVisible = true;
    }, 20);
  }

  skip(): void {
    this.finish();
  }

  private finish(): void {
    if (this.userId != null) {
      this.onboardingService.finish(this.userId);
    }
  }
}
