using Domain.Entities.Users;

namespace Domain.Entities.Notifications
{
    public class Notification
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }

        public string Title { get; set; } = null!;
        public string Message { get; set; } = null!;

        public bool IsRead { get; set; } = false;
        public DateTime CreatedAt { get; set; }

        public User User { get; set; } = null!;
    }
}
