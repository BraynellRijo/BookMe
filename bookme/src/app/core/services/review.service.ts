import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { environment } from '../../../environments/environment';

export interface ReviewDTO {
  id: string;
  listingId: string;
  guestId?: string;
  guestName?: string;
  guestPhotoUrl?: string;
  rating: number;
  comment: string;
  createdAt: string;
}

export interface ReviewCreationDTO {
  listingId: string;
  rating: number;
  comment: string;
}

@Injectable({
  providedIn: 'root'
})
export class ReviewService {
  private http = inject(HttpClient);
  private apiUrl = environment.apiUrl + '/Reviews';

  /**
   * Obtiene la calificación promedio de una propiedad
   */
  getListingRating(listingId: string): Observable<{ overallRating: number }> {
    return this.http.get<{ overallRating: number }>(`${this.apiUrl}/Listing-Rating/${listingId}`).pipe(
      catchError(error => {
        console.error(`[ReviewService] ❌ Error al obtener rating (${listingId}):`, error);
        return throwError(() => error);
      })
    );
  }

  /**
   * Obtiene todas las reseñas de una propiedad específica
   */
  getListingReviews(listingId: string): Observable<ReviewDTO[]> {
    return this.http.get<ReviewDTO[]>(`${this.apiUrl}/Listing-Reviews/${listingId}`).pipe(
      catchError(error => {
        console.error(`[ReviewService] ❌ Error al obtener reseñas (${listingId}):`, error);
        return throwError(() => error);
      })
    );
  }

  /**
   * Crea una nueva reseña para una propiedad.
   */
  createReview(dto: ReviewCreationDTO): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/Create-Review`, dto).pipe(
      catchError(error => {
        console.error('Error al enviar la reseña:', error);
        return throwError(() => error);
      })
    );
  }
}
