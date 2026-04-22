import { Component, AfterViewInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { AuthService } from '../../../../core/services/auth.service';

declare var AOS: any;

@Component({
  selector: 'app-host',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './host.component.html'
})
export class HostComponent implements AfterViewInit {
  private authService = inject(AuthService);
  private router = inject(Router);

  isSubmitting = false;

  ngAfterViewInit() {
    // We use a slightly longer timeout to ensure Angular has finished the render cycle
    setTimeout(() => {
      this.initAos();
    }, 500);
  }

  private initAos() {
    if (typeof AOS !== 'undefined') {
      AOS.init({
        duration: 1000,
        easing: 'ease-out-back',
        once: false,
        mirror: true,
        anchorPlacement: 'top-bottom',
      });
      AOS.refresh();
    }
  }

  startHosting() {
    // Si no está autenticado, mandarlo a login
    if (!this.authService.isAuthenticated()) {
      this.router.navigate(['/auth/login']);
      return;
    }

    // Si ya es host, simplemente llevarlo a crear la propiedad
    if (this.authService.isHost()) {
      this.router.navigate(['/host/create']);
      return;
    }

    // Si está autenticado pero no es host, llamar al endpoint becomeHost
    this.isSubmitting = true;
    this.authService.becomeHost().subscribe({
      next: () => {
        this.isSubmitting = false;
        // Una vez que es host y el token se actualizó, lo llevamos al wizard
        this.router.navigate(['/host/create']);
      },
      error: (err) => {
        console.error('Error al intentar ser host:', err);
        this.isSubmitting = false;
        // Opcional: mostrar un toast o mensaje al usuario aquí
      }
    });
  }
}
