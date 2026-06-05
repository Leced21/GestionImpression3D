export interface AppNotification {
  id: number;
  userId: number;
  type: 'info' | 'success' | 'warning' | 'error';
  title: string;
  message: string;
  link?: string;
  referenceId?: number;
  referenceType?: string;
  isRead: boolean;
  createdAt: Date;
  readAt?: Date;
}
