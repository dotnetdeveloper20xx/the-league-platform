import { Injectable } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { environment } from '../../../environments/environment';
import { AuthService } from './auth.service';

@Injectable({ providedIn: 'root' })
export class SignalRService {
  private notificationConnection: signalR.HubConnection | null = null;
  private matchConnection: signalR.HubConnection | null = null;

  constructor(private authService: AuthService) {}

  async connectNotifications(): Promise<void> {
    const token = this.authService.getAccessToken();
    if (!token) return;

    this.notificationConnection = new signalR.HubConnectionBuilder()
      .withUrl(`${environment.signalRUrl}/notifications`, { accessTokenFactory: () => token })
      .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
      .build();

    this.notificationConnection.on('ReceiveNotification', (type: string, payload: any) => {
      console.log('Notification received:', type, payload);
    });

    await this.notificationConnection.start();
  }

  async connectMatch(matchId: string): Promise<signalR.HubConnection> {
    this.matchConnection = new signalR.HubConnectionBuilder()
      .withUrl(`${environment.signalRUrl}/match-centre`)
      .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
      .build();

    await this.matchConnection.start();
    await this.matchConnection.invoke('JoinMatch', matchId);
    return this.matchConnection;
  }

  async disconnectMatch(matchId: string): Promise<void> {
    if (this.matchConnection) {
      await this.matchConnection.invoke('LeaveMatch', matchId);
      await this.matchConnection.stop();
      this.matchConnection = null;
    }
  }

  async disconnect(): Promise<void> {
    if (this.notificationConnection) {
      await this.notificationConnection.stop();
      this.notificationConnection = null;
    }
  }
}
