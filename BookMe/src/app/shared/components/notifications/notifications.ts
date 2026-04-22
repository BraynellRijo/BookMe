import { Component, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { animate, style, transition, trigger } from '@angular/animations';
import { NotificationService } from '../../../core/services/notification.service';
import { NotificationDTO } from '../../../core/models/notification.model';

@Component({
  selector: 'app-notifications',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './notifications.html',
  animations: [
    trigger('slideFade', [
      transition(':enter', [
        style({ opacity: 0, transform: 'translateY(-10px)' }),
        animate('200ms ease-out', style({ opacity: 1, transform: 'translateY(0)' })),
      ]),
      transition(':leave', [
        animate('150ms ease-in', style({ opacity: 0, transform: 'translateY(-10px)' })),
      ]),
    ]),
  ]
})
export class NotificationsComponent {
  private notificationService = inject(NotificationService);
  
  isOpen = signal(false);
  notifications = this.notificationService.unreadNotifications;
  unreadCount = computed(() => this.notifications().length);

  constructor() {
    if (typeof document !== 'undefined') {
      document.addEventListener('click', () => this.close());
    }
  }

  toggle(event: Event) {
    event.stopPropagation();
    this.isOpen.update(v => !v);
  }

  close() {
    this.isOpen.set(false);
  }

  markAllAsRead(event?: Event) {
    if (event) event.stopPropagation();
    this.notificationService.markAllAsRead().subscribe();
  }

  markAsRead(id: string, event: Event) {
    event.stopPropagation();
    this.notificationService.markAsRead(id).subscribe();
  }

  deleteNotification(id: string, event: Event) {
    event.stopPropagation();
    this.notificationService.delete(id).subscribe();
  }

  formatDate(date: string | Date): string {
    const d = new Date(date);
    return d.toLocaleDateString() + ' ' + d.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
  }
}
