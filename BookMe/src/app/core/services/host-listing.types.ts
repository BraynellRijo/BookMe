export interface CapacityDTO {
  maxGuests: number;
  bedroomsQuantity: number;
  bedsQuantity: number;
  bathroomsQuantity: number;
}

export interface LocationDTO {
  latitude: number;
  longitude: number;
  country?: string;
  city?: string;
  address?: string;
}

export interface PricingRulesDTO {
  pricePerNight: number;
  cleaningFee?: number;
  checkInTime?: string;
  checkOutTime?: string;
}

export interface CreateListingDTO {
  title: string;
  description: string;
  propertyType: string;
  amenityIds: string[];
  
  capacity: CapacityDTO;
  location: LocationDTO;
  pricingRules: PricingRulesDTO;
}

export interface CreateListingResponseDTO {
  id: string;
  message: string;
}

export interface ListingUpdateDTO extends CreateListingDTO {
  // Can add any specific update fields if needed, but the user said "flat" and same properties
}

export interface HostListingSummaryDTO {
  id: string;
  title: string;
  isAvailable: boolean;
  pricePerNight: number;
  coverPhotoUrl: string;
  city?: string;
  country?: string;
}

export interface AmenityDTO {
  id: string;
  name: string;
  icon?: string;
}

export interface ListingImageDTO {
  id: string;
  url: string;
}

export interface HostListingDetailDTO {
  id: string;
  title: string;
  description: string;
  propertyType: string;
  status: string;
  isAvailable: boolean;
  
  capacity: CapacityDTO;
  location: LocationDTO;
  pricingRules: PricingRulesDTO;
  
  amenities: AmenityDTO[];
  images?: ListingImageDTO[];
  averageRating: number;
  reviewsCount: number;
  hostName?: string;
  hostImageUrl?: string;
}

/**
 * Matches the backend's public ListingDTO returned by /api/Listings endpoints.
 * Has nested objects for location, capacity, and pricingRules.
 */
export interface ListingDTO {
  id: string;
  hostId: string;
  title: string;
  description: string;
  type: string; // PropertyType enum string

  capacity: CapacityDTO;
  location: LocationDTO;
  pricingRules: PricingRulesDTO;
  images?: { url: string; publicId: string }[];
  averageRating: number;
  reviewsCount: number;
}
