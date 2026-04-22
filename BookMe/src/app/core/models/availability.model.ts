export interface BlockDatesDTO {
  listingId: string;
  startDate: string; // ISO format
  endDate: string; // ISO format
  reason?: string;
}

export interface BlockedDateRangeDTO {
  id: string;
  startDate: string;
  endDate: string;
  isManualBlock: boolean;
}
