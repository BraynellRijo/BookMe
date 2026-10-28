import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { animate, style, transition, trigger, query, stagger } from '@angular/animations';
import { ToastService, ToastMessage } from '../../../core/services/toast.service';

@Component({
  selector: 'app-toast',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './toast.component.html',
  styleUrls: ['./toast.component.css'],
  animations: [
    trigger('toastAnimation', [
      transition('* => *', [
        query(':enter', [
          style({ opacity: 0, transform: 'translateX(100%) scale(0.9)' }),
          stagger(100, [
            animate('400ms cubic-bezier(0.175, 0.885, 0.32, 1.275)',
              style({ opacity: 1, transform: 'translateX(0) scale(1)' }))
          ])
        ], { optional: true }),
        query(':leave', [
          animate('300ms cubic-bezier(0.4, 0, 1, 1)',
            style({ opacity: 0, transform: 'translateX(100%) scale(0.9)', height: 0, margin: 0, padding: 0 }))
        ], { optional: true })
      ])
    ])
  ]
})
export class ToastComponent {
  toastService = inject(ToastService);

  getIcon(type: string): string {
    switch (type) {
      case 'success': return 'check_circle';
      case 'error': return 'error';
      case 'warning': return 'warning';
      default: return 'info';
    }
  }

  remove(id: number) {
    this.toastService.remove(id);
  }

  trackById(index: number, toast: ToastMessage): number {
    return toast.id;
  }
}
