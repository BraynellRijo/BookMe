using Application.Interfaces.Services.NotificationServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] 
    public class NotificationsController(
        INotificationService notificationService) : ControllerBase
    {
        private readonly INotificationService _notificationService = notificationService;

        [HttpGet("Unread")]
        public async Task<IActionResult> GetUnread()
        {
            Guid userId = GetUserId();
            var notifications = await _notificationService.GetUnreadNotificationsAsync(userId);
            return Ok(notifications);
        }

        [HttpGet("All")]
        public async Task<IActionResult> GetAll()
        {
            Guid userId = GetUserId();
            var notifications = await _notificationService.GetAllNotificationsAsync(userId);
            return Ok(notifications);
        }

        [HttpPatch("MarkAsRead/{id}")]
        public async Task<IActionResult> MarkAsRead(Guid id)
        {
            Guid userId = GetUserId();
            await _notificationService.MarkAsReadAsync(id, userId);
            return NoContent();
        }

        [HttpPatch("MarkAllAsRead")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            Guid userId = GetUserId();
            await _notificationService.MarkAllAsReadAsync(userId);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            Guid userId = GetUserId();
            await _notificationService.DeleteNotificationAsync(id, userId);
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
    }
}