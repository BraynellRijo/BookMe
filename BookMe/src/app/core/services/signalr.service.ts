import { Injectable, inject, signal } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { environment } from '../../../environments/environment';
import { TokenService } from './token.service';
import { NotificationDTO } from '../models/notification.model';
import { Subject } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class SignalRService {
  private tokenService = inject(TokenService);
  private hubConnection: signalR.HubConnection | null = null;
  
  // Observable to notify other services about new incoming notifications
  private notificationReceivedSubject = new Subject<NotificationDTO>();
  notificationReceived$ = this.notificationReceivedSubject.asObservable();

  // Connection status signal
  isConnected = signal<boolean>(false);

  /**
   * Initializes and starts the SignalR Hub connection.
   */
  startConnection(): void {
    if (this.hubConnection?.state === signalR.HubConnectionState.Connected) return;

    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(`${environment.hubUrl}/notifications`, {
        accessTokenFactory: () => this.tokenService.getAccessToken() || ''
      })
      .withAutomaticReconnect()
      .build();

    this.hubConnection
      .start()
      .then(() => {
        this.isConnected.set(true);
        this.registerHandlers();
      })
      .catch(err => {
        console.error('❌ SignalR: Error while starting connection', err);
        this.isConnected.set(false);
      });

    this.hubConnection.onclose(() => {
      this.isConnected.set(false);
    });
    
    this.hubConnection.onreconnecting(() => {
      this.isConnected.set(false);
    });

    this.hubConnection.onreconnected(() => {
      this.isConnected.set(true);
    });
  }

  /**
   * Stops the Hub connection.
   */
  stopConnection(): void {
    if (this.hubConnection) {
      this.hubConnection.stop().then(() => {
        this.isConnected.set(false);
      });
    }
  }

  /**
   * Registers server-to-client method handlers.
   */
  private registerHandlers(): void {
    if (!this.hubConnection) return;

    this.hubConnection.on('ReceiveNotification', (notification: NotificationDTO) => {
      this.notificationReceivedSubject.next(notification);
    });
  }
}
