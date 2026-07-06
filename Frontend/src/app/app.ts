import { Component, signal } from '@angular/core';
import { RouterModule, RouterOutlet } from '@angular/router';
import { Sidebar } from './components/sidebar/sidebar';
import { CommonModule } from '@angular/common';
import { Header } from './components/header/header';
import { AuthService } from './services/auth.service';
import { User } from './models/user.model';
import { Toasts } from './components/toast/toast';
import { SettingsService } from './services/settings.service';
import { TranslationService } from './services/translation.service';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, RouterModule, Header, Sidebar, CommonModule, Toasts],
  templateUrl: './app.html',
  styleUrls: ['./app.css']
})
export class App {
  protected readonly title = signal('Frontend');

  isLoggedIn: boolean = false;
  currentUser: User | null = null;
  private languageSynced = false;

  constructor(
    private authService: AuthService,
    private settingsService: SettingsService,
    private translationService: TranslationService
  ) {}

  ngOnInit() {
    // 2. On met à jour la propriété locale en fonction du service
    this.authService.currentUser$.subscribe(user => {
      this.isLoggedIn = !!user; // Convertit l'objet user en booléen (true si existe)
      this.currentUser = user;

      // Applique la langue enregistrée côté serveur dès la connexion, pour que
      // la sidebar/le header soient dans la bonne langue même sans avoir visité
      // la page Paramètres sur cet appareil.
      if (this.isLoggedIn && !this.languageSynced) {
        this.languageSynced = true;
        this.settingsService.getSettings().subscribe({
          next: (data) => this.translationService.use(data?.language),
          error: () => {}
        });
      }
    });
  }

  logout() {
    this.authService.logout();
  }
}


