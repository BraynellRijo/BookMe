namespace Application.DTOs.NotificationDTOs
{
    public class NotificationCreationDTO
    {
        public Guid UserId { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
    }
}
