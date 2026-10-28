import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { BookingCreationDTO, BookingDTO } from '../models/booking.model';
import { ReviewDTO } from '../models/review.model';

@Injectable({
  providedIn: 'root'
})
export class BookingService {
  private http = inject(HttpClient);
  private apiUrl = environment.apiUrl + '/GuestBooking';

  /**
   * Crea una nueva reserva para un Listing.
   * Endpoint: POST /api/GuestBooking/Booking-Creation
   */
  createBooking(dto: BookingCreationDTO): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/Booking-Creation`, dto).pipe(
      catchError(error => {
        console.error('Error al intentar realizar la reserva:', error);
        return throwError(() => error);
      })
    );
  }

  /**
   * Obtiene la lista de reservas realizadas por el huésped autenticado.
   * Endpoint: GET /api/GuestBooking/Guest-Bookings
   */
  getGuestBookings(): Observable<BookingDTO[]> {
    return this.http.get<BookingDTO[]>(`${this.apiUrl}/Guest-Bookings`).pipe(
      catchError(error => {
        console.error('Error al obtener las reservas del huésped:', error);
        return throwError(() => error);
      })
    );
  }

  /**
   * Cancela una reserva existente.
   * Endpoint: DELETE /api/GuestBooking/Cancel-Booking/{bookingId}
   */
  cancelBooking(bookingId: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/Cancel-Booking/${bookingId}`).pipe(
      catchError(error => {
        console.error(`Error al intentar cancelar la reserva (${bookingId}):`, error);
        return throwError(() => error);
      })
    );
  }

}
