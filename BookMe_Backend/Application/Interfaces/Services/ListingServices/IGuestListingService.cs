using Application.DTOs.ListingDTOs;
using Domain.Enums.Listing;

namespace Application.Interfaces.Services.ListingServices
{
    public interface IGuestListingService
    {
        Task<ListingDTO> GetListingById(Guid id);
        Task<IEnumerable<ListingDTO>> GetNearbyListingsAsync(double latitude, double longitude, double radiusInKm);
        Task<IEnumerable<ListingDTO>> FilterByDateRange(DateTime checkIn, DateTime checkOut);
        Task<IEnumerable<ListingDTO>> FilterByTitle(string title);
        Task<IEnumerable<ListingDTO>> FilterByCity(string city);
        Task<IEnumerable<ListingDTO>> FilterByCountry(string country);
        Task<IEnumerable<ListingDTO>> FilterByPricePerNightRange(decimal minPrice, decimal maxPrice);
        Task<IEnumerable<ListingDTO>> FilterByCapacity(int minGuests, int maxGuests);
        Task<IEnumerable<ListingDTO>> FilterByAmenities(List<Guid> amenityIds);
        Task<IEnumerable<ListingDTO>> FilterByType(PropertyType propertyType);
    }
}
