using Domain.Entities.Notifications;

namespace Application.Interfaces.Repositories.Notifications
{
    public interface IQueryNotificationRepository : IQueryRepository<Notification>
    {
        IQueryable<Notification> GetByUserId(Guid userId, bool? isRead = null);

    }
}
