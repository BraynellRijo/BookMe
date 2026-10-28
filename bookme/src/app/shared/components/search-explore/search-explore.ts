import { Component, Output, EventEmitter, OnInit, inject, signal, ViewEncapsulation } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { trigger, transition, style, animate } from '@angular/animations';
import { LeafletModule } from '@asymmetrik/ngx-leaflet';
import * as L from 'leaflet';

import { ListingsService } from '../../../core/services/listings.service';
import { GeolocationService, GeoLocation } from '../../../core/services/geolocation.service';
import { ListingDTO } from '../../../core/services/host-listing.types';

@Component({
  selector: 'app-search-explore',
  standalone: true,
  imports: [CommonModule, FormsModule, LeafletModule],
  templateUrl: './search-explore.html',
  styleUrls: ['./search-explore.css'],
  encapsulation: ViewEncapsulation.None,
  animations: [
    trigger('fadeSlide', [
      transition(':enter', [
        style({ opacity: 0, transform: 'translateY(15px)' }),
        animate('300ms cubic-bezier(0.4, 0, 0.2, 1)', style({ opacity: 1, transform: 'translateY(0)' }))
      ]),
      transition(':leave', [
        animate('250ms cubic-bezier(0.4, 0, 0.2, 1)', style({ opacity: 0, transform: 'translateY(15px)' }))
      ])
    ])
  ]
})
export class SearchExplore implements OnInit {
  private listingsService = inject(ListingsService);
  private geoService = inject(GeolocationService);
  private router = inject(Router);

  @Output() closeDialog = new EventEmitter<void>();

  // State
  listings = signal<ListingDTO[]>([]);
  isLoading = signal<boolean>(false);
  resultCount = signal<number>(0);

  // Filter state
  searchQuery = '';
  minPrice: number = 50;
  maxPrice: number = 1000;
  adults: number = 2;

  // Map state
  options = {
    layers: [
      L.tileLayer('https://{s}.basemaps.cartocdn.com/rastertiles/voyager/{z}/{x}/{y}{r}.png', {
        maxZoom: 19,
        attribution: '© OpenStreetMap contributors © CARTO'
      })
    ],
    zoom: 12,
    center: L.latLng(18.48, -69.93)
  };

  layers: L.Layer[] = [];
  map!: L.Map;
  userCoords: GeoLocation | null = null;

  ngOnInit() {
    this.isLoading.set(true);
    
    // Get user location and load nearby
    this.geoService.getCurrentPosition()
      .then(coords => {
        this.userCoords = coords;
        this.loadNearbyListings(coords.lat, coords.lng);
      })
      .catch(() => {
        // Fallback to Santo Domingo
        this.userCoords = { lat: 18.48, lng: -69.93 };
        this.loadNearbyListings(18.48, -69.93);
      });
  }

  onMapReady(map: L.Map) {
    this.map = map;
    setTimeout(() => map.invalidateSize(), 200);
    
    if (this.userCoords) {
      map.setView([this.userCoords.lat, this.userCoords.lng], 12);
    }
  }

  loadNearbyListings(lat: number, lng: number) {
    this.isLoading.set(true);
    this.listingsService.getNearby(lat, lng, 100).subscribe({
      next: (data) => {
        this.listings.set(data);
        this.resultCount.set(data.length);
        this.renderMarkers(data);
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error('Error loading search listings', err);
        this.listings.set([]);
        this.resultCount.set(0);
        this.isLoading.set(false);
      }
    });
  }

  searchListings() {
    if (this.searchQuery.trim()) {
      this.isLoading.set(true);
      this.listingsService.searchByTitle(this.searchQuery.trim()).subscribe({
        next: (data) => {
          this.listings.set(data);
          this.resultCount.set(data.length);
          this.renderMarkers(data);
          this.isLoading.set(false);
        },
        error: () => {
          this.listings.set([]);
          this.resultCount.set(0);
          this.isLoading.set(false);
        }
      });
    } else if (this.userCoords) {
      this.loadNearbyListings(this.userCoords.lat, this.userCoords.lng);
    }
  }

  applyFilters() {
    this.isLoading.set(true);
    this.listingsService.filterByPrice(this.minPrice, this.maxPrice).subscribe({
      next: (data) => {
        this.listings.set(data);
        this.resultCount.set(data.length);
        this.renderMarkers(data);
        this.isLoading.set(false);
      },
      error: () => {
        this.listings.set([]);
        this.resultCount.set(0);
        this.isLoading.set(false);
      }
    });
  }

  searchThisArea() {
    if (!this.map) return;
    const center = this.map.getCenter();
    this.loadNearbyListings(center.lat, center.lng);
  }

  private renderMarkers(properties: ListingDTO[]) {
    // Clear previous markers
    this.layers = [];

    // Add user location marker if available
    if (this.userCoords) {
      const userIcon = L.divIcon({
        className: 'user-map-marker',
        html: `<div style="width:16px;height:16px;background:#243642;border-radius:50%;border:3px solid white;box-shadow:0 0 10px rgba(0,0,0,0.3);"></div>`,
        iconSize: [16, 16],
        iconAnchor: [8, 8]
      });
      this.layers.push(
        L.marker([this.userCoords.lat, this.userCoords.lng], { icon: userIcon })
          .bindPopup('Tu ubicación')
      );
    }

    // Generate price markers for each property
    properties.forEach(prop => {
      const lat = prop.location?.latitude;
      const lng = prop.location?.longitude;
      if (!lat || !lng) return;

      const price = prop.pricingRules?.pricePerNight || 0;

      const icon = L.divIcon({
        className: 'custom-price-divicon',
        html: `<div class="price-bubble">$${price}</div>`,
        iconSize: [70, 36],
        iconAnchor: [35, 36],
        popupAnchor: [0, -30]
      });

      const marker = L.marker([lat, lng], { icon });
      marker.bindPopup(`
        <div style="text-align:center;font-family:inherit;">
          <strong style="font-size:13px;">${prop.title}</strong><br/>
          <span style="color:#666;font-size:11px;">${prop.location?.city || ''}</span><br/>
          <button onclick="window.__navigateToProperty__('${prop.id}')" 
            style="margin-top:6px;padding:4px 12px;background:#387478;color:white;border:none;border-radius:6px;font-size:11px;font-weight:bold;cursor:pointer;">
            Ver detalles
          </button>
        </div>
      `);

      this.layers.push(marker);
    });

    // Fit bounds if we have markers
    if (this.layers.length > 1 && this.map) {
      const group = L.featureGroup(this.layers);
      this.map.fitBounds(group.getBounds().pad(0.15));
    }
  }

  incrementAdults() { this.adults++; }
  decrementAdults() { if (this.adults > 1) this.adults--; }

  viewProperty(id: string) {
    this.closeDialog.emit();
    setTimeout(() => this.router.navigate(['/properties', id]), 100);
  }

  onClose() {
    this.closeDialog.emit();
  }

  // Global navigation bridge for Leaflet popups (outside Angular zone)
  constructor() {
    (window as any).__navigateToProperty__ = (id: string) => {
      this.viewProperty(id);
    };
  }
}
