import { Component, inject, OnInit, NgZone, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators, FormArray, FormControl } from '@angular/forms';
import { HostListingService } from '../../../../core/services/host-listing';
import { AmenityService } from '../../../../core/services/amenity.service';
import { AmenityDTO, CreateListingDTO } from '../../../../core/services/host-listing.types';
import { Router, RouterLink } from '@angular/router';
import { ToastService } from '../../../../core/services/toast.service';
import { trigger, transition, style, animate } from '@angular/animations';
import { LeafletModule } from '@asymmetrik/ngx-leaflet';
import { HttpClient } from '@angular/common/http';
import { TokenService } from '../../../../core/services/token.service';
import { MapPickerComponent, MapLocation } from '../../../../shared/components/map-picker/map-picker';
import { ViewChild } from '@angular/core';

@Component({
  selector: 'app-host-listing-create',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink, LeafletModule, MapPickerComponent],
  templateUrl: './host-listing-create.html',
  styleUrls: ['./host-listing-create.css'],
  animations: [
    trigger('stepTransition', [
      transition(':enter', [
        style({ opacity: 0, transform: 'translateX(20px)' }),
        animate('300ms ease-out', style({ opacity: 1, transform: 'translateX(0)' }))
      ]),
      transition(':leave', [
        animate('300ms ease-in', style({ opacity: 0, transform: 'translateX(-20px)' }))
      ])
    ])
  ]
})
export class HostListingCreateComponent implements OnInit {
  @ViewChild('mapPicker') mapPicker?: MapPickerComponent;

  private fb = inject(FormBuilder);
  private hostListingService = inject(HostListingService);
  private router = inject(Router);
  private zone = inject(NgZone);
  private http = inject(HttpClient);
  private toastService = inject(ToastService);
  private amenityService = inject(AmenityService);
  private cdr = inject(ChangeDetectorRef);
  private tokenService = inject(TokenService);

  private readonly CACHE_KEY = 'bookme_new_listing_draft';

  currentStep = 1;
  totalSteps = 7;
  isSubmitting = false;
  isPublished = false;
  isGeocoding = false;
  showLocationHighlight = false;

  createdListingId: string | null = null;
  imageUploadError = false;

  listingForm: FormGroup = this.fb.group({
    title: ['', [Validators.required, Validators.maxLength(100)]],
    propertyType: ['Apartment', Validators.required],

    country: ['', Validators.required],
    city: ['', Validators.required],
    address: ['', Validators.required],
    latitude: [{ value: 18.48, disabled: true }, Validators.required],
    longitude: [{ value: -69.93, disabled: true }, Validators.required],

    description: ['', [Validators.required, Validators.minLength(20)]],

    maxGuests: [1, [Validators.required, Validators.min(1)]],
    bedroomsQuantity: [1, [Validators.required, Validators.min(0)]],
    bedsQuantity: [1, [Validators.required, Validators.min(1)]],
    bathroomsQuantity: [1, [Validators.required, Validators.min(0)]],

    amenities: this.fb.array([]),

    pricePerNight: [0, [Validators.required, Validators.min(1)]]
  });

  get amenitiesArray(): FormArray {
    return this.listingForm.get('amenities') as FormArray;
  }

  ngOnInit() {
    this.loadAmenities();
    this.loadDraft();
  }

  loadAmenities() {
    this.isLoadingAmenities = true;
    this.amenityService.getAll().subscribe({
      next: (data) => {
        this.amenityOptions = data;
        this.isLoadingAmenities = false;
      },
      error: () => {
        this.toastService.error('No se pudieron cargar las comodidades. Intenta recargar la página.');
        this.isLoadingAmenities = false;
      }
    });
  }

  loadDraft() {
    const draft = localStorage.getItem(this.CACHE_KEY);
    if (draft) {
      try {
        const data = JSON.parse(draft);

        // Patch simple values
        this.listingForm.patchValue({
          title: data.title,
          propertyType: data.propertyType,
          country: data.country,
          city: data.city,
          address: data.address,
          description: data.description,
          maxGuests: data.maxGuests,
          bedroomsQuantity: data.bedroomsQuantity || data.bedrooms,
          bedsQuantity: data.bedsQuantity || 1,
          bathroomsQuantity: data.bathroomsQuantity || data.bathrooms,
          pricePerNight: data.pricePerNight
        });

        // Reconstruct amenities FormArray
        if (data.amenities && Array.isArray(data.amenities)) {
          this.amenitiesArray.clear();
          data.amenities.forEach((id: string) => {
            this.amenitiesArray.push(new FormControl(id));
          });
        }
      } catch (e) {
        // Silent fail for draft loading
      }
    }
  }

  saveAndExit() {
    const draftData = this.listingForm.value;
    localStorage.setItem(this.CACHE_KEY, JSON.stringify(draftData));
    this.router.navigate(['/host/dashboard']);
  }

