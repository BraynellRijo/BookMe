import { Injectable, inject, signal, effect } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { NotificationDTO } from '../models/notification.model';
import { SignalRService } from './signalr.service';
import { ToastService } from './toast.service';
import { TokenService } from './token.service';

@Injectable({
  providedIn: 'root'
})
export class NotificationService {
  private http = inject(HttpClient);
  private signalRService = inject(SignalRService);
  private toastService = inject(ToastService);
  private tokenService = inject(TokenService);

  private readonly baseUrl = `${environment.apiUrl}/Notifications`;

  // Global state for unread notifications
  private readonly _unreadNotifications = signal<NotificationDTO[]>([]);
  readonly unreadNotifications = this._unreadNotifications.asReadonly();

  constructor() {
    // Listen to real-time notifications
    this.signalRService.notificationReceived$.subscribe(notification => {
      this.handleIncomingNotification(notification);
    });

    // Automatically manage connection based on auth status
    effect(() => {
      if (this.tokenService.isAuthenticated()) {
        this.signalRService.startConnection();
        this.loadUnread();
      } else {
        this.signalRService.stopConnection();
        this._unreadNotifications.set([]);
      }
    });
  }

  /**
   * Fetch unread notifications from the server.
   */
  loadUnread(): void {
    this.http.get<NotificationDTO[]>(`${this.baseUrl}/Unread`).subscribe({
      next: (notifications) => {
        this._unreadNotifications.set(notifications);
      },
      error: (err) => console.error('Error loading unread notifications', err)
    });
  }

  /**
   * Fetch all notifications (read and unread).
   */
  getAll(): Observable<NotificationDTO[]> {
    return this.http.get<NotificationDTO[]>(`${this.baseUrl}/All`);
  }

  /**
   * Mark a specific notification as read.
   */
  markAsRead(id: string): Observable<void> {
    return this.http.patch<void>(`${this.baseUrl}/MarkAsRead/${id}`, {}).pipe(
      tap(() => {
        // Optimistic update: remove from unread list
        this._unreadNotifications.update(list => list.filter(n => n.id !== id));
      })
    );
  }

  /**
   * Mark all notifications as read for the current user.
   */
  markAllAsRead(): Observable<void> {
    return this.http.patch<void>(`${this.baseUrl}/MarkAllAsRead`, {}).pipe(
      tap(() => {
        this._unreadNotifications.set([]);
      })
    );
  }

  /**
   * Delete a notification.
   */
  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`).pipe(
      tap(() => {
        this._unreadNotifications.update(list => list.filter(n => n.id !== id));
      })
    );
  }

  /**
   * Logic to execute when a real-time notification arrives.
   */
  private handleIncomingNotification(notification: NotificationDTO): void {
    // Prepend to the unread list
    this._unreadNotifications.update(list => [notification, ...list]);

    // Show a generic toast alert (Combining Title and Message as ToastService only takes one message string)
    const toastMessage = `${notification.title}: ${notification.message}`;
    this.toastService.info(toastMessage);
  }
}
