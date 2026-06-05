import { Component, OnDestroy, OnInit } from '@angular/core';
import { User } from '../../models/user.model';
import { AuthService } from '../../services/auth.service';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { Subscription } from 'rxjs';
import { NotificationCenter } from '../notification-center/notification-center';
import { NotificationBell } from '../notification-bell/notification-bell';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [CommonModule, RouterModule, NotificationBell],
  templateUrl: './header.html',
  styleUrls: ['./header.css'],
})
export class Header implements OnInit ,OnDestroy{
  currentUser:User | null = null;
  menuOpen: boolean = false;
  private userSub!: Subscription;

  constructor(
    private authService: AuthService
  ) {}
  ngOnDestroy(): void {
    if (this.userSub) {
      this.userSub.unsubscribe();
    }
  }
  ngOnInit(): void {
    this.userSub=this.authService.currentUser$.subscribe(user => {
      this.currentUser = user;
    });
  }
    getUserInitials(): string {
    if (!this.currentUser) return '?';
    return `${this.currentUser.prenom?.charAt(0) || ''}${this.currentUser.nom?.charAt(0) || ''}`.toUpperCase();
  }

  toggleMenu(): void {
    this.menuOpen = !this.menuOpen;
  }

  logout(): void {
    this.authService.logout();
    this.menuOpen = false;
  }
}


