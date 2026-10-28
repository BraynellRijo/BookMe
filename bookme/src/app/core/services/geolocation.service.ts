import { Injectable } from '@angular/core';
import { Observable, from } from 'rxjs';

export interface GeoLocation {
  lat: number;
  lng: number;
}

@Injectable({
  providedIn: 'root'
})
export class GeolocationService {

  constructor() { }

  /**
   * Obtiene la posición actual del usuario.
   * Retorna un Promise con las coordenadas.
   */
  getCurrentPosition(): Promise<GeoLocation> {
    return new Promise((resolve, reject) => {
      if (!navigator.geolocation) {
        reject(new Error('Geolocation is not supported by your browser'));
        return;
      }

      navigator.geolocation.getCurrentPosition(
        (position) => {
          resolve({
            lat: position.coords.latitude,
            lng: position.coords.longitude
          });
        },
        (error) => {
          reject(error);
        },
        {
          enableHighAccuracy: true,
          timeout: 5000,
          maximumAge: 0
        }
      );
    });
  }

  /**
   * Verifica el estado del permiso de geolocalización.
   */
  async checkPermissionStatus(): Promise<PermissionState> {
    if (!navigator.permissions) return 'prompt';
    try {
      const status = await navigator.permissions.query({ name: 'geolocation' });
      return status.state;
    } catch (e) {
      return 'prompt';
    }
  }
}
