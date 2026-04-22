using Application.Interfaces.Repositories.Listings.Amenities;
using Domain.Entities.Listings;
using Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories.ListingRepositories
{
    public class AmenityRepository(AppDbContext dbContext) : IQueryAmenityRepository,
        ICommandAmenityRepository
    {
        private readonly AppDbContext _dbContext = dbContext;

        public async Task CreateAsync(Amenity entity)
        {
            await _dbContext.Amenities.AddAsync(entity);
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            await _dbContext.Amenities
                .Where(b => b.Id == id)
                .ExecuteDeleteAsync();
        }

        public  IQueryable<Amenity> GetAllAsync()
        {
            var amenities = _dbContext.Amenities;
            return amenities;
        }

        public async Task<Amenity> GetByIdAsync(Guid id)
        {
            return await _dbContext.Amenities
                .Where(a => a.Id == id)
                .FirstOrDefaultAsync();
        }

        public async Task UpdateAsync(Guid id, Amenity entity)
        {
            await _dbContext.Amenities
                .Where(a => a.Id == id)
                .ExecuteUpdateAsync(a => a
                    .SetProperty(p => p.Name, entity.Name)
                    .SetProperty(p => p.IconCode, entity.IconCode));
        }
    }
}
