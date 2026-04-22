using Application.DTOs.BookingDTOs;
using Application.Interfaces.Repositories.Bookings;
using Application.Interfaces.Repositories.Listings;
using Application.Interfaces.Services.ListingServices;
using FluentValidation;

namespace Application.Validators.BookingValidators
{
    public class BookingCreationDTOValidator : AbstractValidator<BookingCreationDTO>
    {
        public BookingCreationDTOValidator(IQueryListingRepository queryListingRepository, IAvailabilityService availabilityService)
        {
            RuleLevelCascadeMode = CascadeMode.Stop;

            RuleFor(x => x.ListingId)
                .NotEmpty().WithMessage("Listing ID is required.")
                .MustAsync(async (listingId, cancellation) =>
                {
                    var listing = await queryListingRepository.GetByIdAsync(listingId);
                    return listing != null;
                })
                .WithMessage("Listing does not exist.");

            RuleFor(x => x.CheckInDate)
                .GreaterThanOrEqualTo(DateTime.UtcNow.Date)
                .WithMessage("Check-in date must be today or in the future.");

            RuleFor(x => x.CheckOutDate)
                .GreaterThan(x => x.CheckInDate)
                .WithMessage("Check-out date must be after check-in date.");

            RuleFor(x => x.TotalGuests)
                .GreaterThan(0)
                .WithMessage("Number of guests must be at least 1.")
                .MustAsync(async (dto, totalGuests, cancellation) =>
                {
                    var listing = await queryListingRepository.GetByIdAsync(dto.ListingId);

                    if (listing == null) return false;

                    var maxGuests = listing.Capacity.MaxGuests;

                    return totalGuests <= maxGuests;
                })
                .WithMessage("Number of guests exceeds the maximum allowed for this listing.");

            RuleFor(x => x)
                .MustAsync(async (dto, cancellation) =>
                {
                    var isAvailable = await availabilityService.IsListingAvailableAsync(dto.ListingId,
                        dto.CheckInDate,
                        dto.CheckOutDate);

                    return isAvailable;
                })
                .WithMessage("The listing is not available for the selected dates.")
                .WithName("Availability");
        }
    }
}
