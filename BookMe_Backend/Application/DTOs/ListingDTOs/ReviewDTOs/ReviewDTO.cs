namespace Application.DTOs.ListingDTOs.ReviewDTOs
{
    public class ReviewDTO
    {
        public int Rating { get; set; }
        public string Comment { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
