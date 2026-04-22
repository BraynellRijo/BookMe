using Domain.Entities.Listings;
using Domain.Enums.Listing;

namespace Application.Interfaces.Repositories.Listings
{
    public interface IFilterListingRepository
    {
        IQueryable<Listing> FilterByDateRange(DateTime checkIn, DateTime checkOut);
        IQueryable<Listing> FilterByTitle(string title);
        IQueryable<Listing> FilterByType(PropertyType type);
        IQueryable<Listing> FilterByCity(string city);
        IQueryable<Listing> FilterByCountry(string country);
        IQueryable<Listing> FilterByPricePerNight(decimal minPrice, decimal maxPrice);
        IQueryable<Listing> FilterByAmenities(List<Guid> amenityIds);
        IQueryable<Listing> FilterByCapacity(int minCapacity, int maxCapacity);
    }
}
