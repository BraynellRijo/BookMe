import { Component, inject, signal, computed, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { TokenService } from '../../../../core/services/token.service';
import { BookingService } from '../../../../core/services/booking.service';
import { BookingDTO } from '../../../../core/models/booking.model';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './profile.component.html'
})
export class ProfileComponent implements OnInit {
  private tokenService = inject(TokenService);
  private bookingService = inject(BookingService);
  
  currentUser = this.tokenService.currentUser;
  
  // Computed role hierarchy
  readonly mainRole = computed(() => {
    const roles = this.tokenService.getUserRoles().map(r => r.toLowerCase());
    if (roles.includes('admin')) return 'Administrador';
    if (roles.includes('host')) return 'Anfitrión';
    return 'Huésped';
  });

  // State
  activeTab = 'about'; 
  isLoadingTrips = signal(false);
  pastTrips = signal<BookingDTO[]>([]);

  ngOnInit() {
    this.loadPastTrips();
  }

  private loadPastTrips() {
    this.isLoadingTrips.set(true);
    this.bookingService.getGuestBookings().subscribe({
      next: (trips) => {
        // Filter or display all based on UI needs
        this.pastTrips.set(trips);
        this.isLoadingTrips.set(false);
      },
      error: (err) => {
        console.error('Error loading trips:', err);
        this.isLoadingTrips.set(false);
      }
    });
  }
}
