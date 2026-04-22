using Domain.Entities.Listings;

namespace Application.Interfaces.Repositories.Listings.Reviews
{
    public interface ICommandReviewRepository : ICommandRepository<Review>
    {
        Task UpdateCommentAsync(Guid reviewId, string comment);
        Task UpdateRateAsync(Guid reviewId, int rate);
    }
}
