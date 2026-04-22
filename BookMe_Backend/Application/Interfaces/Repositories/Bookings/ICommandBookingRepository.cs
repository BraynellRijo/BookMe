using Domain.Entities.Bookings;
using Domain.Enums.Bookings;

namespace Application.Interfaces.Repositories.Bookings
{
    public interface ICommandBookingRepository : ICommandRepository<Booking>
    {
        public Task UpdateStatusAsync(Guid bookingId, BookingStatus newStatus);
        public Task CancelBookingAsync(Guid bookingId);
    }
}
