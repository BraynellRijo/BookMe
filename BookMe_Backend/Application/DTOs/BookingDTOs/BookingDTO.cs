using Domain.Enums.Bookings;

namespace Application.DTOs.BookingDTOs
{
    public class    BookingDTO
    {
        public Guid Id { get; set; }
        public Guid ListingId { get; set; }
        public Guid GuestId { get; set; }

        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }

        public BookingStatus Status { get; set; }

        public int TotalGuests { get; set; }
        public decimal CleaningFee { get; set; }
        public decimal TotalPrice { get; set; }

        public string ListingTitle { get; set; }
        public string ListingLocation { get; set; }
        public string ListingImageUrl { get; set; }
    }
}
