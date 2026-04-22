using Application.DTOs.BookingDTOs;
using Application.DTOs.NotificationDTOs;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Bookings;
using Application.Interfaces.Repositories.Listings;
using Application.Interfaces.Services.BookingServices;
using Application.Interfaces.Services.NotificationServices;
using AutoMapper;
using Domain.Enums.Bookings;
using FluentValidation;

namespace Application.Services.BookingService
{
    public class HostBookingService(
        ICommandBookingRepository commandBookingRepository,
        IQueryBookingRepository queryBookingRepository,
        IQueryListingRepository queryListingRepository,
        IMapper mapper,
        IValidator<BookingDTO> bookingDTOValidator,
        INotificationService notificationService) : IHostBookingService 
    {
        private readonly ICommandBookingRepository _commandBookingRepository = commandBookingRepository;
        private readonly IQueryBookingRepository _queryBookingRepository = queryBookingRepository;
        private readonly IQueryListingRepository _queryListingRepository = queryListingRepository;
        private readonly IMapper _mapper = mapper;
        private readonly IValidator<BookingDTO> _bookingDTOValidator = bookingDTOValidator;
        private readonly INotificationService _notificationService = notificationService;

        // Update booking status methods for hosts
        public async Task BlockBookingStatus(Guid bookingId, Guid userId, string userRole)
        {
            ValidateBookingId(bookingId);

            var booking = await _queryBookingRepository.GetByIdAsync(bookingId)
                    ?? throw new KeyNotFoundException($"Booking with ID {bookingId} was not found.");

            if (booking.Status == BookingStatus.Confirmed || booking.Status == BookingStatus.Cancelled)
                throw new InvalidOperationException("Cannot block a confirmed or cancelled booking.");

            await CheckHostAuthorization(booking.ListingId, userId, userRole);

            await _commandBookingRepository.UpdateStatusAsync(bookingId, BookingStatus.Blocked);
        }

        public async Task DeleteBlockedBooking(Guid bookingId, Guid userId, string userRole)
        {
            ValidateBookingId(bookingId);

            var booking = await _queryBookingRepository.GetByIdAsync(bookingId)
                    ?? throw new KeyNotFoundException($"Booking with ID {bookingId} was not found.");

            if (booking.Status != BookingStatus.Blocked)
                throw new InvalidOperationException("Only blocked bookings can be deleted.");

            await CheckHostAuthorization(booking.ListingId, userId, userRole);
            await _commandBookingRepository.DeleteAsync(bookingId);
        }

        public async Task CancelBooking(Guid bookingId, Guid userId, string userRole)
        {
            ValidateBookingId(bookingId);

            if (userId == Guid.Empty)
                throw new ArgumentException("Invalid user ID.");

            var booking = await _queryBookingRepository.GetByIdAsync(bookingId)
                    ?? throw new KeyNotFoundException($"Booking with ID {bookingId} was not found.");

            if (booking.Status == BookingStatus.Blocked
                || booking.Status == BookingStatus.Cancelled
                || booking.Status == BookingStatus.Completed)
            {
                throw new InvalidOperationException("Cannot cancel a blocked, cancelled, or completed booking.");
            }

            await CheckHostAuthorization(booking.ListingId, userId, userRole);

            await _commandBookingRepository.CancelBookingAsync(bookingId);

            await _notificationService.SendNotificationAsync(new NotificationCreationDTO
            {
                UserId = booking.GuestId,
                Title = "Booking Cancelled by Host",
                Message = $"Unfortunately, your booking from {booking.CheckInDate:MMM dd} to {booking.CheckOutDate:MMM dd} has been cancelled by the host."
            });

            await _notificationService.SendNotificationAsync(new NotificationCreationDTO
            {
                UserId = userId,
                Title = "Booking Cancellation Confirmed",
                Message = $"You have successfully cancelled the booking for the dates {booking.CheckInDate:MMM dd} to {booking.CheckOutDate:MMM dd}."
            });
        }


        private void ValidateBookingId(Guid bookingId)
        {
            if (bookingId == Guid.Empty)
                throw new ArgumentException("Invalid booking ID.");
        }
        private async Task BookingDTOValidator(BookingDTO booking)
        {
            var validationResult = await _bookingDTOValidator.ValidateAsync(booking);
            if (!validationResult.IsValid)
                throw new ValidationException(validationResult.Errors);
        }
        private async Task CheckHostAuthorization(Guid listingId, Guid userId, string userRole)
        {
            if (string.IsNullOrEmpty(userRole) || !userRole.Contains("Host", StringComparison.OrdinalIgnoreCase))
                throw new UnauthorizedAccessException("Host role is required to perform this action.");

            var listing = await _queryListingRepository.GetByIdAsync(listingId);
            if (listing == null)
                throw new KeyNotFoundException($"Property with ID {listingId} was not found.");

            if (listing.HostId != userId)
                throw new UnauthorizedAccessException("You do not have permission to modify a property that does not belong to you.");
        }
    }
}