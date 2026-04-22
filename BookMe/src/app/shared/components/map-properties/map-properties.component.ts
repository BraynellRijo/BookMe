import { Component, Input, OnInit, OnChanges, SimpleChanges, inject, Output, EventEmitter, ViewEncapsulation } from '@angular/core';
import { CommonModule } from '@angular/common';
import { LeafletModule } from '@asymmetrik/ngx-leaflet';
import * as L from 'leaflet';
import { Router } from '@angular/router';

import { ListingDTO } from '../../../core/services/host-listing.types';

@Component({
  selector: 'app-map-properties',
  standalone: true,
  imports: [CommonModule, LeafletModule],
  templateUrl: './map-properties.component.html',
  styleUrls: ['./map-properties.component.scss'],
  encapsulation: ViewEncapsulation.None
})
export class MapPropertiesComponent implements OnInit, OnChanges {
  private router = inject(Router);

  @Input() properties: ListingDTO[] = [];
  @Input() mapCenter: L.LatLngExpression = [18.48, -69.93];
  
  @Output() markerClicked = new EventEmitter<string>();

  map!: L.Map;
  markerLayers: L.Layer[] = [];
  
  mapOptions = {
    layers: [
      L.tileLayer('https://{s}.basemaps.cartocdn.com/rastertiles/voyager/{z}/{x}/{y}{r}.png', {
        maxZoom: 19,
        attribution: '© OpenStreetMap contributors © CARTO'
      })
    ],
    zoom: 12,
    center: L.latLng(18.48, -69.93)
  };

  ngOnInit() {
    this.mapOptions.center = L.latLng(this.mapCenter as L.LatLngTuple);
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['properties'] && this.map) {
      this.renderMarkers();
    }
  }

  onMapReady(map: L.Map) {
    this.map = map;
    this.renderMarkers();
    
    if (this.mapCenter) {
       this.map.setView(this.mapCenter, 12);
    }
  }

  private renderMarkers() {
    if (!this.map) return;

    this.markerLayers = [];

    if (!this.properties || this.properties.length === 0) return;

    const newLayers: L.Layer[] = [];

    this.properties.forEach(prop => {
      const lat = prop.location?.latitude;
      const lng = prop.location?.longitude;

      if (!lat || !lng) return;

      const price = prop.pricingRules?.pricePerNight || 0;

      const icon = L.divIcon({
        className: 'custom-price-divicon',
        html: `<div class="price-marker"><span class="price-text">$${price}</span></div>`,
        iconSize: [60, 40],
        iconAnchor: [30, 40],
        popupAnchor: [0, -35]
      });

      const marker = L.marker([lat, lng], { icon });

      const popupHtml = `
        <div style="text-align:center;font-family:inherit;">
          <h5 style="font-weight:bold;font-size:13px;margin-bottom:6px;">${prop.title}</h5>
          <span style="color:#888;font-size:11px;">${prop.location?.city || ''}</span><br/>
          <button id="btn-prop-${prop.id}" style="margin-top:8px;padding:5px 14px;background:#387478;color:white;border:none;border-radius:8px;font-size:11px;font-weight:bold;cursor:pointer;">Ver Detalles</button>
        </div>
      `;
      marker.bindPopup(popupHtml);

      marker.on('popupopen', () => {
         const btn = document.getElementById(`btn-prop-${prop.id}`);
         if (btn) {
           btn.addEventListener('click', () => {
              this.router.navigate(['/properties', prop.id]);
           });
         }
      });

      newLayers.push(marker);
    });

    this.markerLayers = newLayers;

    if (newLayers.length > 0) {
      const group = L.featureGroup(newLayers);
      this.map.fitBounds(group.getBounds().pad(0.2));
    }
  }
}
