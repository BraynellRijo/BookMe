using Application.DTOs.ListingDTOs;
using Application.Interfaces.Repositories.Listings;
using Application.Interfaces.Services.ListingServices;
using AutoMapper;
using Domain.Enums.Listing;
using Microsoft.EntityFrameworkCore;

namespace Application.Services.ListingService
{
    public class GuestListingService(
        IQueryListingRepository queryRepository,
        IFilterListingRepository filterRepository,
        IMapper mapper) : IGuestListingService
    {
        private readonly IQueryListingRepository _queryRepository = queryRepository;
        private readonly IFilterListingRepository _filterRepository = filterRepository;
        private readonly IMapper _mapper = mapper;

        public async Task<ListingDTO> GetListingById(Guid id)
        {
            var listing = await _queryRepository.GetByIdAsync(id);

            if (listing == null)
                throw new KeyNotFoundException("The requested property was not found.");

            return _mapper.Map<ListingDTO>(listing);
        }

        public async Task<IEnumerable<ListingDTO>> FilterByDateRange(DateTime checkIn, DateTime checkOut)
        {
            if (checkIn > checkOut)
                throw new ArgumentException("Check-in date cannot be later than check-out date.");

            var listings = await _filterRepository.FilterByDateRange(checkIn, checkOut).ToListAsync();
            return _mapper.Map<IEnumerable<ListingDTO>>(listings);
        }

        public async Task<IEnumerable<ListingDTO>> FilterByTitle(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("Title cannot be null or empty.", nameof(title));

            var listings = await _filterRepository.FilterByTitle(title).ToListAsync();
            return _mapper.Map<IEnumerable<ListingDTO>>(listings);
        }

        public async Task<IEnumerable<ListingDTO>> FilterByCity(string city)
        {
            if (string.IsNullOrWhiteSpace(city))
                throw new ArgumentException("City cannot be null or empty.", nameof(city));

            var listings = await _filterRepository.FilterByCity(city).ToListAsync();
            return _mapper.Map<IEnumerable<ListingDTO>>(listings);
        }

        public async Task<IEnumerable<ListingDTO>> FilterByCountry(string country)
        {
            if (string.IsNullOrWhiteSpace(country))
                throw new ArgumentException("Country cannot be null or empty.", nameof(country));

            var listings = await _filterRepository.FilterByCountry(country).ToListAsync();
            return _mapper.Map<IEnumerable<ListingDTO>>(listings);
        }

        public async Task<IEnumerable<ListingDTO>> FilterByPricePerNightRange(decimal minPrice, decimal maxPrice)
        {
            if (minPrice < 0 || maxPrice < 0)
                throw new ArgumentException("Price cannot be negative.");

            if (minPrice > maxPrice)
                throw new ArgumentException("Minimum price cannot be greater than maximum price.");

            var listings = await _filterRepository.FilterByPricePerNight(minPrice, maxPrice).ToListAsync();
            return _mapper.Map<IEnumerable<ListingDTO>>(listings);
        }

        public async Task<IEnumerable<ListingDTO>> FilterByCapacity(int minGuests, int maxGuests)
        {
            if (minGuests < 0 || maxGuests < 0)
                throw new ArgumentException("Guest count cannot be negative.");

            if (minGuests > maxGuests)
                throw new ArgumentException("Minimum guest count cannot be greater than maximum guest count.");

            var listings = await _filterRepository.FilterByCapacity(minGuests, maxGuests).ToListAsync();
            return _mapper.Map<IEnumerable<ListingDTO>>(listings);
        }

        public async Task<IEnumerable<ListingDTO>> FilterByAmenities(List<Guid> amenityIds)
        {
            if (amenityIds == null || !amenityIds.Any())
                throw new ArgumentException("Amenity IDs cannot be null or empty.", nameof(amenityIds));

            var listings = await _filterRepository.FilterByAmenities(amenityIds).ToListAsync();
            return _mapper.Map<IEnumerable<ListingDTO>>(listings);
        }

        public async Task<IEnumerable<ListingDTO>> FilterByType(PropertyType type)
        {
            if (!Enum.IsDefined(typeof(PropertyType), type))
                throw new ArgumentException("Invalid property type.", nameof(type));

            var listings = await _filterRepository.FilterByType(type).ToListAsync();
            return _mapper.Map<IEnumerable<ListingDTO>>(listings);
        }

        public async Task<IEnumerable<ListingDTO>> GetNearbyListingsAsync(double latitude, double longitude, double radiusInKm)
        {
            {
                if (latitude < -90 || latitude > 90 || longitude < -180 || longitude > 180)
                    throw new ArgumentException("Coordenadas geográficas inválidas.");

                if (radiusInKm <= 0)
                    throw new ArgumentException("El radio de búsqueda debe ser mayor a cero.");

                var query = _queryRepository.GetNearbyListings(latitude, longitude, radiusInKm);

                var listings = await query.ToListAsync();

                return _mapper.Map<List<ListingDTO>>(listings);
            }
        }
    }
}