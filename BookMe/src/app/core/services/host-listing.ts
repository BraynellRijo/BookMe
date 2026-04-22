import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { 
  CreateListingDTO, 
  CreateListingResponseDTO,
  HostListingDetailDTO, 
  HostListingSummaryDTO,
  ListingUpdateDTO
} from './host-listing.types';

@Injectable({
  providedIn: 'root'
})
export class HostListingService {
  private http = inject(HttpClient);
  // Se define la variable de entorno base para este servicio
  private apiUrl = environment.apiUrl + '/HostListing';

  /**
   * Crea una nueva publicación en el servidor de forma plana sin imágenes.
   */
  createListing(data: CreateListingDTO): Observable<CreateListingResponseDTO> {
    return this.http.post<CreateListingResponseDTO>(this.apiUrl, data).pipe(
      catchError(error => {
        console.error('Error al crear listing (createListing):', error);
        return throwError(() => error);
      })
    );
  }

  /**
   * Sube imágenes hacia una propiedad existente.
   */
  uploadImages(listingId: string, images: File[]): Observable<any> {
    const formData = new FormData();
    images.forEach(file => {
      formData.append('images', file, file.name);
    });

    return this.http.post(`${this.apiUrl}/${listingId}/images`, formData).pipe(
      catchError(error => {
        console.error(`Error al subir imágenes para (${listingId}):`, error);
        return throwError(() => error);
      })
    );
  }

  /**
   * Obtiene la lista resumida de propiedades publicadas por el anfitrión.
   */
  getMyListings(): Observable<HostListingSummaryDTO[]> {
    return this.http.get<HostListingSummaryDTO[]>(`${this.apiUrl}/my-listings`).pipe(
      catchError(error => {
        console.error('Error al obtener listings (getMyListings):', error);
        return throwError(() => error);
      })
    );
  }

  /**
   * Obtiene la calificación media de las propiedades publicadas por el anfitrión.
   */
  getAverageRating(): Observable<{ overallRating: number }> {
    return this.http.get<{ overallRating: number }>(`${this.apiUrl}/average-rating`).pipe(
      catchError(error => {
        console.error('Error al obtener la calificación media (average-rating):', error);
        return throwError(() => error);
      })
    );
  }

  /**
   * Obtiene todos los detalles de una propiedad específica por ID.
   */
  getListingById(id: string): Observable<HostListingDetailDTO> {
    return this.http.get<HostListingDetailDTO>(`${this.apiUrl}/${id}`).pipe(
      catchError(error => {
        console.error(`Error al obtener listing por ID (${id}):`, error);
        return throwError(() => error);
      })
    );
  }

  /**
   * Actualiza los datos de una publicación existente en formato plano.
   */
  updateListing(id: string, data: ListingUpdateDTO): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}`, data).pipe(
      catchError(error => {
        console.error(`Error al actualizar listing (${id}):`, error);
        return throwError(() => error);
      })
    );
  }

  /**
   * Cambia la disponibilidad de una publicación.
   */
  toggleAvailability(id: string, isAvailable: boolean): Observable<void> {
    // El backend espera el booleano en el body de la petición
    return this.http.patch<void>(`${this.apiUrl}/${id}/availability`, isAvailable ).pipe(
      catchError(error => {
        console.error(`Error al cambiar la disponibilidad del listing (${id}):`, error);
        return throwError(() => error);
      })
    );
  }
}
