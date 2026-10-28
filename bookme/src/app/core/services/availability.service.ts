import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { BlockDatesDTO, BlockedDateRangeDTO } from '../models/availability.model';

@Injectable({
  providedIn: 'root'
})
export class AvailabilityService {
  private http = inject(HttpClient);
  private apiUrl = environment.apiUrl + '/Availability';

  /**
   * Bloquea manualmente un rango de fechas para un Listing.
   */
  blockDates(dto: BlockDatesDTO): Observable<BlockedDateRangeDTO[]> {
    return this.http.post<BlockedDateRangeDTO[]>(`${this.apiUrl}/Block-Dates`, dto).pipe(
      catchError(error => {
        console.error('CRITICAL: Error al intentar bloquear fechas:', error);
        return throwError(() => error);
      })
    );
  }

  /**
   * Desbloquea manualmente un rango de fechas o una regla específica.
   */
  unblockDates(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/Unblock-Dates/${id}`).pipe(
      catchError(error => {
        console.error(`CRITICAL: Error al intentar desbloquear fecha (${id}):`, error);
        return throwError(() => error);
      })
    );
  }

  /**
   * Obtiene todos los bloqueos (manuales y reservas) para un Listing específico.
   * Método público y crítico para el renderizado del calendario.
   */
  getListingBlocks(listingId: string): Observable<BlockedDateRangeDTO[]> {
    return this.http.get<BlockedDateRangeDTO[]>(`${this.apiUrl}/Listing-Blocks/${listingId}`).pipe(
      catchError(error => {
        console.error(`Error al recuperar bloques para el Listing (${listingId}):`, error);
        return throwError(() => error);
      })
    );
  }
}
