using Microsoft.EntityFrameworkCore;
using FluentValidation;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Domain.Entities.Bookings;
using Domain.Enums.Bookings;
using Application.Interfaces.Services.NotificationServices;
using Application.Interfaces.Services.EmailServices; 
using Application.DTOs.BookingDTOs;
using Application.DTOs.NotificationDTOs;
using Application.Interfaces.Repositories.Bookings;
using Application.Interfaces.Repositories.Listings;
using Application.Interfaces.Services.BookingServices;
using System.Transactions;
using Application.Interfaces.Services.ListingServices;

namespace Application.Services.BookingService
{
    public class GuestBookingService(
        ICommandBookingRepository commandBookingRepository,
        IQueryBookingRepository queryBookingRepository,
        IQueryListingRepository queryListingRepository,
        IAvailabilityService availabilityService,
        IMapper mapper,
        IValidator<BookingCreationDTO> bookingCreationDTOValidator,
        IValidator<BookingDTO> bookingDTOValidator,
        INotificationService notificationService,  
        IEmailServices emailServices)             
        : IGuestBookingService
    {
        private readonly ICommandBookingRepository _commandBookingRepository = commandBookingRepository;
        private readonly IQueryBookingRepository _queryBookingRepository = queryBookingRepository;
        private readonly IQueryListingRepository _queryListingRepository = queryListingRepository;
        private readonly IAvailabilityService _availabilityService = availabilityService;
        private readonly IMapper _mapper = mapper;
        private readonly IValidator<BookingCreationDTO> _bookingCreationDTOValidator = bookingCreationDTOValidator;
        private readonly IValidator<BookingDTO> _bookingDTOValidator = bookingDTOValidator;
        private readonly INotificationService _notificationService = notificationService;
        private readonly IEmailServices _emailServices = emailServices;

        public async Task CreateBookingAsync(BookingCreationDTO bookingCreationDTO, Guid guestId)
        {
            await BookingCreationValidator(bookingCreationDTO);

            var listing = await _queryListingRepository.GetByIdAsync(bookingCreationDTO.ListingId)
                ?? throw new KeyNotFoundException("The requested property was not found.");

            if(listing.HostId == guestId)
                throw new InvalidOperationException("You cannot book your own property.");

            var booking = _mapper.Map<Booking>(bookingCreationDTO);
            booking.GuestId = guestId;
            booking.Status = BookingStatus.Confirmed;

            booking.TotalNights = GetTotalNights(booking.CheckInDate, booking.CheckOutDate);
            decimal listingPricePerNight = await _queryListingRepository.GetListingPricePerNightAsync(booking.ListingId);

            booking.AccomodationCost = CalculateAccomodationCost(booking.TotalNights, listingPricePerNight);
            booking.CleaningFee = await _queryListingRepository.GetListingCleaningFeeAsync(booking.ListingId);
            booking.TotalPrice = booking.AccomodationCost + booking.CleaningFee;

            await ConcurrencyHandler(booking);

            var notification = new NotificationCreationDTO
            {
                UserId = listing.HostId, 
                Title = "New Booking Received!",
                Message = $"You have a new booking from {booking.CheckInDate:MMM dd, yyyy} to {booking.CheckOutDate:MMM dd, yyyy}. Total Nights: {booking.TotalNights}."
            };
            await _notificationService.SendNotificationAsync(notification);

        }

        public async Task CancelBooking(Guid bookingId, Guid userId)
        {
            ValidateBookingId(bookingId);

            var booking = await _queryBookingRepository.GetByIdAsync(bookingId)
                    ?? throw new KeyNotFoundException($"Booking with ID {bookingId} was not found.");

            if (userId != booking.GuestId)
                throw new UnauthorizedAccessException("You are not authorized to cancel this booking.");

            await _commandBookingRepository.CancelBookingAsync(bookingId);

            var listing = await _queryListingRepository.GetByIdAsync(booking.ListingId);

            if (listing != null)
            {
                await _notificationService.SendNotificationAsync(new NotificationCreationDTO
                {
                    UserId = listing.HostId,
                    Title = "Booking Cancelled",
                    Message = $"A guest has cancelled their booking for dates {booking.CheckInDate:MMM dd} to {booking.CheckOutDate:MMM dd}."
                });
            }

            await _notificationService.SendNotificationAsync(new NotificationCreationDTO
            {
                UserId = booking.GuestId,
                Title = "Booking Cancellation Confirmed",
                Message = $"Your booking cancellation for the dates {booking.CheckInDate:MMM dd} to {booking.CheckOutDate:MMM dd} has been processed successfully."
            });
        }

        public async Task<IEnumerable<BookingDTO>> GetGuestBookingsAsync(Guid guestId)
        {
            if (guestId == Guid.Empty)
                throw new ArgumentException("The guest ID cannot be empty.", nameof(guestId));

            var bookings = _queryBookingRepository.GetAllByGuest(guestId);
            var bookingsDTO = await bookings.ProjectTo<BookingDTO>(_mapper.ConfigurationProvider).ToListAsync();

            return bookingsDTO;
        }

        public async Task UpdateStatusCompleted()
        {
            var existingBookings = await _queryBookingRepository.GetAllAsync()
                .Where(b => b.Status == BookingStatus.Confirmed && b.CheckOutDate <= DateTime.UtcNow)
                .ToListAsync();

            if(!existingBookings.Any())
                return;

            foreach (var booking in existingBookings)
            {
                await _commandBookingRepository.UpdateStatusAsync(booking.Id, BookingStatus.Completed);
                await _notificationService.SendNotificationAsync(new NotificationCreationDTO
                    {
                        UserId = booking.GuestId,
                        Title = "Trip Completed - Please Leave a Review",
                        Message = "We hope you enjoyed your stay! Please take a moment to leave a review for your host."
                    }
                );
         
            }
        }


        //Helper Methods
        private async Task ConcurrencyHandler(Booking booking)
        {
            var transactionOption = new TransactionOptions
            {
                IsolationLevel = IsolationLevel.Serializable,
                Timeout = TimeSpan.FromSeconds(10)
            };

            try
            {
                using var scope = new TransactionScope(TransactionScopeOption.Required, transactionOption, TransactionScopeAsyncFlowOption.Enabled);
                var isAvailable = await _availabilityService.IsListingAvailableAsync(booking.ListingId, booking.CheckInDate, booking.CheckOutDate);

                if (!isAvailable)
                    throw new InvalidOperationException("The property is not available for the selected dates.");

                await _commandBookingRepository.CreateAsync(booking);
                scope.Complete();
            }
            catch (Exception ex) when (ex is DbUpdateException || ex.Message.Contains("deadlock"))
            {
                throw new InvalidOperationException("The property is no longer available for the selected dates. Another user just booked it.");
            }
        }

        private int GetTotalNights(DateTime checkInDate, DateTime checkOutDate)
        {
            return (checkOutDate - checkInDate).Days;
        }
        private decimal CalculateAccomodationCost(int nights, decimal nightlyRate)
        {
            return nights * nightlyRate;
        }
        private void ValidateBookingId(Guid bookingId)
        {
            if (bookingId == Guid.Empty)
                throw new ArgumentException("Invalid booking ID.");
        }
        private async Task BookingCreationValidator(BookingCreationDTO bookingCreationDTO)
        {
            var validationResult = await _bookingCreationDTOValidator.ValidateAsync(bookingCreationDTO);
            if (!validationResult.IsValid)
                throw new ValidationException(validationResult.Errors);
        }
        private async Task BookingDTOValidator(BookingDTO booking)
        {
            var validationResult = await _bookingDTOValidator.ValidateAsync(booking);
            if (!validationResult.IsValid)
                throw new ValidationException(validationResult.Errors);
        }
    }
}