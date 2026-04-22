using Application.DTOs.NotificationDTOs;
using Application.Interfaces.Services.NotificationServices;
using Microsoft.AspNetCore.SignalR;

namespace Infrastructure.Services.NotificationServices
{
    public class SignalRNotificationService(
          IHubContext<NotificationHub, INotificationClient> hubContext) : IRealTimeNotificationService
    {
        private readonly IHubContext<NotificationHub, INotificationClient> _hubContext = hubContext;

        public async Task NotifyUserAsync(Guid userId, NotificationDTO notification)
        {
            await _hubContext.Clients.User(userId.ToString()).ReceiveNotification(notification);
        }
    }
}