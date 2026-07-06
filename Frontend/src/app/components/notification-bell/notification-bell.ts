import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, HostListener, OnDestroy, OnInit } from '@angular/core';
import { RouterModule } from '@angular/router';
import { NotificationCenter } from '../notification-center/notification-center';
import { NotificationManagerService } from '../../services/notification-manager.service';

@Component({
  selector: 'app-notification-bell',
  standalone: true,
  imports: [CommonModule, RouterModule, NotificationCenter],
  templateUrl: './notification-bell.html',
  styleUrls: ['./notification-bell.css'],
})
export class NotificationBell implements OnInit, OnDestroy {
  unreadCount = 0;
  isOpen = false;
  private refreshIntervalId?: ReturnType<typeof setInterval>;

  constructor(
    private notificationService: NotificationManagerService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.loadUnreadCount();
    // Rafraîchir toutes les 30 secondes
    this.refreshIntervalId = setInterval(() => this.loadUnreadCount(), 30000);
  }

  ngOnDestroy(): void {
    if (this.refreshIntervalId) {
      clearInterval(this.refreshIntervalId);
    }
  }

  loadUnreadCount(): void {
    this.notificationService.getUnreadCount().subscribe({
      next: (data) => {
        this.unreadCount = data.count;
        this.cdr.detectChanges();
      },
      error: (err) => console.error(err)
    });
  }

  togglePanel(): void {
    this.isOpen = !this.isOpen;
    if (!this.isOpen) {
      this.loadUnreadCount();
    }
  }

  @HostListener('document:click')
  closePanel(): void {
    this.isOpen = false;
  }
}


