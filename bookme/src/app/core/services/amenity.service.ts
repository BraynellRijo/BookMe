import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, shareReplay, catchError, throwError } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AmenityDTO } from './host-listing.types';

@Injectable({
  providedIn: 'root'
})
export class AmenityService {
  private http = inject(HttpClient);
  private apiUrl = environment.apiUrl + '/Amenities';

  // Cache the amenities list — it's a catalog that rarely changes
  private amenities$?: Observable<AmenityDTO[]>;

  /**
   * Fetches the full amenity catalog from the backend.
   * Results are cached via shareReplay to avoid redundant API calls.
   */
  getAll(): Observable<AmenityDTO[]> {
    if (!this.amenities$) {
      this.amenities$ = this.http.get<AmenityDTO[]>(this.apiUrl).pipe(
        shareReplay(1),
        catchError(error => {
          console.error('Error fetching amenities catalog:', error);
          this.amenities$ = undefined; // Clear cache on error so it retries
          return throwError(() => error);
        })
      );
    }
    return this.amenities$;
  }
}
