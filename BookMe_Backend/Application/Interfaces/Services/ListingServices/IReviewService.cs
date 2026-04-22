
using Application.DTOs.ListingDTOs.ReviewDTOs;
using Domain.Entities.Listings;

namespace Application.Interfaces.Services.ListingServices
{
    public interface IReviewService
    {
        Task CreateReviewAsync(ReviewCreationDTO review, Guid userId, string userRole);
        Task UpdateReviewComment(Guid reviewId, string comment, Guid userId, string userRole);
        Task UpdateRatingAsync(Guid reviewId, int rating, Guid userId, string userRole);
        Task DeleteReviewAsync(Guid reviewId, Guid userId, string userRole);
        Task<IEnumerable<ReviewDTO>> GetAllReviewsFromListingAsync(Guid listingId);
        Task<decimal> GetReviewAverageAsync(Guid listingId);
    }
}
