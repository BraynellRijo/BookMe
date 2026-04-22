using Application.Interfaces.Repositories.Notifications;
using Domain.Entities.Notifications;
using Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories
{
    public class NotificationRepository(AppDbContext dbContext) : IQueryNotificationRepository, ICommandNotificationRepository
    {
        private readonly AppDbContext _dbContext = dbContext;

        public async Task CreateAsync(Notification entity)
        {
            await _dbContext.Notifications.AddAsync(entity);
            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateAsync(Guid id, Notification entity)
        {
            var existingEntity = await _dbContext.Notifications.FindAsync(id);

            if (existingEntity == null)
                throw new KeyNotFoundException($"Notification with ID {id} was not found.");

            _dbContext.Entry(existingEntity).CurrentValues.SetValues(entity);
            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateRangeAsync(IEnumerable<Notification> entities)
        {
            _dbContext.Notifications.UpdateRange(entities);
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var entity = await _dbContext.Notifications.FindAsync(id);

            if (entity != null)
            {
                _dbContext.Notifications.Remove(entity);
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task<Notification?> GetByIdAsync(Guid id)
        {
            return await _dbContext.Notifications.FindAsync(id);
        }

        public IQueryable<Notification> GetByUserId(Guid userId, bool? isRead = null)
        {
            var query = _dbContext.Notifications
                .Where(n => n.UserId == userId);

            if (isRead.HasValue)
            {
                query = query.Where(n => n.IsRead == isRead.Value);
            }

            return query.OrderByDescending(n => n.CreatedAt);
        }

        public IQueryable<Notification> GetAllAsync()
        {
            return _dbContext.Notifications.AsNoTracking();
        }
    }
}