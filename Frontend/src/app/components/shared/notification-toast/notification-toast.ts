import { CommonModule } from '@angular/common';
import { Component, OnDestroy, OnInit } from '@angular/core';
import { NotificationService } from '../../../services/notification.service';
import { AppNotification } from '../../../models/notification.model';

@Component({
  selector: 'app-notification-toast',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './notification-toast.html',
  styleUrls: ['./notification-toast.css'],
})
export class NotificationToast implements OnInit, OnDestroy{
  notifications: AppNotification[] = [];
  private interval: any;

  constructor(
    private notificationService: NotificationService
  ) {}

  ngOnInit(): void {
    this.notificationService.onNotification((notification) => {
      this.notifications.unshift(notification);
      
      // Auto-suppression après 5 secondes
      setTimeout(() => {
        this.removeNotification(notification.id);
      }, 5000);
    });
  }

  markAsRead(id: string): void {
    this.notificationService.markAsRead(id);
  }

  removeNotification(id: string): void {
    this.notifications = this.notifications.filter(n => n.id !== id);
  }

  ngOnDestroy(): void {
    if (this.interval) clearInterval(this.interval);
  }
}


