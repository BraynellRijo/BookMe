using Application.DTOs.ListingDTOs;
using Application.DTOs.PhotoDTOs;
using Domain.Enums.Listing;

namespace Application.Interfaces.Services.ListingServices
{
    public interface IListingService
    {
            Task<ListingDTO> GetLastListingAsync();
        Task<ListingDTO> GetListingByIdAsync(Guid id);
        Task<IEnumerable<PhotoUploadResultDTO>> GetListingImagesAsync(Guid listingId);
        Task<string> GetMainImageUrlAsync(Guid listingId);
        Task<IEnumerable<ListingDTO>> GetNearbyListingsAsync(double lat, double lng, double radiusKm, PropertyType? type = null);
        Task<IEnumerable<ListingDTO>> FilterByCityAsync(string city);
        Task<IEnumerable<ListingDTO>> FilterByCountryAsync(string country);
        Task<IEnumerable<ListingDTO>> FilterByPricePerNightRangeAsync(decimal minPrice, decimal maxPrice);
        Task<IEnumerable<ListingDTO>> FilterByTypeAsync(PropertyType type);
        Task<IEnumerable<ListingDTO>> FilterByTitleAsync(string title);
        Task<IEnumerable<ListingDTO>> FilterByCapacityAsync(int minGuests, int maxGuests);
        Task<IEnumerable<ListingDTO>> FilterByDateRangeAsync(DateTime checkIn, DateTime checkOut);
        Task<IEnumerable<ListingDTO>> FilterByAmenitiesAsync(List<Guid> amenityIds);
    }
}
