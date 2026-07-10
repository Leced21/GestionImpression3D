import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

const STORAGE_PREFIX = 'onboarding_seen_';

@Injectable({ providedIn: 'root' })
export class OnboardingService {
  private visibleSubject = new BehaviorSubject<boolean>(false);
  public visible$ = this.visibleSubject.asObservable();

  // Ne déclenche le tour automatiquement qu'une fois par utilisateur (par navigateur) :
  // à appeler quand on connaît l'utilisateur courant (ex. depuis App.ngOnInit sur
  // currentUser$), jamais en boucle sur chaque changement de route.
  showIfFirstVisit(userId: number): void {
    if (localStorage.getItem(STORAGE_PREFIX + userId)) return;
    this.visibleSubject.next(true);
  }

  start(): void {
    this.visibleSubject.next(true);
  }

  finish(userId: number): void {
    localStorage.setItem(STORAGE_PREFIX + userId, '1');
    this.visibleSubject.next(false);
  }

  dismiss(userId: number): void {
    this.finish(userId);
  }
}
