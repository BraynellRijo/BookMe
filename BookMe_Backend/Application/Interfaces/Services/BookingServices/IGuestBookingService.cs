using Application.DTOs.BookingDTOs;

namespace Application.Interfaces.Services.BookingServices
{
    public interface IGuestBookingService 
    {
        Task CreateBookingAsync(BookingCreationDTO bookingCreationDTO, Guid guestId);
        Task CancelBooking(Guid bookingId, Guid userId);
        Task<IEnumerable<BookingDTO>> GetGuestBookingsAsync(Guid guestId);
        Task UpdateStatusCompleted();
    }
}
