import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { HostListingService } from '../../../../core/services/host-listing';
import { AvailabilityService } from '../../../../core/services/availability.service';
import { ToastService } from '../../../../core/services/toast.service';
import { BlockedDateRangeDTO } from '../../../../core/models/availability.model';
import { MatDatepickerModule, DateRange, MatCalendarCellCssClasses } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { FormsModule } from '@angular/forms';

interface HostStats {
  totalRevenue: number;
  activeBookings: number;
  avgRating: number;
  totalProperties: number;
}

interface PropertyPreview {
  id: string;
  title: string;
  location: string;
  price: number;
  isActive: boolean;
  imageUrl: string;
}

@Component({
  selector: 'app-host-dashboard',
  standalone: true,
  imports: [
    CommonModule, 
    RouterLink, 
    CommonModule, 
    RouterLink, 
    MatDatepickerModule,
    MatNativeDateModule,
    FormsModule
  ],
  templateUrl: './host-dashboard.html',
  styleUrls: ['./host-dashboard.css']
})
export class HostDashboardComponent implements OnInit {
  private hostListingService = inject(HostListingService);
  private availabilityService = inject(AvailabilityService);
  private toastService = inject(ToastService);

  blockingPropertyId = signal<string | null>(null);
  isLoadingBlocks = signal<boolean>(false);
  blockReason = signal<string>('');

  blockedRanges = signal<BlockedDateRangeDTO[]>([]);
  selectedDateRange = signal<DateRange<Date> | null>(null);

  stats = signal<HostStats>({
    totalRevenue: 0,
    activeBookings: 0,
    avgRating: 0,
    totalProperties: 0
  });

  properties = signal<PropertyPreview[]>([]);
  isLoading = signal<boolean>(false);
  today = this.getTodayAtMidnight();

  private getTodayAtMidnight(): Date {
    const d = new Date();
    d.setHours(0, 0, 0, 0);
    return d;
  }

  ngOnInit() {
    this.loadProperties();
    this.loadAverageRating();
  }

  loadAverageRating() {
    this.hostListingService.getAverageRating().subscribe({
      next: (res) => {
        if (res && res.overallRating !== undefined) {
          this.stats.update(s => ({
            ...s,
            avgRating: Number(res.overallRating.toFixed(1))
          }));
        }
      },
      error: (err) => console.error('Error fetching average rating:', err)
    });
  }

  loadProperties() {
    this.isLoading.set(true);
    this.hostListingService.getMyListings().subscribe({
      next: (response: any) => {
        // Handle direct array or wrapped data (e.g. { data: [...] } or { value: [...] })
        const rawData = Array.isArray(response) ? response : (response?.data || response?.value || []);
        
        if (Array.isArray(rawData)) {
          const mappedProperties = rawData.map(p => {
            // Helper to handle both PascalCase and camelCase from backend DTO
            const getVal = (obj: any, key: string) => obj[key] ?? obj[key.charAt(0).toUpperCase() + key.slice(1)];
            
            const location = getVal(p, 'location');
            const city = location ? getVal(location, 'city') : null;
            const country = location ? getVal(location, 'country') : null;
            
            const pricing = getVal(p, 'pricingRules');
            const price = pricing ? getVal(pricing, 'pricePerNight') : 0;
            
            const images = getVal(p, 'images') || [];
            const firstImage = images.length > 0 ? getVal(images[0], 'url') : null;

            return {
              id: getVal(p, 'id'),
              title: getVal(p, 'title') || 'Sin Título',
              location: (city && country) 
                ? `${city}, ${country}` 
                : 'Ubicación no especificada',
              price: price,
              isActive: getVal(p, 'isAvailable') ?? true,
              imageUrl: firstImage || 'https://images.unsplash.com/photo-1512917774080-9991f1c4c750?auto=format&fit=crop&q=80&w=800'
            };
          });

          this.properties.set(mappedProperties);
          
          this.stats.update(s => ({
            ...s,
            totalProperties: mappedProperties.length
          }));
        }
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error('Error fetching properties:', err);
        this.isLoading.set(false);
        this.properties.set([]);
      }
    });
  }

  refreshListings() {
    this.loadProperties();
  }

