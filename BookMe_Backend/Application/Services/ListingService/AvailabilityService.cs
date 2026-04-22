using Application.DTOs.ListingDTOs;
using Application.Interfaces.Repositories.Bookings;
using Application.Interfaces.Repositories.Listings.ListingBlocks;
using Application.Interfaces.Services.ListingServices;
using Microsoft.EntityFrameworkCore;

namespace Application.Services.ListingService
{
    public class AvailabilityService(IQueryListingBlockRepository queryListingBlockRepository,
        IQueryBookingRepository queryBookingRepository) : IAvailabilityService
    {
        private readonly IQueryBookingRepository _queryBookingRepository = queryBookingRepository;
        private readonly IQueryListingBlockRepository _queryListingBlockRepository = queryListingBlockRepository;

        public async Task<bool> IsListingAvailableAsync(Guid listingId, DateTime startDate, DateTime endDate, Guid? userId = null)
        {
            bool hasOverlappingBlock = await _queryListingBlockRepository
                .HasOverlappingAsync(listingId, startDate, endDate);

            if (hasOverlappingBlock)
                return false;

            bool hasOverlappingBooking = await _queryBookingRepository
                .HasOverlappingAsync(listingId, startDate, endDate, userId);
            
            if (hasOverlappingBooking)
                return false;

            return true;
        }

        public async Task<IEnumerable<BlockedDateRangeDTO>> GetBlockedDateRangesAsync(Guid listingId)
        {
            var blockedDateRanges = await _queryListingBlockRepository.GetActiveBlockForListingAsync(listingId)
                .Select(lb => new BlockedDateRangeDTO { 
                    Id = lb.Id, 
                    StartDate = lb.StartDate, 
                    EndDate = lb.EndDate,
                    IsManualBlock = true 
                })
                .ToListAsync();
        
            var bookingDateRanges = await _queryBookingRepository.GetActiveBookingsByListing(listingId)
                .Select(b => new BlockedDateRangeDTO { 
                    Id = b.Id, 
                    StartDate = b.CheckInDate, 
                    EndDate = b.CheckOutDate,
                    IsManualBlock = false 
                })
                .ToListAsync();

            var allBlockedDates = blockedDateRanges.Concat(bookingDateRanges).OrderBy(d => d.StartDate);
            return allBlockedDates; 
        }

    }
}
