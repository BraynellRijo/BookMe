using Application.DTOs.ListingDTOs;
using Application.DTOs.PhotoDTOs;
using Domain.Entities.Listings;
using Microsoft.AspNetCore.Http;

namespace Application.Interfaces.Services.ListingServices
{
    public interface IHostListingService
    {
        Task<ListingDTO> GetListingByIdForHost(Guid id, Guid hostId);
        Task<IEnumerable<ListingDTO>> GetListingsByHost(Guid hostId);
        Task<double> GetHostAverageRatingAsync(Guid hostId);
        Task<IEnumerable<PhotoUploadResultDTO>> GetListingImagesAsync(Guid listingId, Guid hostId);
        Task<Guid> CreateListing(ListingCreationDTO listingDTO, Guid hostId);
        Task DeleteListing(Guid id, Guid userId, string userRole);
        Task UploadListingImages(Guid listingId, Guid userId, string userRole, IEnumerable<IFormFile> images);
        Task UpdateFullListingAsync(Guid listingId, Guid userId, string userRole, ListingUpdateDTO updateDTO);

        Task UpdateCapacity(Guid listingId, Guid userId, string userRole, ListingCapacity capacity);
        Task UpdateLocation(Guid listingId, Guid userId, string userRole, ListingLocation location);
        Task UpdatePriceAndRules(Guid listingId, Guid userId, string userRole, ListingPricingRules pricingRules);

        Task ToggleListingAvailability(Guid listingId, Guid userId, string userRole, bool availability);
        Task BlockDatesAsync(Guid listingId, DateTime startDate, DateTime endDate, string reason, Guid userId, string userRole);
        Task UnblockDatesAsync(Guid blockId, Guid listingId, Guid userId, string userRole);

        Task AddAmenities(Guid listingId, Guid amenityId, Guid userId, string userRole);
        Task RemoveAmenities(Guid listingId, Guid amenityId, Guid userId, string userRole);

    }
}
