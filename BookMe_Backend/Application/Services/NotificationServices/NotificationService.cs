using Application.DTOs.NotificationDTOs;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Notifications;
using Application.Interfaces.Services.NotificationServices;
using AutoMapper;
using Domain.Entities.Notifications;
using Domain.Entities.Users;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Application.Services.NotificationService
{
    public class NotificationService(
        IQueryNotificationRepository queryNotificationRepository,
        ICommandNotificationRepository commandNotificationRepository,
        IRealTimeNotificationService realTimeNotificationService,
        IMapper mapper) : INotificationService
    {
        private readonly IQueryNotificationRepository _queryNotificationRepository = queryNotificationRepository;
        private readonly ICommandNotificationRepository _commandNotificationRepository = commandNotificationRepository;
        private readonly IRealTimeNotificationService _realTimeService = realTimeNotificationService;
        private readonly IMapper _mapper = mapper;

        public async Task SendNotificationAsync(NotificationCreationDTO notificationDTO)
        {

            var notification = _mapper.Map<Notification>(notificationDTO);
            notification.CreatedAt = DateTime.UtcNow;
            notification.IsRead = false;

            await _commandNotificationRepository.CreateAsync(notification);

            var responseDTO = _mapper.Map<NotificationDTO>(notification);
            await _realTimeService.NotifyUserAsync(notification.UserId, responseDTO);
        }

        public async Task<IEnumerable<NotificationDTO>> GetUnreadNotificationsAsync(Guid userId)
        {
            var unreadNotifications = await _queryNotificationRepository
                .GetByUserId(userId, isRead: false).ToListAsync();
            
            return _mapper.Map<IEnumerable<NotificationDTO>>(unreadNotifications);
        }

        public async Task<IEnumerable<NotificationDTO>> GetAllNotificationsAsync(Guid userId)
        {
            var notifications = await _queryNotificationRepository.
                GetByUserId(userId).ToListAsync(); 
            return _mapper.Map<IEnumerable<NotificationDTO>>(notifications);
        }

        public async Task MarkAsReadAsync(Guid notificationId, Guid currentUserId)
        {
            var notification = await _queryNotificationRepository.GetByIdAsync(notificationId);

            if (notification == null)
                throw new KeyNotFoundException("Notification not found.");

            if (notification.UserId != currentUserId)
                throw new UnauthorizedAccessException("You don't have permission to modify this notification.");

            if (!notification.IsRead)
            {
                notification.IsRead = true;
                await _commandNotificationRepository.UpdateAsync(notificationId, notification);
            }
        }

        public async Task MarkAllAsReadAsync(Guid userId)
        {
            var unreadNotifications = await _queryNotificationRepository
                .GetByUserId(userId, isRead: false).ToListAsync();
            var notificationsList = unreadNotifications.ToList();

            if (notificationsList.Any())
            {
                foreach (var notification in notificationsList)
                {
                    notification.IsRead = true;
                }

                await _commandNotificationRepository.UpdateRangeAsync(notificationsList);
            }
        }
        public async Task DeleteNotificationAsync(Guid id, Guid userId)
        {
            var notification = await _queryNotificationRepository.GetByIdAsync(id)
                ?? throw new KeyNotFoundException("Notification not found.");

            if (notification.UserId != userId)
                throw new UnauthorizedAccessException("You are not authorized to delete this notification.");

            await _commandNotificationRepository.DeleteAsync(id);
        }
    }
}