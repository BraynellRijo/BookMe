using Application.DTOs.BookingDTOs;
using Application.Interfaces.Services.BookingServices;
using Application.Services.BookingService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Runtime.InteropServices;
using System.Security.Claims;

namespace Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles ="Guest")]
    public class GuestBookingController : ControllerBase
    {
        private readonly IGuestBookingService _bookingService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public GuestBookingController(IGuestBookingService bookingService, IHttpContextAccessor httpContextAccessor)
        {
            _bookingService = bookingService;
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpPost("Booking-Creation")]
        public async Task<IActionResult> CreateBooking(BookingCreationDTO bookingCreationDTO)
        {
            Guid guestId = GetUserId();
            await _bookingService.CreateBookingAsync(bookingCreationDTO, guestId);
            return Created();
        }

        [HttpGet("Guest-Bookings")]
        public async Task<IActionResult> GetGuestBookings()
        {
            Guid guestId = GetUserId();
            var bookings = await _bookingService.GetGuestBookingsAsync(guestId);
            return Ok(bookings);
        }

        [HttpDelete("Cancel-Booking/{bookingId}")]
        public async Task<IActionResult> CancelBooking(Guid bookingId)
        {
            Guid userId = GetUserId();
            await _bookingService.CancelBooking(bookingId, userId);
            return NoContent();
        }


        //Helper Methods
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
            var userRole = _httpContextAccessor.HttpContext.User.Claims
                .FirstOrDefault(c => c.Type == "Role")?.Value;
            return userRole;
        }
    }
}
