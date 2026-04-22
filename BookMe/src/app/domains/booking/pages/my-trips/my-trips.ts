import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { BookingService } from '../../../../core/services/booking.service';
import { ToastService } from '../../../../core/services/toast.service';
import { ReviewService, ReviewCreationDTO } from '../../../../core/services/review.service';
import { BookingDTO, BookingStatus } from '../../../../core/models/booking.model';

interface Trip {
  id: string;
  listingId: string;
  propertyName: string;
  location: string;
  checkIn: Date;
  checkOut: Date;
  imageUrl: string;
  status: 'upcoming' | 'past' | 'cancelled';
  totalPrice: number;
  hasReviewed?: boolean;
}

@Component({
  selector: 'app-my-trips',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './my-trips.html',
  styleUrls: ['./my-trips.css']
})
export class MyTripsComponent {
  private bookingService = inject(BookingService);
  private reviewService = inject(ReviewService);
  private toastService = inject(ToastService);

  trips: Trip[] = [];
  isLoading = true;

  activeTab: 'all' | 'upcoming' | 'past' | 'cancelled' = 'all';

  ngOnInit() {
    this.loadTrips();
  }

  loadTrips() {
    this.isLoading = true;
    this.bookingService.getGuestBookings().subscribe({
      next: (bookings: BookingDTO[]) => {
        this.trips = bookings.map((b: BookingDTO) => this.mapBookingToTrip(b));
        this.isLoading = false;
      },
      error: (err: any) => {
        console.error('Error loading trips:', err);
        this.toastService.show('No pudimos cargar tus viajes.', 'error');
        this.isLoading = false;
      }
    });
  }

  private mapBookingToTrip(booking: BookingDTO): Trip {
    const checkOutDate = new Date(booking.checkOutDate);
    const now = new Date();
    
    let status: 'upcoming' | 'past' | 'cancelled' = 'upcoming';
    
    if (booking.status === BookingStatus.Cancelled) {
      status = 'cancelled';
    } else if (booking.status === BookingStatus.Completed || checkOutDate < now) {
      status = 'past';
    } else {
      status = 'upcoming';
    }

    return {
      id: booking.id,
      listingId: booking.listingId,
      propertyName: booking.listingTitle || 'Propiedad sin nombre',
      location: booking.listingLocation || 'Ubicación no disponible',
      checkIn: new Date(booking.checkInDate),
      checkOut: checkOutDate,
      imageUrl: booking.listingImageUrl || 'assets/images/placeholder.png',
      status: status,
      totalPrice: booking.totalPrice,
      hasReviewed: false // Reset or check if we need to fetch this
    };
  }
  
  // Cancel Modal State
  showCancelModal = false;
  tripToCancel: Trip | null = null;

  // Review Modal State
  showReviewModal = false;
  tripToReview: Trip | null = null;
  reviewRating = 0;
  reviewComment = '';

  get filteredTrips(): Trip[] {
    if (this.activeTab === 'all') return this.trips;
    return this.trips.filter(t => t.status === this.activeTab);
  }

  setTab(tab: 'all' | 'upcoming' | 'past' | 'cancelled') {
    this.activeTab = tab;
  }

  openCancelModal(event: Event, trip: Trip) {
    event.stopPropagation();
    this.tripToCancel = trip;
    this.showCancelModal = true;
  }

  closeModal() {
    this.showCancelModal = false;
    this.tripToCancel = null;
  }

  confirmCancel() {
    if (!this.tripToCancel) return;

    this.bookingService.cancelBooking(this.tripToCancel.id).subscribe({
      next: () => {
        this.toastService.show('Reserva cancelada correctamente.', 'success');
        this.loadTrips(); // Refresh list
        this.closeModal();
      },
      error: (err: any) => {
        console.error('Error cancelling booking:', err);
        this.toastService.show('No se pudo cancelar la reserva.', 'error');
      }
    });
  }

  // --- Review Modal Logic ---

  openReviewModal(event: Event, trip: Trip) {
    event.stopPropagation();
    this.tripToReview = trip;
    this.reviewRating = 0;
    this.reviewComment = '';
    this.showReviewModal = true;
  }

  closeReviewModal() {
    this.showReviewModal = false;
    this.tripToReview = null;
  }

  setRating(stars: number) {
    this.reviewRating = stars;
  }

  updateReviewComment(event: Event) {
    this.reviewComment = (event.target as HTMLTextAreaElement).value;
  }

  submitReview() {
    if (!this.tripToReview || this.reviewRating === 0) {
      this.toastService.show('Por favor selecciona una calificación.', 'warning');
      return;
    }

    const dto: ReviewCreationDTO = {
      listingId: this.tripToReview.listingId,
      rating: this.reviewRating,
      comment: this.reviewComment
    };

    this.isLoading = true;

    this.reviewService.createReview(dto).subscribe({
      next: () => {
        this.isLoading = false;
        if (this.tripToReview) {
           this.tripToReview.hasReviewed = true;
        }
        this.toastService.show('¡Gracias por tu reseña! Tu opinión es valiosa.', 'success');
        this.closeReviewModal();
      },
      error: (err: any) => {
        this.isLoading = false;
        console.error('Error al enviar reseña:', err);
        const msg = err.error?.message || 'No pudimos registrar tu reseña en este momento.';
        this.toastService.show(msg, 'error');
      }
    });
  }
}
