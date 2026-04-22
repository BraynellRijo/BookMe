using Application.DTOs.ListingDTOs;

namespace Application.Interfaces.Services.ListingServices
{
    public interface IAvailabilityService
    {
        Task<bool> IsListingAvailableAsync(Guid listingId, DateTime startDate, DateTime endDate, Guid? userId = null);
        Task<IEnumerable<BlockedDateRangeDTO>> GetBlockedDateRangesAsync(Guid listingId);
    }
}
