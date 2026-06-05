import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ToastService, Toast } from '../../services/toast.service';

@Component({
  selector: 'app-toasts',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="toast-container">
      <div *ngFor="let t of toasts" class="toast" [ngClass]="t.level">
        <div class="msg">{{t.message}}</div>
        <button class="close" (click)="dismiss(t.id)">×</button>
      </div>
    </div>
  `,
  styles: [`
    .toast-container{position:fixed;right:1rem;top:1rem;z-index:2000;display:flex;flex-direction:column;gap:.5rem}
    .toast{min-width:220px;padding:.75rem 1rem;border-radius:.5rem;color:#fff;box-shadow:0 2px 6px rgba(0,0,0,.12);display:flex;justify-content:space-between;align-items:center}
    .toast.info{background:#3b82f6}
    .toast.success{background:#10b981}
    .toast.warning{background:#f59e0b}
    .toast.error{background:#ef4444}
    .toast .close{background:transparent;border:none;color:inherit;font-size:1.25rem;line-height:1;cursor:pointer}
  `]
})
export class Toasts {
  toasts: Toast[] = [];

  constructor(private toast: ToastService) {
    this.toast.toasts$.subscribe(list => this.toasts = list);
  }

  dismiss(id: string) { this.toast.dismiss(id); }
}
