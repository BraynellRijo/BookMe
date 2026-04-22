using Application.DTOs.ListingDTOs.ReviewDTOs;
using Application.Interfaces.Services.ListingServices;
using Application.Services.ListingService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewsController(IReviewService reviewService, IHttpContextAccessor httpContextAccessor) : ControllerBase
    {
        private readonly IReviewService _reviewService = reviewService;
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

        [HttpPost("Create-Review")]
        [Authorize(Roles = "Guest")]
        public async Task<IActionResult> CreateReview(ReviewCreationDTO reviewCreationDTO)
        {
            Guid userId = GetUserId();
            string userRole = GetUserRole();
            await _reviewService.CreateReviewAsync(reviewCreationDTO, userId, userRole);
            return CreatedAtAction(nameof(CreateReview), reviewCreationDTO);
        }

        [HttpGet("Listing-Rating/{listingId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetListingRating(Guid listingId)
        {
            var reviews = await _reviewService.GetReviewAverageAsync(listingId);
            return Ok(new { overallRating = reviews });
        }

        [HttpGet("Listing-Reviews/{listingId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetListingReviews(Guid listingId)
        {
            var reviews = await _reviewService.GetAllReviewsFromListingAsync(listingId);
            return Ok(reviews);
        }

        [HttpPatch("Update-Comment/{reviewId}")]
        [Authorize(Roles = "Guest")]
        public async Task<IActionResult> UpdateComment(Guid reviewId, [FromBody] string comment)
        {
            Guid userId = GetUserId();
            string userRole = GetUserRole();
            await _reviewService.UpdateReviewComment(reviewId, comment, userId, userRole);
            return NoContent();
        }

        [HttpPatch("Update-Rating/{reviewId}")]
        [Authorize(Roles = "Guest")]
        public async Task<IActionResult> UpdateRating(Guid reviewId, [FromBody] int rating)
        {
            Guid userId = GetUserId();
            string userRole = GetUserRole();
            await _reviewService.UpdateRatingAsync(reviewId, rating, userId, userRole);
            return NoContent();
        }

        // Delete reviews
        [HttpDelete("Delete-Review/{reviewId}")]
        [Authorize(Roles = "Guest")]
        public async Task<IActionResult> DeleteReview(Guid reviewId)
        {
            Guid userId = GetUserId();
            string userRole = GetUserRole();
            await _reviewService.DeleteReviewAsync(reviewId, userId, userRole);
            return NoContent();
        }

        private Guid GetUserId()
        {
            var userIdFromToken = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                               ?? User.FindFirst("sub")?.Value
                               ?? User.Claims.FirstOrDefault(c => c.Type == "id")?.Value;

            if (string.IsNullOrEmpty(userIdFromToken) || !Guid.TryParse(userIdFromToken, out var userId))
                throw new UnauthorizedAccessException("El ID de usuario no existe en el Token o su formato Guid es inválido.");

            return userId;
        }

        private string GetUserRole()
        {
            var userRole = _httpContextAccessor.HttpContext?.User?.Claims
                .FirstOrDefault(c => c.Type == ClaimTypes.Role || c.Type == "Role")?.Value;
            return userRole ?? string.Empty;
        }
    }
}