  clearDraft() {
    localStorage.removeItem(this.CACHE_KEY);
  }

  selectedFiles: File[] = [];
  previewUrls: string[] = [];

  // Dynamic amenity catalog loaded from API
  amenityOptions: AmenityDTO[] = [];
  isLoadingAmenities = false;

  nextStep() {
    if (this.currentStep < this.totalSteps) {
      this.currentStep++;
      // Redibujar mapa si se navega hacia él
      if (this.currentStep === 2) {
        setTimeout(() => this.mapPicker?.refresh(), 100);
      }
    }
  }

  onLocationSelected(location: MapLocation) {
    this.listingForm.patchValue({
      latitude: location.lat,
      longitude: location.lng,
      city: location.city,
      country: location.country,
      address: location.address
    });

    this.showLocationHighlight = true;
    setTimeout(() => this.showLocationHighlight = false, 2000);
    this.cdr.detectChanges();
  }

  prevStep() {
    if (this.currentStep > 1) {
      this.currentStep--;
    }
  }

  isAmenityChecked(id: string): boolean {
    return this.amenitiesArray.controls.some(c => c.value === id);
  }

  onCheckboxChange(e: any, id: string) {
    if (e.target.checked) {
      this.amenitiesArray.push(new FormControl(id));
    } else {
      const index = this.amenitiesArray.controls.findIndex(c => c.value === id);
      if (index >= 0) this.amenitiesArray.removeAt(index);
    }
  }

  onFileSelected(event: any) {
    const files: FileList = event.target.files;
    if (files) {
      for (let i = 0; i < files.length; i++) {
        const file = files[i];
        this.selectedFiles.push(file);

        const reader = new FileReader();
        reader.onload = (e: any) => {
          this.previewUrls.push(e.target.result);
        };
        reader.readAsDataURL(file);
      }
    }
  }

  removeImage(index: number) {
    this.selectedFiles.splice(index, 1);
    this.previewUrls.splice(index, 1);
  }

  submitListing() {
    if (this.createdListingId) {
      this.uploadImagesFlow(this.createdListingId);
      return;
    }

    if (this.listingForm.invalid) {
      this.listingForm.markAllAsTouched();
      this.toastService.error('Formulario incompleto. Asegúrate de rellenar la capacidad y la descripción correctamente.');
      return;
    }

    if (this.selectedFiles.length === 0) {
      this.toastService.warning('Debes subir al menos 1 foto de tu propiedad.');
      return;
    }

    this.isSubmitting = true;
    this.cdr.detectChanges();

    const formValue = this.listingForm.getRawValue();

    const submitData: CreateListingDTO = {
      title: formValue.title,
      description: formValue.description,
      propertyType: formValue.propertyType,
      amenityIds: this.amenitiesArray.value || [],
      capacity: {
        maxGuests: Number(formValue.maxGuests),
        bedroomsQuantity: Number(formValue.bedroomsQuantity),
        bedsQuantity: Number(formValue.bedsQuantity),
        bathroomsQuantity: Number(formValue.bathroomsQuantity),
      },
      location: {
        latitude: parseFloat(formValue.latitude?.toString() || '18.48'),
        longitude: parseFloat(formValue.longitude?.toString() || '-69.93'),
        country: formValue.country,
        city: formValue.city,
        address: formValue.address,
      },
      pricingRules: {
        pricePerNight: Number(formValue.pricePerNight),
        cleaningFee: 0,
        checkInTime: '15:00:00',
        checkOutTime: '11:00:00'
      }
    };

    this.hostListingService.createListing(submitData).subscribe({
      next: (res) => {
        this.createdListingId = res.id;
        this.uploadImagesFlow(res.id);
      },
      error: (err) => {
        console.error('Error creating listing:', err);
        this.toastService.error('Error al crear la propiedad. Revisa los datos.');
        this.isSubmitting = false;
        this.cdr.detectChanges();
      }
    });
  }

  private uploadImagesFlow(listingId: string) {
    this.isSubmitting = true;
    this.imageUploadError = false;
    this.cdr.detectChanges();

    this.hostListingService.uploadImages(listingId, this.selectedFiles).subscribe({
      next: () => {
        this.finalizeSubmission();
      },
      error: (err) => {
        console.error('Base listing created but image upload failed', err);
        this.isSubmitting = false;
        this.imageUploadError = true;
        this.toastService.warning('Propiedad creada, pero hubo un error al subir las fotos. ¡Puedes reintentar ahora mismo!');
        this.cdr.detectChanges();
      }
    });
  }

  private finalizeSubmission() {
    // Set states
    this.isSubmitting = false;
    this.isPublished = true;

    // Clear draft
    this.clearDraft();

    // Ensure CD runs
    this.cdr.detectChanges();
  }
}
