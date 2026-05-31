import { Injectable, NgZone } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { AppNotification } from '../models/notification.model';

@Injectable({
  providedIn: 'root',
})
export class NotificationService {
  private hubConnection!: signalR.HubConnection;
  private notifications: AppNotification[] = [];
  private listeners: ((notification: AppNotification) => void)[] = [];

  constructor(private ngZone: NgZone) {}

  startConnection(): void {
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl('https://localhost:7000/notificationHub')
      .withAutomaticReconnect()
      .build();

    this.hubConnection.start()
      .then(() => console.log('SignalR connecté'))
      .catch(err => console.error('Erreur SignalR:', err));

    this.hubConnection.on('NotificationReceived', (data) => {
      this.ngZone.run(() => {
        const notification: AppNotification = {
          id: crypto.randomUUID(),
          type: data.type,
          title: data.title,
          message: data.message,
          timestamp: new Date(data.timestamp),
          isRead: false
        };
        this.notifications.unshift(notification);
        this.listeners.forEach(listener => listener(notification));
      });
    });

    this.hubConnection.on('PrintJobStarted', (data) => {
      console.log(`Job ${data.jobNumber} démarré`);
    });

    this.hubConnection.on('PrintJobCompleted', (data) => {
      console.log(`Job ${data.jobNumber} terminé`);
    });

    this.hubConnection.on('PrintJobFailed', (data) => {
      console.log(`Job ${data.jobNumber} échoué: ${data.reason}`);
    });

    this.hubConnection.on('LowStockAlert', (data) => {
      console.log(`Stock bas: ${data.name} (${data.quantity})`);
    });
  }

  onNotification(callback: (notification: AppNotification) => void): void {
    this.listeners.push(callback);
  }

  getNotifications(): AppNotification[] {
    return this.notifications;
  }

  getUnreadCount(): number {
    return this.notifications.filter(n => !n.isRead).length;
  }

  markAsRead(id: string): void {
    const notification = this.notifications.find(n => n.id === id);
    if (notification) {
      notification.isRead = true;
    }
  }

  markAllAsRead(): void {
    this.notifications.forEach(n => n.isRead = true);
  }

  clearAll(): void {
    this.notifications = [];
  }
}
