export enum BookingStatus {
  Pending = 1,
  Blocked = 2,
  Confirmed = 3,
  Completed = 4,
  Cancelled = 5,
  Rejected = 6
}

export interface BookingCreationDTO {
  listingId: string;
  checkInDate: string; // ISO string
  checkOutDate: string; // ISO string
  totalGuests: number;
}

export interface BookingDTO {
  id: string;
  listingId: string;
  guestId: string;
  checkInDate: string;
  checkOutDate: string;
  status: BookingStatus;
  totalGuests: number;
  cleaningFee: number;
  totalPrice: number;
  listingTitle: string;
  listingLocation: string;
  listingImageUrl: string;
}
