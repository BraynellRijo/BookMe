import { Component, inject, OnInit, CUSTOM_ELEMENTS_SCHEMA, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterModule, Router } from '@angular/router';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { FormBuilder, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { forkJoin, of, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';

import { TokenService } from '../../../../core/services/token.service';
import { ListingsService } from '../../../../core/services/listings.service';
import { ReviewService } from '../../../../core/services/review.service';
import { ReviewDTO } from '../../../../core/models/review.model';
import { AvailabilityService } from '../../../../core/services/availability.service';
import { signal } from '@angular/core';
import { HostListingDetailDTO } from '../../../../core/services/host-listing.types';
import { BlockedDateRangeDTO } from '../../../../core/models/availability.model';
import { DateRange, MatCalendarCellCssClasses } from '@angular/material/datepicker';
import { BookingService } from '../../../../core/services/booking.service';
import { ToastService } from '../../../../core/services/toast.service';
import { BookingCreationDTO } from '../../../../core/models/booking.model';
import { NavbarComponent } from '../../../../shared/components/navbar/navbar.component';


@Component({
  selector: 'app-property-detail',
  standalone: true,
  imports: [
    CommonModule, 
    RouterModule, 
    ReactiveFormsModule,
    MatCardModule, 
    MatButtonModule, 
    MatIconModule,
    MatDatepickerModule,
    MatNativeDateModule,
    NavbarComponent
  ],
  templateUrl: './property-detail.component.html',
  styleUrls: ['./property-detail.component.scss'],
  schemas: [CUSTOM_ELEMENTS_SCHEMA]
})
export class PropertyDetailComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private tokenService = inject(TokenService);
  private listingsService = inject(ListingsService);
  private reviewService = inject(ReviewService);
  private availabilityService = inject(AvailabilityService);
  private bookingService = inject(BookingService);
  private toastService = inject(ToastService);
  private fb = inject(FormBuilder);
  private cdr = inject(ChangeDetectorRef);

  isAuthenticated = this.tokenService.isAuthenticated;

  propertyId: string | null = null;
  
  isLoading = true;
  property: HostListingDetailDTO | null = null;
  overallRating: number = 0;
  reviews: ReviewDTO[] = [];
  
  blockedRanges = signal<BlockedDateRangeDTO[]>([]);
  isLoadingBlocks = signal<boolean>(false);
  today = this.getTodayAtMidnight();
  selectedDateRange = signal<DateRange<Date> | null>(null);

  private getTodayAtMidnight(): Date {
    const d = new Date();
    d.setHours(0, 0, 0, 0);
    return d;
  }

  bookingForm!: FormGroup;

  totalNights = 0;
  basePriceTotal = 0;
  finalTotal = 0;

  ngOnInit() {
    this.route.paramMap.subscribe(params => {
      const id = params.get('id');
      if (id) {
        this.propertyId = id;
        this.loadAllData(id);
      } else {
        console.warn('[PropertyDetail] No se encontró el ID en la ruta');
      }
    });

    this.initBookingForm();
  }

  initBookingForm() {
    this.bookingForm = this.fb.group({
      checkIn: [null],
      checkOut: [null],
      guests: [1]
    });

    // Recalculate cost when dates change
    this.bookingForm.valueChanges.subscribe(val => {
      this.calculateTotal(val.checkIn, val.checkOut);
    });
  }

  loadAllData(id: string) {
    this.isLoading = true;
    const startTime = performance.now();

    forkJoin({
      property: this.listingsService.getListingById(id).pipe(
        catchError(err => {
          console.error('CRITICAL: Error loading property details', err);
          return throwError(() => err); 
        })
      ),
      rating: this.reviewService.getListingRating(id).pipe(
        catchError(err => {
          console.warn('Non-critical: Error loading rating', err);
          return of({ overallRating: 0 });
        })
      ),
      reviews: this.reviewService.getListingReviews(id).pipe(
        catchError(err => {
          console.warn('Non-critical: Error loading reviews', err);
          return of([]);
        })
      ),
      blocks: this.availabilityService.getListingBlocks(id).pipe(
        catchError(err => {
          console.warn('Non-critical: Error loading availability blocks', err);
          return of([]); 
        })
      ),
      images: this.listingsService.getListingImages(id).pipe(
        catchError(err => {
          console.warn('Non-critical: Error loading images separately', err);
          return of([]);
        })
      )
    }).subscribe({
      next: (res) => {
        this.property = res.property;
        
        this.overallRating = 0;
        if (this.property && this.property.averageRating > 0) {
          this.overallRating = this.property.averageRating;
        } else if (res.rating && res.rating.overallRating > 0) {
          this.overallRating = res.rating.overallRating;
        }

        this.reviews = res.reviews || [];
        this.blockedRanges.set(res.blocks || []);
        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Final failure in loadAllData:', err);
        this.isLoading = false;
        this.cdr.detectChanges();

        if (!this.property) {
          alert('No pudimos cargar los detalles de este santuario.');
        }
      }
    });
  }

  calculateTotal(checkIn: Date | null, checkOut: Date | null, precalculatedDays?: number) {
    if (!checkIn || !this.property) {
      this.totalNights = 0;
      this.basePriceTotal = 0;
      this.finalTotal = 0;
      return;
    }

    if (precalculatedDays !== undefined) {
      this.totalNights = precalculatedDays > 0 ? precalculatedDays : 0;
    } else if (checkOut) {
      const diffTime = Math.abs(checkOut.getTime() - checkIn.getTime());
      this.totalNights = Math.ceil(diffTime / (1000 * 60 * 60 * 24)); 
    } else {
      this.totalNights = 0;
    }
    
    this.basePriceTotal = this.totalNights * (this.property.pricingRules?.pricePerNight || 0);
    const cleaningFee = this.property.pricingRules?.cleaningFee || 0;
    this.finalTotal = this.totalNights > 0 ? (this.basePriceTotal + cleaningFee) : 0;
  }

  reserve() {
    if (!this.isAuthenticated()) {
      this.toastService.show('Por favor inicia sesión para reservar.', 'info');
      this.router.navigate(['/auth/login']);
      return;
    }

    if (this.bookingForm.invalid || this.totalNights === 0 || !this.bookingForm.value.checkIn || !this.bookingForm.value.checkOut) {
      this.toastService.show('Por favor selecciona un rango de fechas válido.', 'warning');
      return;
    }

    const checkIn = this.bookingForm.value.checkIn;
    const checkOut = this.bookingForm.value.checkOut;

    // Normalizamos a UTC para evitar desfases de zona horaria en el backend
    const dto: BookingCreationDTO = {
      listingId: this.property?.id || '',
      checkInDate: new Date(Date.UTC(checkIn.getFullYear(), checkIn.getMonth(), checkIn.getDate())).toISOString(),
      checkOutDate: new Date(Date.UTC(checkOut.getFullYear(), checkOut.getMonth(), checkOut.getDate())).toISOString(),
      totalGuests: this.bookingForm.value.guests
    };

    this.isLoading = true;
    this.cdr.detectChanges();

    this.bookingService.createBooking(dto).subscribe({
      next: () => {
        this.isLoading = false;
        this.toastService.show('¡Reserva confirmada! Prepárate para tu viaje.', 'success');
        this.router.navigate(['/reservations']);
      },
      error: (err: any) => {
        this.isLoading = false;
        console.error('Error al reservar:', err);
        const errorMsg = err.error?.message || 'No se pudo completar la reserva. Revisa la disponibilidad.';
        this.toastService.show(errorMsg, 'error');
        this.cdr.detectChanges();
      }
    });
  }

  dateFilterFn = (date: Date | null): boolean => {
    if (!date) return true;
    const ranges = this.blockedRanges();
    const d = new Date(date);
    d.setHours(0,0,0,0);

    const isOccupied = ranges.some(range => {
      const start = new Date(range.startDate);
      const end = new Date(range.endDate);
      start.setHours(0,0,0,0);
      end.setHours(0,0,0,0);
      return d >= start && d <= end;
    });
    return !isOccupied;
  }

  dateClassFn = (date: Date): MatCalendarCellCssClasses => {
    const ranges = this.blockedRanges();
    const d = new Date(date);
    d.setHours(0,0,0,0);

    const isOccupied = ranges.some(range => {
      const start = new Date(range.startDate);
      const end = new Date(range.endDate);
      start.setHours(0,0,0,0);
      end.setHours(0,0,0,0);
      return d >= start && d <= end;
    });
    
    if (isOccupied) return 'is-blocked';
    return '';
  }

  onDateSelected(date: Date | null) {
    if (!date) return;

    let currentRange = this.selectedDateRange();
    if (!currentRange || (currentRange.start && currentRange.end)) {
      this.selectedDateRange.set(new DateRange<Date>(date, null));
      this.bookingForm.patchValue({ checkIn: date, checkOut: null });
    } else {
      const start = currentRange.start;
      if (!start || date < start) {
         this.selectedDateRange.set(new DateRange<Date>(date, null));
         this.bookingForm.patchValue({ checkIn: date, checkOut: null });
      } else {
          const hasOverlap = this.blockedRanges().some(range => {
            const blockStart = new Date(range.startDate);
            blockStart.setHours(0,0,0,0);
            return blockStart >= start && blockStart <= date;
         });

         if (hasOverlap) {
            this.selectedDateRange.set(new DateRange<Date>(date, null));
            this.bookingForm.patchValue({ checkIn: date, checkOut: null });
         } else {
            this.selectedDateRange.set(new DateRange<Date>(start, date));
            this.bookingForm.patchValue({ checkIn: start, checkOut: date });
         }
      }
    }
    this.cdr.detectChanges();
  }
}
