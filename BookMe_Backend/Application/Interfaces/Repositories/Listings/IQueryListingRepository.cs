using Domain.Entities.Listings;
using Domain.Enums.Listing;

namespace Application.Interfaces.Repositories.Listings
{
    public interface IQueryListingRepository : IQueryRepository<Listing>
    {
        Task<Listing?> GetLastListing();
        IQueryable<Listing> GetListingsByHost(Guid HostId);
        IQueryable<Listing> GetNearbyListings(double latitude, double longitude, double radiusInKm, PropertyType? type = null);
        Task<decimal> GetListingCleaningFeeAsync(Guid listingId);
        Task<decimal> GetListingPricePerNightAsync(Guid listingId);
        Task<IEnumerable<ListingImage>> GetImagesByListingIdAsync(Guid listingId);
        Task<Listing?> GetListingForEditAsync(Guid id);
    }
}
