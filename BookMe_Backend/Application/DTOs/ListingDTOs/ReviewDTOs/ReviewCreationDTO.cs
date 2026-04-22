namespace Application.DTOs.ListingDTOs.ReviewDTOs
{
    public class ReviewCreationDTO
    {
        public Guid ListingId { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }
    }
}
