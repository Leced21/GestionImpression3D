import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Toast, ToastService } from '../../services/toast.service';

@Component({
  selector: 'app-toasts',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="toast-container" aria-live="polite" aria-atomic="true">
      <div *ngFor="let t of toasts" class="toast" [ngClass]="t.level" role="status">
        <span class="tone"></span>
        <div class="msg">{{ t.message }}</div>
        <button class="close" type="button" aria-label="Fermer" (click)="dismiss(t.id)">×</button>
      </div>
    </div>
  `,
  styles: [`
    .toast-container{position:fixed;right:1rem;top:1rem;z-index:2000;display:flex;flex-direction:column;gap:.625rem;max-width:min(420px,calc(100vw - 2rem))}
    .toast{width:100%;min-width:280px;padding:.85rem .75rem .85rem 1rem;border:1px solid #e2e8f0;border-radius:.5rem;background:#fff;color:#1e293b;box-shadow:0 12px 32px rgba(15,23,42,.16);display:grid;grid-template-columns:4px 1fr 28px;gap:.75rem;align-items:center;animation:toast-in .18s ease-out}
    .tone{width:4px;height:100%;min-height:32px;border-radius:999px;background:#64748b}
    .toast.info .tone{background:#3b82f6}
    .toast.success .tone{background:#10b981}
    .toast.warning .tone{background:#f59e0b}
    .toast.error .tone{background:#ef4444}
    .msg{font-size:.9rem;line-height:1.35}
    .close{width:28px;height:28px;border:none;border-radius:.375rem;background:transparent;color:#64748b;font-size:1.25rem;line-height:1;cursor:pointer}
    .close:hover{background:#f1f5f9;color:#0f172a}
    @keyframes toast-in{from{opacity:0;transform:translateY(-6px)}to{opacity:1;transform:translateY(0)}}
  `]
})
export class Toasts {
  toasts: Toast[] = [];

  constructor(private toast: ToastService) {
    this.toast.toasts$.subscribe((list) => this.toasts = list);
  }

  dismiss(id: string) {
    this.toast.dismiss(id);
  }
}
