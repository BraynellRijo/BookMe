import { Component, Input, Output, EventEmitter, OnInit, inject, signal, NgZone, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { LeafletModule } from '@asymmetrik/ngx-leaflet';
import * as L from 'leaflet';
import { HttpClient } from '@angular/common/http';

export interface MapLocation {
  lat: number;
  lng: number;
  address: string;
  city: string;
  country: string;
}

@Component({
  selector: 'app-map-picker',
  standalone: true,
  imports: [CommonModule, LeafletModule],
  templateUrl: './map-picker.html',
  styleUrls: ['./map-picker.css']
})
export class MapPickerComponent implements OnInit {
  private http = inject(HttpClient);
  private zone = inject(NgZone);
  private cdr = inject(ChangeDetectorRef);

  @Input() initialLat: number = 18.48;
  @Input() initialLng: number = -69.93;
  @Input() zoom: number = 14;
  @Input() height: string = '400px';

  @Output() locationSelected = new EventEmitter<MapLocation>();

  map!: L.Map;
  markerLayer: L.Layer[] = [];
  isGeocoding = signal<boolean>(false);

  mapOptions = {
    layers: [
      L.tileLayer('https://{s}.basemaps.cartocdn.com/rastertiles/voyager/{z}/{x}/{y}{r}.png', {
        maxZoom: 19,
        attribution: '© OpenStreetMap contributors © CARTO'
      })
    ],
    zoom: this.zoom,
    center: L.latLng(this.initialLat, this.initialLng)
  };

  ngOnInit() {
    this.mapOptions.zoom = this.zoom;
    this.mapOptions.center = L.latLng(this.initialLat, this.initialLng);
  }

  onMapReady(map: L.Map) {
    this.map = map;
    this.map.on('click', (e: L.LeafletMouseEvent) => {
      this.zone.run(() => {
        this.updateLocation(e.latlng.lat, e.latlng.lng);
      });
    });

    // Inicilizamos el marcador
    this.setMarker(this.initialLat, this.initialLng);
    
    // Forzamos el redibujado por si está en un contenedor oculto al inicio
    setTimeout(() => {
      this.map.invalidateSize();
    }, 200);
  }

  setMarker(lat: number, lng: number) {
    const icon = L.divIcon({
      className: 'custom-map-marker',
      html: `
        <div class="marker-container">
          <div class="marker-pin"></div>
          <div class="marker-pulse"></div>
        </div>`,
      iconSize: [30, 30],
      iconAnchor: [15, 30]
    });

    const marker = L.marker([lat, lng], { icon, draggable: true });
    
    marker.on('dragend', (e) => {
      const pos = e.target.getLatLng();
      this.zone.run(() => {
        this.updateLocation(pos.lat, pos.lng);
      });
    });

    this.markerLayer = [marker];
  }

  updateLocation(lat: number, lng: number) {
    this.setMarker(lat, lng);
    this.reverseGeocode(lat, lng);
    
    if (this.map) {
      this.map.panTo([lat, lng]);
    }
  }

  reverseGeocode(lat: number, lng: number) {
    this.isGeocoding.set(true);
    const url = `https://nominatim.openstreetmap.org/reverse?format=json&lat=${lat}&lon=${lng}`;
    
    this.http.get<any>(url, {
      headers: {
        'Accept-Language': 'es',
        'User-Agent': 'BookMeApp/1.0'
      }
    }).subscribe({
      next: (res) => {
        if (res && res.address) {
          const addr = res.address;
          const location: MapLocation = {
            lat,
            lng,
            city: addr.city || addr.town || addr.village || addr.county || '',
            country: addr.country || '',
            address: addr.road || addr.suburb || addr.neighbourhood || res.display_name.split(',')[0] || ''
          };
          this.locationSelected.emit(location);
        }
        this.isGeocoding.set(false);
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Error reverse geocoding:', err);
        this.isGeocoding.set(false);
        this.cdr.detectChanges();
      }
    });
  }

  /**
   * Método público para forzar el resize del mapa desde el padre
   */
  public refresh() {
    if (this.map) {
      this.map.invalidateSize();
    }
  }
}
