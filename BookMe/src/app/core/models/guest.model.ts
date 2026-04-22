export interface BookingCreationDTO {
  listingId: string;
  checkIn: string; // ISO date string e.g., '2026-05-10T00:00:00Z'
  checkOut: string; // ISO date string
  guestsCount: number;
}

export interface GuestBookingDTO {
  id: string;
  listingId: string;
  listingTitle: string;
  coverPhotoUrl?: string;
  checkIn: string;
  checkOut: string;
  guestsCount: number;
  totalPrice: number;
  status: 'Pending' | 'Confirmed' | 'Cancelled' | 'Completed';
  createdAt: string;
}

export interface ReviewCreationDTO {
  listingId: string;
  rating: number;   // 1 to 5
  comment: string;
}

export interface ReviewUpdateDTO {
  comment?: string;
  rating?: number;
}
