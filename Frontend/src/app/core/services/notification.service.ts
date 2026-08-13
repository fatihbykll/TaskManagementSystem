import { Injectable, inject } from '@angular/core';
import { Router } from '@angular/router';
import * as signalR from '@microsoft/signalr';
import { BehaviorSubject } from 'rxjs';
export interface AppNotification {
  id: string;
  message: string;
  type: 'success' | 'info' | 'warning';
  timestamp: Date;
}
@Injectable({ providedIn: 'root' })
export class NotificationService {
  private hubConnection: signalR.HubConnection | null = null;
  // Bileşenler bu stream'i dinleyerek anlık bildirimleri alır
  private notificationsSubject = new BehaviorSubject<AppNotification[]>([]);
  notifications$ = this.notificationsSubject.asObservable();
  startConnection(token: string): void {
    if (this.hubConnection?.state === signalR.HubConnectionState.Connected) return;
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl('/hubs/notifications', {
        accessTokenFactory: () => token
      })
      .withAutomaticReconnect()
      .build();
    // Backend'den gelen 'TaskCreated', 'TaskUpdated' gibi olayları dinle
    this.hubConnection.on('TaskNotification', (payload: { message: string; type: 'success' | 'info' | 'warning' }) => {
      this.addNotification(payload.message, payload.type);
    });
    this.hubConnection
      .start()
      .catch(err => console.error('SignalR bağlantısı kurulamadı:', err));
  }
  stopConnection(): void {
    this.hubConnection?.stop();
  }
  addNotification(message: string, type: 'success' | 'info' | 'warning' = 'info'): void {
    const notification: AppNotification = {
      id: crypto.randomUUID(),
      message,
      type,
      timestamp: new Date()
    };
    const current = this.notificationsSubject.getValue();
    // En fazla 5 bildirim göster
    this.notificationsSubject.next([notification, ...current].slice(0, 5));
  }
  removeNotification(id: string): void {
    const current = this.notificationsSubject.getValue();
    this.notificationsSubject.next(current.filter(n => n.id !== id));
  }
  clearAll(): void {
    this.notificationsSubject.next([]);
  }
}
