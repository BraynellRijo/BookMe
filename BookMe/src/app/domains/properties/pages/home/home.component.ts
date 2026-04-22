import { Component, OnInit, inject, CUSTOM_ELEMENTS_SCHEMA, signal, ViewChild, ElementRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { ListingsService } from '../../../../core/services/listings.service';
import { GeolocationService, GeoLocation } from '../../../../core/services/geolocation.service';
import { ListingDTO } from '../../../../core/services/host-listing.types';
import { CategoryFilterComponent } from '../../../../shared/components/category-filter/category-filter';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, RouterModule, CategoryFilterComponent],
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.scss'],
  schemas: [CUSTOM_ELEMENTS_SCHEMA]
})
export class HomeComponent implements OnInit {
  private router = inject(Router);
  private listingsService = inject(ListingsService);
  private geoService = inject(GeolocationService);
  
  nearbyListings = signal<ListingDTO[]>([]);
  inspirationProperties = signal<ListingDTO[]>([]);
  featuredProperty = signal<ListingDTO | null>(null);
  
  isLoadingNearby = signal<boolean>(false);
  isLoadingInspiration = signal<boolean>(false);
  isLoadingFeatured = signal<boolean>(false);
  
  userLocation = signal<GeoLocation | null>(null);
  noGpsAccess = signal<boolean>(false);
  activeCategoryId = signal<string>('All');
  
  @ViewChild('nearbyContainer') nearbyContainer!: ElementRef<HTMLDivElement>;

  ngOnInit(): void {
    this.isLoadingNearby.set(true);
    this.loadInspirationProperties();

    this.geoService.getCurrentPosition()
      .then(coords => {
        this.userLocation.set(coords);
        this.noGpsAccess.set(false);
        this.loadNearbyProperties(this.activeCategoryId());
      })
      .catch(err => {
        this.noGpsAccess.set(true);
        this.userLocation.set(null);
        this.loadNearbyProperties(this.activeCategoryId());
      });
  }

  selectCategory(categoryId: string) {
    this.activeCategoryId.set(categoryId);
    this.loadNearbyProperties(categoryId);
  }

  loadNearbyProperties(categoryId: string) {
    this.isLoadingNearby.set(true);
    const loc = this.userLocation();
    
    if (loc) {
      this.listingsService.getNearby(
        loc.lat, loc.lng, 100,
        categoryId === 'All' ? undefined : categoryId
      ).subscribe({
        next: (data) => {
          this.nearbyListings.set(data);
          this.isLoadingNearby.set(false);
        },
        error: (err) => {
          this.nearbyListings.set([]);
          this.isLoadingNearby.set(false);
        }
      });
    } else {
      // Fallback: use Santo Domingo coordinates with large radius
      this.listingsService.getNearby(
        18.48, -69.93, 500,
        categoryId === 'All' ? undefined : categoryId
      ).subscribe({
        next: (data) => {
          this.nearbyListings.set(data);
          this.isLoadingNearby.set(false);
        },
        error: (err) => {
          this.nearbyListings.set([]);
          this.isLoadingNearby.set(false);
        }
      });
    }
  }

  private loadInspirationProperties() {
    this.isLoadingInspiration.set(true);
    // Use fallback (Santo Domingo, big radius) to load inspiration
    this.listingsService.getAllListings().subscribe({
      next: (data) => {
        const reversed = [...data].reverse();
        const inspiration = reversed.length > 3 ? reversed.slice(0, 6) : reversed;
        this.inspirationProperties.set(inspiration);
        
        if (reversed.length > 0) {
          this.featuredProperty.set(reversed[0]);
        }
        
        this.isLoadingInspiration.set(false);
      },
      error: (err) => {
        this.inspirationProperties.set([]);
        this.isLoadingInspiration.set(false);
      }
    });
  }

  viewProperty(id: string) {
    this.router.navigate(['/properties', id]);
  }

  scrollNearby(direction: 'left' | 'right') {
    if (!this.nearbyContainer) return;
    
    const container = this.nearbyContainer.nativeElement;
    const scrollAmount = container.clientWidth * 0.8; 
    
    container.scrollBy({
      left: direction === 'left' ? -scrollAmount : scrollAmount,
      behavior: 'smooth'
    });
  }
}
