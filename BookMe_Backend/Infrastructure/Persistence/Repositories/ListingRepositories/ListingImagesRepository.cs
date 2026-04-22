using Application.Interfaces.Repositories.Listings.ListingImages;
using Domain.Entities.Listings;
using Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories.ListingRepositories
{
    public class ListingImagesRepository(AppDbContext dbContext) : ICommandListingImagesRepository, IQueryListingImagesRepository
    {
        private readonly AppDbContext _context = dbContext;

        public async Task CreateAsync(ListingImage entity)
        {
            await _context.ListingImages.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var entity = await _context.ListingImages.FindAsync(id);

                _context.ListingImages.Remove(entity);
                await _context.SaveChangesAsync();
        }

        public IQueryable<ListingImage> GetAllAsync()
        {
            return _context.ListingImages.AsNoTracking();
        }

        public async Task<ListingImage?> GetByIdAsync(Guid id)
        {
            return await _context.ListingImages.FindAsync(id);
        }

        public async Task UpdateAsync(Guid id, ListingImage entity)
        {
            var existingEntity = await _context.ListingImages.FindAsync(id);

            _context.Entry(existingEntity).CurrentValues.SetValues(entity);
            await _context.SaveChangesAsync();
        }

        public IQueryable<ListingImage> GetImagesByListingId(Guid listingId)
        {
            return _context.ListingImages
                .Where(l => l.ListingId == listingId)
                .AsNoTracking();
        }

        public async Task CreateRangeAsync(IEnumerable<ListingImage> entities)
        {
            await _context.ListingImages.AddRangeAsync(entities);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteRangeAsync(IEnumerable<ListingImage> entities)
        {
            _context.ListingImages.RemoveRange(entities);
            await _context.SaveChangesAsync();
        }

    }
}