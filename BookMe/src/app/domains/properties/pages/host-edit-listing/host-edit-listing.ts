import { Component, inject, OnInit, Input, Output, EventEmitter, NgZone } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators, FormArray, FormControl } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { HostListingService } from '../../../../core/services/host-listing';
import { AmenityService } from '../../../../core/services/amenity.service';
import { AmenityDTO } from '../../../../core/services/host-listing.types';
import { HttpClient } from '@angular/common/http';
import { LeafletModule } from '@asymmetrik/ngx-leaflet';
import * as L from 'leaflet';
import { MapPickerComponent, MapLocation } from '../../../../shared/components/map-picker/map-picker';
import { ViewChild } from '@angular/core';

interface ListingImagePreview {
  id?: string;
  url: string;
  isNew: boolean;
  file?: File;
}

@Component({
  selector: 'app-host-edit-listing',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, MapPickerComponent],
  templateUrl: './host-edit-listing.html',
  styleUrls: ['./host-edit-listing.css']
})
export class HostEditListingComponent implements OnInit {
  @ViewChild('mapPicker') mapPicker?: MapPickerComponent;

  private fb = inject(FormBuilder);
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private hostListingService = inject(HostListingService);
  private activatedRoute = inject(ActivatedRoute);
  private http = inject(HttpClient);
  private zone = inject(NgZone);
  private amenityService = inject(AmenityService);

  @Input() listingId: string = '';
  @Output() close = new EventEmitter<void>();
  @Output() saved = new EventEmitter<void>();

  isLoading = true;
  isSaving = false;
  showSavedToast = false;
  isGeocoding = false;
  showLocationHighlight = false;

  images: ListingImagePreview[] = [];

  amenityOptions: AmenityDTO[] = [];
  isLoadingAmenities = false;

  editForm: FormGroup = this.fb.group({
    title: ['', [Validators.required, Validators.maxLength(100)]],
    description: ['', [Validators.required, Validators.minLength(20)]],
    amenities: this.fb.array([]),

    maxGuests: [1, [Validators.required, Validators.min(1)]],
    bedroomsQuantity: [1, [Validators.required, Validators.min(0)]],
    bedsQuantity: [1, [Validators.required, Validators.min(1)]],
    bathroomsQuantity: [1, [Validators.required, Validators.min(0)]],

    pricePerNight: [0, [Validators.required, Validators.min(1)]],
    cleaningFee: [0, [Validators.required, Validators.min(0)]],
    checkInTime: ['14:00', Validators.required],
    checkOutTime: ['11:00', Validators.required],
    
    country: ['', Validators.required],
    city: ['', Validators.required],
    address: ['', Validators.required],
    latitude: [{ value: 0, disabled: true }, Validators.required],
    longitude: [{ value: 0, disabled: true }, Validators.required]
  });

  get amenitiesArray(): FormArray {
    return this.editForm.get('amenities') as FormArray;
  }

  ngOnInit() {
    // If not provided via Input, try to get from route
    if (!this.listingId) {
      this.listingId = this.activatedRoute.snapshot.paramMap.get('id') || '';
    }

    if (this.listingId) {
      this.loadAmenities();
      this.loadListing();
    } else {
      this.isLoading = false;
    }
  }

  loadAmenities() {
    this.isLoadingAmenities = true;
    this.amenityService.getAll().subscribe({
      next: (data) => {
        this.amenityOptions = data;
        this.isLoadingAmenities = false;
      },
      error: () => {
        this.isLoadingAmenities = false;
      }
    });
  }

  loadListing() {
    this.hostListingService.getListingById(this.listingId).subscribe({
      next: (data) => {
        // Prepare patch object for better performance
        const patchData = {
          title: data.title,
          description: data.description,
          maxGuests: data.capacity.maxGuests || 1,
          bedroomsQuantity: data.capacity.bedroomsQuantity || 1,
          bedsQuantity: data.capacity.bedsQuantity || 1,
          bathroomsQuantity: data.capacity.bathroomsQuantity || 1,
          pricePerNight: data.pricingRules.pricePerNight || 0,
          cleaningFee: data.pricingRules.cleaningFee || 0,
          checkInTime: (data.pricingRules.checkInTime || '14:00').substring(0, 5),
          checkOutTime: (data.pricingRules.checkOutTime || '11:00').substring(0, 5),
          latitude: data.location.latitude || 18.48,
          longitude: data.location.longitude || -69.93,
          city: data.location.city,
          country: data.location.country,
          address: data.location.address
        };

        this.editForm.patchValue(patchData);

        // Patch amenities efficiently
        if (data.amenities && Array.isArray(data.amenities)) {
          this.amenitiesArray.clear({ emitEvent: false });
          data.amenities.forEach(am => {
            this.amenitiesArray.push(new FormControl(am.id), { emitEvent: false });
          });
        }

        // Patch images
        if (data.images && Array.isArray(data.images)) {
          this.images = data.images.map(img => ({
            id: img.id,
            url: img.url,
            isNew: false
          }));
        }

        // Move the map to the correct position after the component is rendered
        setTimeout(() => {
          if (this.mapPicker) {
            this.mapPicker.updateLocation(patchData.latitude, patchData.longitude);
            this.mapPicker.refresh();
          }
        }, 300);

        this.isLoading = false;
      },
      error: (err) => {
        console.error('Error fetching listing details:', err);
        this.isLoading = false;
        this.router.navigate(['/host/dashboard']);
      }
    });
  }

