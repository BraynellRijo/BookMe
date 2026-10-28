import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, throwError, of } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { HostListingDetailDTO, HostListingSummaryDTO, ListingDTO } from './host-listing.types';

// Default fallback coordinates: Santo Domingo Este
const FALLBACK_LAT = 18.48;
const FALLBACK_LNG = -69.93;
const FALLBACK_RADIUS = 20000; // Large radius for fallback (global)

@Injectable({
  providedIn: 'root'
})
export class ListingsService {
  private http = inject(HttpClient);
  private apiUrl = environment.apiUrl + '/Listings';

  /**
   * Obtiene los detalles completos de una propiedad pública por ID.
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
   * Obtiene todas las imágenes de una propiedad.
   * Endpoint: GET /api/Listings/{id}/images
   */
  getListingImages(id: string): Observable<{url: string, publicId: string}[]> {
    return this.http.get<{url: string, publicId: string}[]>(`${this.apiUrl}/${id}/images`).pipe(
      catchError(error => {
        console.error(`Error al obtener imágenes del listing (${id}):`, error);
        return throwError(() => error);
      })
    );
  }

  /**
   * Obtiene la imagen principal de una propiedad.
   * Endpoint: GET /api/Listings/{id}/main-image
   */
  getMainImage(id: string): Observable<string> {
    return this.http.get<{url: string}>(`${this.apiUrl}/${id}/main-image`).pipe(
      map(res => res.url),
      catchError(error => {
        console.error(`Error al obtener la imagen principal (${id}):`, error);
        return of('assets/images/placeholder.png'); 
      })
    );
  }

  /**
   * NO existe un GET /api/Listings genérico en el backend.
   * Usamos el endpoint /nearby con coordenadas de Santo Domingo y un radio amplio
   * como fallback universal para obtener propiedades.
   */
  getAllListings(): Observable<ListingDTO[]> {
    return this.getNearby(FALLBACK_LAT, FALLBACK_LNG, FALLBACK_RADIUS);
  }

  /**
   * Obtiene propiedades cercanas a una ubicación específica.
   * Endpoint: GET /api/Listings/nearby?lat=...&lng=...&radiusKm=...&type=...
   */
  getNearby(lat: number, lng: number, radiusKm: number = 100, type?: string): Observable<ListingDTO[]> {
    let url = `${this.apiUrl}/nearby?lat=${lat}&lng=${lng}&radiusKm=${radiusKm}`;
    if (type && type !== 'All') {
      url += `&type=${type}`;
    }
    return this.http.get<ListingDTO[]>(url).pipe(
      catchError(error => {
        console.error('Error al obtener listings cercanos:', error);
        return throwError(() => error);
      })
    );
  }

  /**
   * Filtra propiedades por tipo (PropertyType enum).
   * Endpoint: GET /api/Listings/Type/{type}
   */
  getByType(type: string): Observable<ListingDTO[]> {
    return this.http.get<ListingDTO[]>(`${this.apiUrl}/Type/${type}`).pipe(
      catchError(error => {
        console.error(`Error al obtener listings por tipo (${type}):`, error);
        return throwError(() => error);
      })
    );
  }

  /**
   * Busca propiedades por título.
   * Endpoint: GET /api/Listings/Search/{title}
   */
  searchByTitle(title: string): Observable<ListingDTO[]> {
    return this.http.get<ListingDTO[]>(`${this.apiUrl}/Search/${title}`).pipe(
      catchError(error => {
        console.error(`Error buscando listings por título (${title}):`, error);
        return throwError(() => error);
      })
    );
  }

  /**
   * Filtra propiedades por rango de precio por noche.
   * Endpoint: GET /api/Listings/Price/{min}/{max}
   */
  filterByPrice(minPrice: number, maxPrice: number): Observable<ListingDTO[]> {
    return this.http.get<ListingDTO[]>(`${this.apiUrl}/Price/${minPrice}/${maxPrice}`).pipe(
      catchError(error => {
        console.error('Error filtrando por precio:', error);
        return throwError(() => error);
      })
    );
  }

  /**
   * Filtra propiedades por capacidad de huéspedes.
   * Endpoint: GET /api/Listings/Capacity/{min}/{max}
   */
  filterByCapacity(minGuests: number, maxGuests: number): Observable<ListingDTO[]> {
    return this.http.get<ListingDTO[]>(`${this.apiUrl}/Capacity/${minGuests}/${maxGuests}`).pipe(
      catchError(error => {
        console.error('Error filtrando por capacidad:', error);
        return throwError(() => error);
      })
    );
  }

  /**
   * Filtra propiedades disponibles en un rango de fechas.
   * Endpoint: GET /api/Listings/Available-Dates?checkIn=...&checkOut=...
   */
  filterByDates(checkIn: string, checkOut: string): Observable<ListingDTO[]> {
    return this.http.get<ListingDTO[]>(
      `${this.apiUrl}/Available-Dates?checkIn=${checkIn}&checkOut=${checkOut}`
    ).pipe(
      catchError(error => {
        console.error('Error filtrando por fechas:', error);
        return throwError(() => error);
      })
    );
  }

  /**
   * Filtra propiedades por ciudad.
   * Endpoint: GET /api/Listings/City/{city}
   */
  filterByCity(city: string): Observable<ListingDTO[]> {
    return this.http.get<ListingDTO[]>(`${this.apiUrl}/City/${city}`).pipe(
      catchError(error => {
        console.error(`Error filtrando por ciudad (${city}):`, error);
        return throwError(() => error);
      })
    );
  }
}
