export interface ReviewDTO {
  id?: string;
  listingId?: string;
  guestId?: string;
  guestName?: string;
  guestPhotoUrl?: string;
  rating: number;
  comment: string;
  createdAt: string;
}

export interface ReviewCreationDTO {
  listingId: string;
  rating: number;
  comment: string;
}
