using Application.DTOs.ListingDTOs;

namespace Application.Interfaces.Services.ListingServices
{
    public interface IAmenityService
    {
        Task<IEnumerable<AmenityDTO>> GetAllAsync();
        Task<AmenityDTO> GetByIdAsync(Guid id);
        Task CreateAsync(AmenityDTO amenityDto);
        Task UpdateAsync(Guid id, AmenityDTO amenityDto);
        Task DeleteAsync(Guid id);
    }
}