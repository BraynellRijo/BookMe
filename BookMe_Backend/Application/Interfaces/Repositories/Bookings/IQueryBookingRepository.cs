using Domain.Entities.Bookings;
using Domain.Enums.Bookings;

namespace Application.Interfaces.Repositories.Bookings
{
    public interface IQueryBookingRepository : IQueryRepository<Booking>
    {
        Task<Booking> GetBookingWithListingAsync(Guid id);
        IQueryable<Booking> GetAllByListing(Guid listingId);
        IQueryable<Booking> GetAllByGuest(Guid guestId);
        IQueryable<Booking> FilterBookingsByDateRange(DateTime checkIn, DateTime checkOut);
        IQueryable<Booking> GetActiveBookingsByListing(Guid listingId);
        IQueryable<Booking> GetByStatus(BookingStatus status);
        Task<bool> HasOverlappingAsync(Guid listingId, DateTime startDate, DateTime endDate, Guid? bookingId = null);
    }
}
