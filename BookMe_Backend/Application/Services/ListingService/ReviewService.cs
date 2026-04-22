using Application.DTOs.ListingDTOs.ReviewDTOs;
using Application.Interfaces.Repositories.Bookings;
using Application.Interfaces.Repositories.Listings.Reviews;
using Application.Interfaces.Services.ListingServices;
using AutoMapper;
using Domain.Entities.Listings;
using Domain.Enums.Bookings;
using Microsoft.EntityFrameworkCore;

namespace Application.Services.ListingService
{
    public class ReviewService(IQueryReviewRepository reviewRepository,
        ICommandReviewRepository commandReviewRepository,
        IQueryBookingRepository queryBookingRepository,
        IMapper mapper) : IReviewService
    {
        private readonly IQueryReviewRepository _reviewRepository = reviewRepository;
        private readonly ICommandReviewRepository _commandReviewRepository = commandReviewRepository;
        private readonly IQueryBookingRepository _queryBookingRepository = queryBookingRepository;
        private readonly IMapper _mapper = mapper;
        
        public async Task CreateReviewAsync(ReviewCreationDTO review, Guid userId, string userRole)
        {
            await CheckGuestAuthorization(review.ListingId, userId, userRole);

            var reviewEntity = _mapper.Map<Review>(review);
            reviewEntity.GuestId = userId;

            await _commandReviewRepository.CreateAsync(reviewEntity);
        }

        public async Task UpdateReviewComment(Guid reviewId, string comment, Guid userId, string userRole)
        {
            if (string.IsNullOrWhiteSpace(comment))
                throw new ArgumentException("The comment cannot be empty or whitespace.", nameof(comment));

            await VerifyReviewOwnership(reviewId, userId, userRole);
            await _commandReviewRepository.UpdateCommentAsync(reviewId, comment);
        }

        public async Task UpdateRatingAsync(Guid reviewId, int rating, Guid userId, string userRole)
        {
            if (rating < 1 || rating > 5)
                throw new ArgumentOutOfRangeException(nameof(rating), "The rating must be between 1 and 5.");

            await VerifyReviewOwnership(reviewId, userId, userRole);
            await _commandReviewRepository.UpdateRateAsync(reviewId, rating);
        }

        public async Task DeleteReviewAsync(Guid reviewId, Guid userId, string userRole)
        {
            await VerifyReviewOwnership(reviewId, userId, userRole);
            await _commandReviewRepository.DeleteAsync(reviewId);
        }

        public async Task<IEnumerable<ReviewDTO>> GetAllReviewsFromListingAsync(Guid listingId)
        {
            var reviews = await _reviewRepository.GetAllReviewsFromListingAsync(listingId)
                .ToListAsync();

            return _mapper.Map<IEnumerable<ReviewDTO>>(reviews);
        }

        public async Task<decimal> GetReviewAverageAsync(Guid listingId)
        {
            var reviews = await _reviewRepository.GetAvgReviewsAsync(listingId);
            return reviews;
        }

        private async Task CheckGuestAuthorization(Guid listingId, Guid userId, string userRole)
        {
            if (userRole != "Guest")
                throw new UnauthorizedAccessException("Only guests can perform this action.");

            bool hasValidBooking = await _queryBookingRepository.GetAllByGuest(userId)
                .AnyAsync(b => b.ListingId == listingId && 
                               (b.Status == BookingStatus.Completed || b.Status == BookingStatus.Confirmed));
            
            if (!hasValidBooking)
                throw new InvalidOperationException("You can only review properties where you have a confirmed reservation or a completed stay.");
        }

        private async Task VerifyReviewOwnership(Guid reviewId, Guid userId, string userRole)
        {
            if (userRole != "Guest")
                throw new UnauthorizedAccessException("Only guests can perform this action.");

            var review = await _reviewRepository.GetByIdAsync(reviewId)
                ?? throw new KeyNotFoundException("Review not found.");

            if (review.GuestId != userId)
                throw new UnauthorizedAccessException("You are not authorized to modify this review.");
        }

    }
}