  // Actions
  toggleStatus(prop: PropertyPreview) {
    const newStatus = !prop.isActive;
    // Optimistic UI update
    prop.isActive = newStatus;
    
    this.hostListingService.toggleAvailability(prop.id, newStatus).subscribe({
      error: (err) => {
        console.error('Error toggling status:', err);
        prop.isActive = !newStatus; // Revert on error
      }
    });
  }

  // Flow for Blocking Dates
  openBlockDates(id: string) {
    this.blockingPropertyId.set(id);
    this.selectedDateRange.set(null);
    this.isLoadingBlocks.set(true); // Start loading, will trigger ngIf to hide/detach calendar
    this.loadBlockedDates(id);
  }

  cancelBlockDates() {
    this.blockingPropertyId.set(null);
    this.blockedRanges.set([]);
    this.selectedDateRange.set(null);
    this.blockReason.set('');
  }

  loadBlockedDates(id: string) {
    this.availabilityService.getListingBlocks(id).subscribe({
      next: (ranges) => {
        this.blockedRanges.set(ranges);
        // Delay slightly to ensure the detach/attach cycle of ngIf works for re-render
        setTimeout(() => this.isLoadingBlocks.set(false), 50);
      },
      error: (err) => {
        console.error('Error fetching blocks:', err);
        this.isLoadingBlocks.set(false);
      }
    });
  }

  dateFilterFn = (date: Date | null): boolean => {
    if (!date) return true;
    const ranges = this.blockedRanges();
    
    // Normalize date for comparison
    const d = new Date(date);
    d.setHours(0,0,0,0);
    
    const isOccupied = ranges.some(range => {
      const start = new Date(range.startDate);
      const end = new Date(range.endDate);
      start.setHours(0,0,0,0);
      end.setHours(0,0,0,0);
      return d >= start && d <= end;
    });
    return !isOccupied; // Return false to disable
  }

  dateClassFn = (date: Date): MatCalendarCellCssClasses => {
    const ranges = this.blockedRanges();
    const isOccupied = ranges.some(range => {
      const start = new Date(range.startDate);
      const end = new Date(range.endDate);
      start.setHours(0,0,0,0);
      end.setHours(23,59,59,999);
      return date >= start && date <= end;
    });
    
    if (isOccupied) {
      return 'is-blocked';
    }
    return '';
  }

  onDateSelected(date: Date | null) {
    if (!date) return;

    let currentRange = this.selectedDateRange();
    if (!currentRange || (currentRange.start && currentRange.end)) {
      this.selectedDateRange.set(new DateRange<Date>(date, null));
    } else {
      const start = currentRange.start;
      if (!start || date < start) {
         this.selectedDateRange.set(new DateRange<Date>(date, null));
      } else {
         // ensure no overlap with blocked ranges
         const hasOverlap = this.blockedRanges().some(range => {
            const blockStart = new Date(range.startDate);
            const blockEnd = new Date(range.endDate);
            blockStart.setHours(0,0,0,0);
            blockEnd.setHours(23,59,59,999);
            // Overlaps if block start is between selection start and end
            return blockStart >= start && blockStart <= date;
         });
         
         if (hasOverlap) {
            this.toastService.error('El rango seleccionado incluye fechas que ya están ocupadas o bloqueadas.');
            this.selectedDateRange.set(new DateRange<Date>(date, null));
         } else {
            this.selectedDateRange.set(new DateRange<Date>(start, date));
         }
      }
    }
  }

  saveAvailability() {
    const range = this.selectedDateRange();
    const propId = this.blockingPropertyId();
    if (!propId || !range || !range.start || !range.end) {
      this.toastService.warning('Selecciona un rango de fechas válido primero.');
      return;
    }
    
    this.availabilityService.blockDates({
      listingId: propId,
      startDate: range.start.toISOString(),
      endDate: range.end.toISOString(),
      reason: this.blockReason() || 'Bloqueo manual del anfitrión'
    }).subscribe({
      next: () => {
        this.toastService.success('Fechas bloqueadas exitosamente.');
        this.selectedDateRange.set(null);
        this.blockReason.set('');
        this.loadBlockedDates(propId); // refresh
      },
      error: (err) => {
        console.error('Error saving availability', err);
        this.toastService.error('Error al bloquear fechas.');
      }
    });
  }

  onEditSaved() {
    this.loadProperties();
  }
}
