import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { trigger, transition, style, animate } from '@angular/animations';
import { NotificationService, AppNotification } from '../../../core/services/notification.service';
@Component({
  selector: 'app-notification-toast',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './notification-toast.component.html',
  styleUrl: './notification-toast.component.scss',
  animations: [
    trigger('slideIn', [
      transition(':enter', [
        style({ transform: 'translateX(110%)', opacity: 0 }),
        animate('300ms ease-out', style({ transform: 'translateX(0)', opacity: 1 }))
      ]),
      transition(':leave', [
        animate('200ms ease-in', style({ transform: 'translateX(110%)', opacity: 0 }))
      ])
    ])
  ]
})
export class NotificationToastComponent {
  readonly notificationService = inject(NotificationService);
  readonly notifications$ = this.notificationService.notifications$;
  getIcon(type: AppNotification['type']): string {
    return { success: '✅', info: 'ℹ️', warning: '⚠️' }[type];
  }
  remove(id: string): void {
    this.notificationService.removeNotification(id);
  }
}
