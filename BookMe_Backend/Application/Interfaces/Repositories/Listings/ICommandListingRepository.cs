using Domain.Entities.Listings;
using Domain.Enums.Listing;

namespace Application.Interfaces.Repositories.Listings
{
    public interface ICommandListingRepository : ICommandRepository<Listing>
    {
        Task UpdateGeneralInfoAsync(Guid listingId, string title, string description, PropertyType propertyType);
        Task UpdatePriceRulesAsync(Guid listingId, ListingPricingRules pricingRules);
        Task UpdateLocationAsync(Guid listingId, ListingLocation location);
        Task UpdateCapacityAsync(Guid listingId, ListingCapacity capacity);

        Task AddAmenityAsync(Guid listingId, Guid amenityId);
        Task RemoveAmenityAsync(Guid listingId, Guid amenityId);
        
        Task UpdateAvailabilityAsync(Guid listingId, bool isAvailable);
    }
}
