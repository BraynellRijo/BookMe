using Application.DTOs.ListingDTOs;
using Application.DTOs.PhotoDTOs;
using Application.Interfaces.Repositories.Listings;
using Application.Interfaces.Repositories.Listings.ListingImages;
using Application.Interfaces.Services.ListingServices;
using AutoMapper;
using Domain.Enums.Listing;
using Microsoft.EntityFrameworkCore;

namespace Application.Services.ListingService
{
    public class ListingService(
        IQueryListingRepository queryRepository,
        IQueryListingImagesRepository queryImagesRepository,
        IFilterListingRepository filterListingRepository,
        IMapper mapper) : IListingService
    {
        private readonly IQueryListingRepository _queryRepository = queryRepository;
        private readonly IQueryListingImagesRepository _queryImagesRepository = queryImagesRepository;
        private readonly IFilterListingRepository _filterListingRepository = filterListingRepository;
        private readonly IMapper _mapper = mapper;

        public async Task<ListingDTO> GetListingByIdAsync(Guid id)
        {
            var listing = await _queryRepository.GetByIdAsync(id);
            if (listing == null) throw new KeyNotFoundException("The property is not available or does not exist.");

            return _mapper.Map<ListingDTO>(listing);
        }
        public async Task<string> GetMainImageUrlAsync(Guid listingId)
        {
            var images = await _queryImagesRepository.GetImagesByListingId(listingId).ToListAsync();

            if (images == null || !images.Any())
                return "assets/images/placeholder.png";

            var mainImage = images.FirstOrDefault(img => img.Order == 0);
            return mainImage?.Url ?? images.First().Url;
        }
        public async Task<IEnumerable<PhotoUploadResultDTO>> GetListingImagesAsync(Guid id)
        {
            var images = await _queryImagesRepository.GetImagesByListingId(id).ToListAsync();

            return images.Select(img => new PhotoUploadResultDTO
            {
                Url = img.Url,
                PublicId = img.PublicId
            });
        }

        public async Task<IEnumerable<ListingDTO>> GetNearbyListingsAsync(double latitude, double longitude, double radiusKm, PropertyType? type = null)
        {
            if (latitude < -90 || latitude > 90 || longitude < -180 || longitude > 180)
                throw new ArgumentException("Coordenadas geográficas inválidas.");

            if (radiusKm <= 0)
                throw new ArgumentException("El radio de búsqueda debe ser mayor a cero.");

            var query = _queryRepository.GetNearbyListings(latitude, longitude, radiusKm, type);

            var listings = await query.ToListAsync();

            return _mapper.Map<IEnumerable<ListingDTO>>(listings);
        }

        public async Task<IEnumerable<ListingDTO>> FilterByCityAsync(string city)
        {
            var result = await _filterListingRepository.FilterByCity(city)
                .ToListAsync();
            return _mapper.Map<IEnumerable<ListingDTO>>(result);
        }

        public async Task<IEnumerable<ListingDTO>> FilterByCountryAsync(string country)
        {
            var result = await _filterListingRepository.FilterByCountry(country)
                .ToListAsync();
            return _mapper.Map<IEnumerable<ListingDTO>>(result);
        }

        public async Task<IEnumerable<ListingDTO>> FilterByPricePerNightRangeAsync(decimal minPrice, decimal maxPrice)
        {
            var result = await _filterListingRepository.FilterByPricePerNight(minPrice, maxPrice)
                .ToListAsync();
            return _mapper.Map<IEnumerable<ListingDTO>>(result);
        }

        public async Task<IEnumerable<ListingDTO>> FilterByTypeAsync(PropertyType type)
        {
            var result = await _filterListingRepository.FilterByType(type)
                .ToListAsync();
            return _mapper.Map<IEnumerable<ListingDTO>>(result);
        }

        public async Task<IEnumerable<ListingDTO>> FilterByTitleAsync(string title)
        {
            var result = await _filterListingRepository.FilterByTitle(title)
                .ToListAsync();
            return _mapper.Map<IEnumerable<ListingDTO>>(result);
        }

        public async Task<IEnumerable<ListingDTO>> FilterByCapacityAsync(int minGuests, int maxGuests)
        {
            var result = await _filterListingRepository.FilterByCapacity(minGuests, maxGuests)
                .ToListAsync();
            return _mapper.Map<IEnumerable<ListingDTO>>(result);
        }

        public async Task<IEnumerable<ListingDTO>> FilterByDateRangeAsync(DateTime checkIn, DateTime checkOut)
        {
            var result = await _filterListingRepository.FilterByDateRange(checkIn, checkOut)
                .ToListAsync();
            return _mapper.Map<IEnumerable<ListingDTO>>(result);
        }

        public async Task<IEnumerable<ListingDTO>> FilterByAmenitiesAsync(List<Guid> amenityIds)
        {
            var result = await _filterListingRepository.FilterByAmenities(amenityIds)
                .ToListAsync();
            return _mapper.Map<IEnumerable<ListingDTO>>(result);
        }
    }
}