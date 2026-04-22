using Application.DTOs.ListingDTOs;
using Application.Interfaces.Services.ListingServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Host")]
    public class HostListingController : ControllerBase
    {
        private readonly IHostListingService _listingService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public HostListingController(IHostListingService listingService, IHttpContextAccessor httpContextAccessor)
        {
            _listingService = listingService;
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpPost]
        public async Task<IActionResult> CreateListing([FromBody] ListingCreationDTO listingCreationDTO)
        {
            Guid hostId = GetUserId();
            Guid newListingId = await _listingService.CreateListing(listingCreationDTO, hostId);

            return StatusCode(StatusCodes.Status201Created, new
            {
                id = newListingId,
                message = "Property details saved successfully. Ready for image upload."
            });
        }

        [HttpPost("{id}/images")]
        public async Task<IActionResult> UploadListingImages(Guid id, [FromForm] IEnumerable<IFormFile> images)
        {
            Guid hostId = GetUserId();
            string userRole = GetUserRole();

            await _listingService.UploadListingImages(id, hostId, userRole, images);
            return Ok(new { message = "Images uploaded successfully." });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateListing(Guid id, [FromBody] ListingUpdateDTO updateDTO)
        {
            Guid hostId = GetUserId();
            string userRole = GetUserRole();

            await _listingService.UpdateFullListingAsync(id, hostId, userRole, updateDTO);
            return NoContent();
        }

        [HttpGet("my-listings")]
        public async Task<IActionResult> GetListingsByHost()
        {
            Guid hostId = GetUserId();
            var listings = await _listingService.GetListingsByHost(hostId);
            return Ok(listings);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetListingById(Guid id)
        {
            Guid hostId = GetUserId();
            var listing = await _listingService.GetListingByIdForHost(id, hostId);
            return Ok(listing);
        }

        [HttpPatch("{id}/availability")]
        public async Task<IActionResult> ToggleAvailability(Guid id, [FromBody] bool isAvailable)
        {
            Guid hostId = GetUserId();
            string userRole = GetUserRole();

            await _listingService.ToggleListingAvailability(id, hostId, userRole, isAvailable);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteListing(Guid id)
        {
            Guid hostId = GetUserId();
            string userRole = GetUserRole();

            await _listingService.DeleteListing(id, hostId, userRole);
            return NoContent();
        }
        [HttpGet("average-rating")]
        public async Task<IActionResult> GetHostAverageRating()
        {
            Guid hostId = GetUserId();
            var average = await _listingService.GetHostAverageRatingAsync(hostId);

            return Ok(new { overallRating = average });
        }

        [HttpGet("{id}/images")]
        public async Task<IActionResult> GetListingImages(Guid id)
        {
            Guid hostId = GetUserId();
            var images = await _listingService.GetListingImagesAsync(id, hostId);
            return Ok(images);
        }

        private Guid GetUserId()
        {
            var userIdFromToken = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                               ?? User.FindFirst("sub")?.Value
                               ?? User.Claims.FirstOrDefault(c => c.Type == "id")?.Value;

            if (string.IsNullOrEmpty(userIdFromToken) || !Guid.TryParse(userIdFromToken, out var userId))
                throw new UnauthorizedAccessException("El ID de usuario no existe en el Token o su formato Guid es inválido.");

            return userId;
        }

        private string GetUserRole()
        {
            var roles = _httpContextAccessor.HttpContext?.User?.Claims
                .Where(c => c.Type == ClaimTypes.Role || c.Type == "Role")
                .Select(c => c.Value);

            return roles != null ? string.Join(",", roles) : string.Empty;
        }
    }
}