  onLocationSelected(location: MapLocation) {
    this.editForm.patchValue({
      latitude: location.lat,
      longitude: location.lng,
      city: location.city,
      country: location.country,
      address: location.address
    });
    
    this.showLocationHighlight = true;
    setTimeout(() => this.showLocationHighlight = false, 2000);
  }

  isAmenityChecked(id: string): boolean {
    return this.amenitiesArray.controls.some(c => c.value === id);
  }

  onAmenityToggle(event: any, id: string) {
    if (event.target.checked) {
      this.amenitiesArray.push(new FormControl(id));
    } else {
      const idx = this.amenitiesArray.controls.findIndex(c => c.value === id);
      if (idx >= 0) this.amenitiesArray.removeAt(idx);
    }
  }

  onFileSelected(event: any) {
    const files: FileList = event.target.files;
    if (files) {
      for (let i = 0; i < files.length; i++) {
        const file = files[i];
        const reader = new FileReader();
        reader.onload = (e: any) => {
          this.images.push({
            url: e.target.result,
            isNew: true,
            file: file
          });
        };
        reader.readAsDataURL(file);
      }
    }
    // Reset input so same file can be selected again
    event.target.value = '';
  }

  removeImage(index: number) {
    this.images.splice(index, 1);
  }

  increment(field: string, step: number = 1) {
    const current = this.editForm.get(field)?.value ?? 0;
    this.editForm.get(field)?.setValue(current + step);
  }

  decrement(field: string, step: number = 1, min: number = 0) {
    const current = this.editForm.get(field)?.value ?? 0;
    if (current - step >= min) {
      this.editForm.get(field)?.setValue(current - step);
    }
  }

  saveChanges() {
    if (this.editForm.invalid) {
      this.editForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    // Use getRawValue to get disabled fields (latitude, longitude)
    const formValue = this.editForm.getRawValue();
    
    // Construct flat updateData to match backend ListingUpdateDTO
    const updateData: any = {
      title: formValue.title,
      description: formValue.description,
      // Map enum type if needed, but currently keeping as is if matches
      maxGuests: formValue.maxGuests,
      bedroomsQuantity: formValue.bedroomsQuantity,
      bedsQuantity: formValue.bedsQuantity,
      bathroomsQuantity: formValue.bathroomsQuantity,
      country: formValue.country,
      city: formValue.city,
      address: formValue.address,
      latitude: parseFloat(formValue.latitude?.toString() || '18.48'),
      longitude: parseFloat(formValue.longitude?.toString() || '-69.93'),
      pricePerNight: formValue.pricePerNight,
      cleaningFee: formValue.cleaningFee,
      checkInTime: formValue.checkInTime.includes(':') && formValue.checkInTime.split(':').length === 2 ? formValue.checkInTime + ':00' : formValue.checkInTime, 
      checkOutTime: formValue.checkOutTime.includes(':') && formValue.checkOutTime.split(':').length === 2 ? formValue.checkOutTime + ':00' : formValue.checkOutTime,
      amenityIds: this.amenitiesArray.value || []
    };
    
    // Obtenemos las imágenes nuevas seleccionadas
    const newFiles = this.images.filter(i => i.isNew && i.file).map(i => i.file!);

    this.hostListingService.updateListing(this.listingId, updateData).subscribe({
      next: () => {
        if (newFiles.length > 0) {
          this.hostListingService.uploadImages(this.listingId, newFiles).subscribe({
            next: () => {
              this.finalizeUpdate();
            },
            error: (err) => {
              console.error('Data guardada, pero ocurrió un error al subir imágenes', err);
              this.isSaving = false;
            }
          });
        } else {
          this.finalizeUpdate();
        }
      },
      error: () => {
        this.isSaving = false;
      }
    });
  }

  private finalizeUpdate() {
    this.isSaving = false;
    this.showSavedToast = true;
    setTimeout(() => {
      this.showSavedToast = false;
      this.router.navigate(['/host/dashboard']);
    }, 1500);
  }

  goBack() {
    this.router.navigate(['/host/dashboard']);
  }
}
