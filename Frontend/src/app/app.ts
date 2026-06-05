import { Component, signal } from '@angular/core';
import { RouterModule, RouterOutlet } from '@angular/router';
import { Sidebar } from './components/sidebar/sidebar';
import { CommonModule } from '@angular/common';
import { Header } from './components/header/header';
import { AuthService } from './services/auth.service';
import { User } from './models/user.model';
import { Toasts } from './components/toast/toast';

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

  constructor(private authService: AuthService) {}

  ngOnInit() {
    // 2. On met à jour la propriété locale en fonction du service
    this.authService.currentUser$.subscribe(user => {
      this.isLoggedIn = !!user; // Convertit l'objet user en booléen (true si existe)
      this.currentUser = user;
    });
  }

  logout() {
    this.authService.logout();
  }
}


