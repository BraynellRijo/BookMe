using Application.Interfaces.Repositories.Listings.Reviews;
using Domain.Entities.Listings;
using Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories.ListingRepositories
{
    public class ReviewRepository(AppDbContext dbContext) : IQueryReviewRepository,
        ICommandReviewRepository
    {
        private readonly AppDbContext _dbContext = dbContext;
        public async Task CreateAsync(Review entity)
        {
            await _dbContext.Reviews.AddAsync(entity);
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            await _dbContext.Reviews.Where(r => r.Id == id)
                .ExecuteDeleteAsync();
        }

        public IQueryable<Review> GetAllAsync()
        {
            var reviews = _dbContext.Reviews.AsNoTracking();

            return reviews;
        }
        public IQueryable<Review> GetAllReviewsFromListingAsync(Guid listingId)
        {
            var reviews = _dbContext.Reviews.AsNoTracking()
                .Where(r => r.ListingId == listingId);
            return reviews;
        }

        public async Task<decimal> GetAvgReviewsAsync(Guid listingId)
        {
            var avg = await _dbContext.Reviews
                   .Where(r => r.ListingId == listingId)
                   .AverageAsync(r => (decimal?)r.Rating) ?? 0m;
            return Math.Round(avg, 1);
        }

        public async Task<Review> GetByIdAsync(Guid id)
        {
            return await _dbContext.Reviews
                .FirstOrDefaultAsync(r => r.Id == id); ;
        }

        public async Task UpdateAsync(Guid id, Review entity)
        {
            await _dbContext.Reviews.Where(r => r.Id == id)
                .ExecuteUpdateAsync(r => r
                    .SetProperty(r => r.GuestId, entity.GuestId)
                    .SetProperty(r => r.ListingId, entity.ListingId)
                    .SetProperty(r => r.Rating, entity.Rating)
                    .SetProperty(r => r.Comment, entity.Comment));
        }

        public async Task UpdateCommentAsync(Guid reviewId, string comment)
        {
            await _dbContext.Reviews.Where(r => r.Id == reviewId)
                .ExecuteUpdateAsync(r => r
                    .SetProperty(r => r.Comment, comment));
        }

        public async Task UpdateRateAsync(Guid reviewId, int rate)
        {
            await _dbContext.Reviews.Where(r => r.Id == reviewId)
                .ExecuteUpdateAsync(r => r
                    .SetProperty(r => r.Rating, rate));
        }
    }
}
