
using Domain.Enums.Bookings;

namespace Application.DTOs.BookingDTOs
{
    public class BookingCreationDTO
    {
        public Guid ListingId { get; set; }

        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }

        public int TotalGuests { get; set; }
    }
}
