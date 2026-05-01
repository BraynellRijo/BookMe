using Application.Interfaces.Repositories.Listings;
using Domain.Entities.Listings;
using Domain.Enums.Bookings;
using Domain.Enums.Listing;
using Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite;
using NetTopologySuite.Geometries;

namespace Infrastructure.Persistence.Repositories.ListingRepositories
{
    public class ListingRepository(AppDbContext dbContext) : IQueryListingRepository,
        ICommandListingRepository,
        IFilterListingRepository
    {
        private readonly AppDbContext _dbContext = dbContext;


        public async Task CreateAsync(Listing entity)
        {
            _dbContext.Listings.Add(entity);
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            await _dbContext.Listings.Where(l => l.Id == id)
                .ExecuteDeleteAsync();
        }

        //Gets
        public IQueryable<Listing> GetAllAsync()
        {
            var listings = _dbContext.Listings
                .Include(l => l.Images)
                .Include(l => l.Reviews)
                .AsNoTracking();
            return listings;
        }
        public async Task<Listing> GetByIdAsync(Guid id)
        {
            return await _dbContext.Listings
                .IgnoreQueryFilters()
                .Include(l => l.Images)
                .Include(l => l.Amenities)
                .Include(l => l.Reviews)
                .FirstOrDefaultAsync(l => l.Id == id);
        }

        public async Task<Listing> GetLastListing()
        {
            return await _dbContext.Listings
                .IgnoreQueryFilters()
                .Include(l => l.Images)
                .Include(l => l.Amenities)
                .Include(l => l.Reviews)
                .OrderDescending() // Fix it
                .FirstOrDefaultAsync();
        }

        public async Task<Listing?> GetListingForEditAsync(Guid id)
        {
            return await _dbContext.Listings
                .IgnoreQueryFilters()
                .Include(l => l.Images)
                .Include(l => l.Amenities)
                .FirstOrDefaultAsync(l => l.Id == id);
        }

        public IQueryable<Listing> GetListingsByHost(Guid HostId)
        {
            var listings = _dbContext.Listings.AsNoTracking()
                .IgnoreQueryFilters()
                .Include(l => l.Images)
                .Include(l => l.Reviews)
                .Where(l => l.HostId == HostId);
            return listings;
        }

        public async Task<decimal> GetListingPricePerNightAsync(Guid listingId)
        {
            return await _dbContext.Listings.AsNoTracking()
                .Where(l => l.Id == listingId)
                .Select(l => (decimal)l.PricingRules.PricePerNight)
                .FirstOrDefaultAsync();
        }

        public async Task<decimal> GetListingCleaningFeeAsync(Guid listingId)
        {
            return await _dbContext.Listings
                .Where(l => l.Id == listingId)
                .Select(l => (decimal)l.PricingRules.CleaningFee)
                .FirstOrDefaultAsync();
        }

        //Updates
        public async Task UpdateAsync(Guid id, Listing entity)
        {
            _dbContext.Listings.Update(entity);

            await _dbContext.SaveChangesAsync();
        }

        public Task UpdateGeneralInfoAsync(Guid listingId, string title, string description, PropertyType propertyType)
        {
            return _dbContext.Listings
                .Where(l => l.Id == listingId)
                .ExecuteUpdateAsync(l => l
                    .SetProperty(p => p.Title, title)
                    .SetProperty(p => p.Description, description)
                    .SetProperty(p => p.Type, propertyType)
            );
        }

        public async Task UpdateCapacityAsync(Guid listingId, ListingCapacity capacity)
        {
            await _dbContext.Listings.Where(l => l.Id == listingId)
                .ExecuteUpdateAsync(l => l
                    .SetProperty(p => p.Capacity, capacity)
            );
        }

        public async Task UpdateLocationAsync(Guid listingId, ListingLocation location)
        {
            await _dbContext.Listings.
                Where(l => l.Id == listingId).ExecuteUpdateAsync(l => l
                    .SetProperty(p => p.Location, location)
            );
        }

        public async Task UpdatePriceRulesAsync(Guid listingId, ListingPricingRules pricingRules)
        {
            await _dbContext.Listings.Where(l => l.Id == listingId)
                .ExecuteUpdateAsync(l => l
                    .SetProperty(p => p.PricingRules, pricingRules)
            );
        }

        //Availability
        public Task UpdateAvailabilityAsync(Guid listingId, bool isAvailable)
        {
            return _dbContext.Listings
                .IgnoreQueryFilters()
                .Where(l => l.Id == listingId)
                .ExecuteUpdateAsync(l => l
                    .SetProperty(p => p.IsAvailable, isAvailable)
            );
        }

        //Amenities
        public async Task AddAmenityAsync(Guid listingId, Guid amenityId)
        {
            var listing = await _dbContext.Listings
                .Include(l => l.Amenities)
                .FirstOrDefaultAsync(l => l.Id == listingId);

            var existingAmenity = await _dbContext.Amenities
                .FindAsync(amenityId);

            listing.Amenities.Add(existingAmenity);
            await _dbContext.SaveChangesAsync();
        }

        public async Task RemoveAmenityAsync(Guid listingId, Guid amenityId)
        {
            var listing = await _dbContext.Listings
                .Include(l => l.Amenities)
                .FirstOrDefaultAsync(l => l.Id == listingId);

            var existingAmenity = await _dbContext.Amenities
                .FindAsync(amenityId);

            listing.Amenities.Remove(existingAmenity);
            await _dbContext.SaveChangesAsync();
        }


        //Filters
        public IQueryable<Listing> FilterByDateRange(DateTime checkIn, DateTime checkOut)
        {
            var listings = _dbContext.Listings.AsNoTracking()
                .Include(l => l.Images)
                .Include(l => l.Reviews)
                .Where(l => !_dbContext.Bookings
                    .Any(b => (b.ListingId == l.Id)
                        && (b.Status == BookingStatus.Confirmed
                            || b.Status == BookingStatus.Blocked
                            || b.Status == BookingStatus.Pending)
                        && (b.CheckInDate < checkOut && b.CheckOutDate > checkIn)));

            return listings;
        }

        public IQueryable<Listing> FilterByTitle(string title)
        {
            var listings = _dbContext.Listings.AsNoTracking()
                .Include(l => l.Images)
                .Include(l => l.Reviews)
                .Where(l => l.Title.Contains(title));
            return listings;
        }
        public IQueryable<Listing> FilterByCity(string city)
        {
            var listing = _dbContext.Listings
                .Include(l => l.Images)
                .Include(l => l.Reviews)
                .Where(l => l.Location.City.Contains(city));

            return listing;
        }

        public IQueryable<Listing> FilterByCountry(string country)
        {
            var listing = _dbContext.Listings.AsNoTracking()
                .Include(l => l.Images)
                .Include(l => l.Reviews)
                .Where(l => l.Location.Country.Contains(country));

            return listing;
        }


        public IQueryable<Listing> FilterByPricePerNight(decimal minPrice, decimal maxPrice)
        {
            var listings = _dbContext.Listings.AsNoTracking()
                .Include(l => l.Images)
                .Include(l => l.Reviews)
                .Where(l => l.PricingRules.PricePerNight >= minPrice
                        && l.PricingRules.PricePerNight <= maxPrice);

            return listings;
        }

        public IQueryable<Listing> FilterByCapacity(int minCapacity, int maxCapacity)
        {
            var listings = _dbContext.Listings.AsNoTracking()
                .Include(l => l.Images)
                .Include(l => l.Reviews)
                .Where(l => l.Capacity.MaxGuests >= minCapacity
                        && l.Capacity.MaxGuests <= maxCapacity);
            return listings;
        }
        public IQueryable<Listing> FilterByAmenities(List<Guid> amenityIds)
        {
            var listings = _dbContext.Listings.AsNoTracking()
                .Include(l => l.Images)
                .Include(l => l.Reviews)
                .Where(l => l.Amenities.Any(a => amenityIds.Contains(a.Id)));
            return listings;
        }

        public IQueryable<Listing> FilterByType(PropertyType type)
        {
            var listings = _dbContext.Listings.AsNoTracking()
                .Include(l => l.Images)
                .Include(l => l.Reviews)
                .Where(l => l.Type == type);
            return listings;
        }
        public IQueryable<Listing> GetNearbyListings(double latitude, double longitude, double radiusInKm, PropertyType? type = null)
        {
            var geometryFactory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);
            var userLocation = geometryFactory.CreatePoint(new Coordinate(longitude, latitude));
            var userRadius = radiusInKm * 1000;

            var query = _dbContext.Listings.AsNoTracking()
                .Include(l => l.Images)
                .Include(l => l.Reviews)
                .Where(l => EF.Property<Point>(l.Location, "SpatialLocation").Distance(userLocation) <= userRadius);

            if (type.HasValue)
                query = query.Where(l => l.Type == type.Value);

            return query.OrderBy(l => EF.Property<Point>(l.Location, "SpatialLocation").Distance(userLocation));
        }
        public async Task<IEnumerable<ListingImage>> GetImagesByListingIdAsync(Guid listingId)
        {
            /* 
               Fetch all images for the given listing ID using AsNoTracking() for better performance.
               Ordering by the 'Order' property ensures we get them in the intended sequence.
            */
            return await _dbContext.ListingImages
                .AsNoTracking()
                .Where(img => img.ListingId == listingId)
                .OrderBy(img => img.Order)
                .ToListAsync();
        }
    }
}
