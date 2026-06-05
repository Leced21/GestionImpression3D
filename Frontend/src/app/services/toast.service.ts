import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

export type ToastLevel = 'success' | 'info' | 'warning' | 'error';

export interface Toast {
  id: string;
  level: ToastLevel;
  message: string;
  timeout?: number;
}

@Injectable({ providedIn: 'root' })
export class ToastService {
  private toastsSubject = new BehaviorSubject<Toast[]>([]);
  public toasts$ = this.toastsSubject.asObservable();

  show(level: ToastLevel, message: string, timeout = 5000) {
    const id = Math.random().toString(36).slice(2, 9);
    const t: Toast = { id, level, message, timeout };
    const list = [...this.toastsSubject.value, t];
    this.toastsSubject.next(list);
    if (timeout > 0) {
      setTimeout(() => this.dismiss(id), timeout);
    }
  }

  success(message: string, timeout = 5000) { this.show('success', message, timeout); }
  info(message: string, timeout = 4000) { this.show('info', message, timeout); }
  warn(message: string, timeout = 5000) { this.show('warning', message, timeout); }
  error(message: string, timeout = 8000) { this.show('error', message, timeout); }

  dismiss(id: string) {
    const list = this.toastsSubject.value.filter(t => t.id !== id);
    this.toastsSubject.next(list);
  }
}
