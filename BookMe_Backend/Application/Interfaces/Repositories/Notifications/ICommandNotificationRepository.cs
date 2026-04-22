using Domain.Entities.Notifications;

namespace Application.Interfaces.Repositories.Notifications
{
    public interface ICommandNotificationRepository : ICommandRepository<Notification>
    {
        Task UpdateRangeAsync(IEnumerable<Notification> entities);

    }
}
