import { Injectable, signal } from '@angular/core';

export interface ToastMessage {
  id: string;
  type: 'success' | 'error' | 'warning' | 'info';
  message: string;
  duration: number;
}

@Injectable({
  providedIn: 'root',
})
export class NotificationService {
  private _toasts = signal<ToastMessage[]>([]);
  readonly toasts = this._toasts.asReadonly();

  success(message: string, duration = 4000): void {
    this.show({ type: 'success', message, duration });
  }

  error(message: string, duration = 6000): void {
    this.show({ type: 'error', message, duration });
  }

  warning(message: string, duration = 5000): void {
    this.show({ type: 'warning', message, duration });
  }

  info(message: string, duration = 4000): void {
    this.show({ type: 'info', message, duration });
  }

  dismiss(id: string): void {
    this._toasts.update((toasts) => toasts.filter((t) => t.id !== id));
  }

  private show(toast: Omit<ToastMessage, 'id'>): void {
    const id = crypto.randomUUID();
    const newToast: ToastMessage = { ...toast, id };
    this._toasts.update((toasts) => [...toasts, newToast]);

    setTimeout(() => this.dismiss(id), toast.duration);
  }
}
