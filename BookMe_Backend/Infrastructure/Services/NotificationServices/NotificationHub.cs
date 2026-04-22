using Application.Interfaces.Services.NotificationServices;
using Microsoft.AspNetCore.SignalR;

namespace Infrastructure.Services.NotificationServices
{
    public class NotificationHub : Hub<INotificationClient>
    {
        //public override async Task OnConnectedAsync()
        //{
        //    await base.OnConnectedAsync();
        //}
    }
}
