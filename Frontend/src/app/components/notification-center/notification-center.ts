import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { RouterModule } from '@angular/router';
import { AppNotification } from '../../models/app-notification.model';
import { NotificationManagerService } from '../../services/notification-manager.service';

@Component({
  selector: 'app-notification-center',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './notification-center.html',
  styleUrls: ['./notification-center.css'],
})
export class NotificationCenter implements OnInit {
  notifications: AppNotification[] = [];
  hasUnread: boolean= false;
  isLoading: boolean = false;

  constructor(
    private notificationService: NotificationManagerService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.loadNotifications();
  }

  loadNotifications(): void {
    this.isLoading = true;
    this.notificationService.getNotifications().subscribe({
      next: (data) => {
        this.notifications = data;
        this.hasUnread = data.some(n => !n.isRead);
        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error(err);
        this.isLoading = false;
      }
    });
  }

  markAsRead(id: number): void {
    this.notificationService.markAsRead(id).subscribe({
      next: () => this.loadNotifications()
    });
  }

  markAllAsRead(): void {
    this.notificationService.markAllAsRead().subscribe({
      next: () => this.loadNotifications()
    });
  }

  deleteNotification(id: number): void {
    this.notificationService.deleteNotification(id).subscribe({
      next: () => this.loadNotifications()
    });
  }

  deleteAll(): void {
    if (confirm('Supprimer toutes les notifications ?')) {
      this.notificationService.deleteAll().subscribe({
        next: () => this.loadNotifications()
      });
    }
  }

  openNotification(notification: AppNotification): void {
    if (!notification.isRead) {
      this.markAsRead(notification.id);
    }
    if (notification.link) {
      window.location.href = notification.link;
    }
  }
    getNotificationIcon(type: string): string {
    switch (type) {
      case 'success': return '✅';
      case 'error': return '❌';
      case 'warning': return '⚠️';
      default: return 'ℹ️';
    }
  }

  formatDate(date: Date): string {
    const now = new Date();
    const notifDate = new Date(date);
    const diffMs = now.getTime() - notifDate.getTime();
    const diffMins = Math.floor(diffMs / 60000);
    const diffHours = Math.floor(diffMs / 3600000);
    const diffDays = Math.floor(diffMs / 86400000);

    if (diffMins < 1) return 'à l\'instant';
    if (diffMins < 60) return `il y a ${diffMins} min`;
    if (diffHours < 24) return `il y a ${diffHours} h`;
    return `il y a ${diffDays} j`;
  }
}


