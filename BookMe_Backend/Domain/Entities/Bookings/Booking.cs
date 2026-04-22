using Domain.Entities.Listings;
using Domain.Entities.Users;
using Domain.Enums.Bookings;

namespace Domain.Entities.Bookings
{
    public class Booking
    {
        public Guid Id { get; set; }
        public Guid ListingId { get; set; }
        public Guid GuestId { get; set; }

        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public int TotalNights { get; set; }

        public BookingStatus Status { get; set; }

        public int TotalGuests { get; set; }
        public decimal CleaningFee { get; set; }
        public decimal AccomodationCost { get; set; }
        public decimal TotalPrice { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CancelledAt { get; set; }

        public Listing Listing { get; set; } = null;
        public User Guest { get; set; } = null;
    }
}
