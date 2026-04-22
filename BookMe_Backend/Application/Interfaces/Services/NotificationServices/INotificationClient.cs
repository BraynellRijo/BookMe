using Application.DTOs.NotificationDTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Interfaces.Services.NotificationServices
{
    public interface INotificationClient
    {
        Task ReceiveNotification(NotificationDTO notification);
    }
}
