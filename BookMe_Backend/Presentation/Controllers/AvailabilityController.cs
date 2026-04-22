using Application.DTOs.ListingDTOs;
using Application.Interfaces.Services.ListingServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AvailabilityController : ControllerBase
    {
        private readonly IHostListingService _hostListingService;
        private readonly IAvailabilityService _availabilityService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AvailabilityController(
            IHostListingService hostListingService,
            IAvailabilityService availabilityService,
            IHttpContextAccessor httpContextAccessor)
        {
            _hostListingService = hostListingService;
            _availabilityService = availabilityService;
            _httpContextAccessor = httpContextAccessor;
        }


        [HttpPost("Block-Dates")]
        [Authorize(Roles = "Host")] 
        public async Task<IActionResult> BlockDates([FromBody] BlockDatesDTO request)
        {
            Guid hostId = GetUserId();
            string role = GetUserRole();

            await _hostListingService.BlockDatesAsync(request.ListingId, request.StartDate, request.EndDate, request.Reason, hostId, role);

            return Ok(new { message = "Dates blocked successfully." });
        }

        [HttpDelete("Unblock-Dates/{blockId}/Listing/{listingId}")]
        [Authorize(Roles = "Host")]
        public async Task<IActionResult> UnblockDates(Guid blockId, Guid listingId)
        {
            Guid hostId = GetUserId();
            string role = GetUserRole();

            await _hostListingService.UnblockDatesAsync(blockId, listingId, hostId, role);

            return NoContent();
        }


        [HttpGet("Listing-Blocks/{listingId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetBlockedDates(Guid listingId)
        {
            var blockedDates = await _availabilityService.GetBlockedDateRangesAsync(listingId);
            return Ok(blockedDates);
        }

        [HttpGet("Check-Availability")]
        [AllowAnonymous]
        public async Task<IActionResult> CheckAvailability([FromQuery] Guid listingId, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var isAvailable = await _availabilityService.IsListingAvailableAsync(listingId, startDate, endDate);
            return Ok(new { isAvailable });
        }

        //Helper Methods
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