using Application.Interfaces.Repositories.Listings.ListingBlocks;
using Domain.Entities.Listings;
using Domain.Enums.Bookings;
using Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories.ListingRepositories
{
    public class ListingBlockRepository(AppDbContext dbContext) : ICommandListingBlockRepository,
        IQueryListingBlockRepository
    {
        private readonly AppDbContext _dbContext = dbContext;

        public async Task CreateAsync(ListingBlock entity)
        {
            await _dbContext.ListingBlocks.AddAsync(entity);
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            await _dbContext.ListingBlocks.Where(b => b.Id == id)
                .ExecuteDeleteAsync();
        }

        public IQueryable<ListingBlock> GetAllAsync()
        {
            return _dbContext.ListingBlocks.AsNoTracking();
        }

        public async Task<ListingBlock?> GetByIdAsync(Guid id)
        {
            return await _dbContext.ListingBlocks.FindAsync(id);
        }

        public async Task<bool> HasOverlappingAsync(Guid listingId, DateTime startDate, DateTime endDate)
        {
            return await _dbContext.ListingBlocks
                .AnyAsync(b => b.ListingId == listingId
                            && startDate < b.EndDate
                            && endDate > b.StartDate);
        }

        public IQueryable<ListingBlock> GetActiveBlockForListingAsync(Guid listingId)
        {
            return _dbContext.ListingBlocks.AsNoTracking()
                            .Where(lb => lb.ListingId == listingId
                                && lb.EndDate >= DateTime.UtcNow.Date); 
        }

        public async Task UpdateAsync(Guid id, ListingBlock entity)
        {
            _dbContext.ListingBlocks.Update(entity);
            await _dbContext.SaveChangesAsync();
        }
    }
}