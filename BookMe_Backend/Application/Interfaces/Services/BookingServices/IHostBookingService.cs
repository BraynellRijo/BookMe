namespace Application.Interfaces.Services.BookingServices
{
    public interface IHostBookingService
    {
        Task BlockBookingStatus(Guid bookingId, Guid userId, string userRole);
        Task DeleteBlockedBooking(Guid bookingId, Guid userId, string userRole);
        Task CancelBooking(Guid bookingId, Guid userId, string userRole);
    }
}
