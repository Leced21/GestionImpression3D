import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { UserService } from '../../services/user.service';
import { SettingsService } from '../../services/settings.service';

@Component({
  selector: 'app-settings',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './settings.html',
  styleUrl: './settings.css',
})
export class Settings implements OnInit {
  activeTab = 'general';
  isAdmin = false;

  // Utilisateur
  user = {
    prenom: '',
    nom: '',
    email: '',
    role: ''
  };

  // Paramètres
  settings = {
    language: 'fr',
    timezone: 'Europe/Paris',
    dateFormat: 'DD/MM/YYYY',
    theme: 'light',
    primaryColor: '#3b82f6',
    emailNotifications: true,
    stockAlerts: true,
    productionAlerts: true,
    weeklyReports: false,
    twoFactorEnabled: false
  };

  // Sécurité
  security = {
    currentPassword: '',
    newPassword: '',
    confirmPassword: ''
  };

  // Système
  appVersion = '2.0.0';
  environment = 'Développement';
  dbStatus = '🟢 Connectée';
  diskSpace = '2.4 Go / 50 Go';

  constructor(
    private settingsService: SettingsService,
    private userService: UserService,
    private cdr: ChangeDetectorRef
  ) { }

  ngOnInit(): void {
    this.loadUser();
    this.loadSettings();
  }

  loadUser(): void {
    this.userService.getProfile().subscribe({
      next: (data) => {
        this.user = data;
        this.isAdmin = data.role === "Admin";
        this.cdr.detectChanges()
      },
      error: (err) => console.error(err)
    });
  }
  loadSettings(): void {
    this.settingsService.getSettings().subscribe({
      next: (data) => {
        this.settings = { ...this.settings, ...data };
        this.applyTheme();
        this.applyColor();
      },
      error: (err) => console.error(err)
    });
  }
  getInitials(): string {
    return `${this.user.prenom?.charAt(0) || ''}${this.user.nom?.charAt(0) || ''}`;
  }

  updateProfile(): void {
    this.userService.updateProfile(this.user).subscribe({
      next: () => alert('✅ Profil mis à jour'),
      error: (err) => console.error(err)
    });
  }

  changePassword(): void {
    if (this.security.newPassword !== this.security.confirmPassword) {
      alert('❌ Les mots de passe ne correspondent pas');
      return;
    }

    this.userService.changePassword({
      currentPassword: this.security.currentPassword,
      newPassword: this.security.newPassword
    }).subscribe({
      next: () => {
        alert('✅ Mot de passe changé avec succès');
        this.security = { currentPassword: '', newPassword: '', confirmPassword: '' };
      },
      error: (err) => console.error(err)
    });
  }

  toggle2FA(): void {
    this.settings.twoFactorEnabled = !this.settings.twoFactorEnabled;
    alert(`2FA ${this.settings.twoFactorEnabled ? 'activé' : 'désactivé'}`);
  }

  setTheme(theme: string): void {
    this.settings.theme = theme;
    this.applyTheme();
  }

  applyTheme(): void {
    const theme = this.settings.theme === 'system'
      ? window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light'
      : this.settings.theme;

    document.documentElement.setAttribute('data-theme', theme);
  }

  setPrimaryColor(color: string): void {
    this.settings.primaryColor = color;
    this.applyColor();
  }

  applyColor(): void {
    document.documentElement.style.setProperty('--primary-color', this.settings.primaryColor);
  }

  resetSettings(): void {
    if (confirm('Réinitialiser tous les paramètres ?')) {
      this.settings = {
        language: 'fr',
        timezone: 'Europe/Paris',
        dateFormat: 'DD/MM/YYYY',
        theme: 'light',
        primaryColor: '#3b82f6',
        emailNotifications: true,
        stockAlerts: true,
        productionAlerts: true,
        weeklyReports: false,
        twoFactorEnabled: false
      };
      this.applyTheme();
      this.applyColor();
      alert('✅ Paramètres réinitialisés');
    }
  }

  saveSettings(): void {
    this.settingsService.saveSettings(this.settings).subscribe({
      next: () => alert('✅ Paramètres sauvegardés'),
      error: (err) => console.error(err)
    });
  }

  // Actions système (Admin)
  clearCache(): void {
    if (confirm('Vider le cache ?')) {
      alert('🧹 Cache vidé avec succès');
    }
  }

  exportData(): void {
    alert('📤 Export des données en cours...');
  }

  resetApp(): void {
    if (confirm('⚠️ Réinitialiser complètement l\'application ? Cette action est irréversible !')) {
      alert('🔄 Réinitialisation en cours...');
    }
  }
}
