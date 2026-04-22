using Domain.Entities.Listings;

namespace Application.Interfaces.Repositories.Listings.Reviews
{
    public interface IQueryReviewRepository : IQueryRepository<Review>
    {
        Task<decimal> GetAvgReviewsAsync(Guid listingId);
        IQueryable<Review> GetAllReviewsFromListingAsync(Guid listingId);
    }
}
