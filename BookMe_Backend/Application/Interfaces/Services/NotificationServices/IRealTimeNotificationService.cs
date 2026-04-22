using Application.DTOs.NotificationDTOs;

namespace Application.Interfaces.Services.NotificationServices
{
    public interface IRealTimeNotificationService
    {
        Task NotifyUserAsync(Guid userId, NotificationDTO notification);
    }
}
