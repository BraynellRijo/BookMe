
using Application.DTOs.NotificationDTOs;

namespace Application.Interfaces.Services.NotificationServices
{
    public interface INotificationService
    {
        Task SendNotificationAsync(NotificationCreationDTO notificationDTO);
        Task<IEnumerable<NotificationDTO>> GetUnreadNotificationsAsync(Guid userId);
        Task<IEnumerable<NotificationDTO>> GetAllNotificationsAsync(Guid userId);
        Task MarkAsReadAsync(Guid notificationId, Guid currentUserId);
        Task MarkAllAsReadAsync(Guid userId);
        Task DeleteNotificationAsync(Guid id, Guid userId);
    }
}
