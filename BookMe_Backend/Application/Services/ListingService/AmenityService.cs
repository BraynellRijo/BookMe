using Application.DTOs.ListingDTOs;
using Application.Interfaces.Repositories.Listings.Amenities;
using Application.Interfaces.Services.ListingServices;
using AutoMapper;
using Domain.Entities.Listings;
using Microsoft.EntityFrameworkCore;

namespace Application.Services.ListingService
{
    public class AmenityService(
        IQueryAmenityRepository queryRepository,
        ICommandAmenityRepository commandRepository,
        IMapper mapper) : IAmenityService
    {
        private readonly IQueryAmenityRepository _queryRepository = queryRepository;
        private readonly ICommandAmenityRepository _commandRepository = commandRepository;
        private readonly IMapper _mapper = mapper;

        public async Task<IEnumerable<AmenityDTO>> GetAllAsync()
        {
            var amenities = await _queryRepository.GetAllAsync().ToListAsync();
            return _mapper.Map<IEnumerable<AmenityDTO>>(amenities);
        }

        public async Task<AmenityDTO> GetByIdAsync(Guid id)
        {
            var amenity = await _queryRepository.GetByIdAsync(id);
            if (amenity == null) throw new KeyNotFoundException("Amenity not found.");

            return _mapper.Map<AmenityDTO>(amenity);
        }

        public async Task CreateAsync(AmenityDTO amenityDto)
        {
            var amenity = _mapper.Map<Amenity>(amenityDto);
            await _commandRepository.CreateAsync(amenity);
        }

        public async Task UpdateAsync(Guid id, AmenityDTO amenityDto)
        {
            var amenity = _mapper.Map<Amenity>(amenityDto);
            await _commandRepository.UpdateAsync(id, amenity);
        }

        public async Task DeleteAsync(Guid id)
        {
            await _commandRepository.DeleteAsync(id);
        }
    }
}