namespace Domain.Entities.Listings
{
    public class Review
    {
        public Guid Id { get; set; }
        public Guid ListingId { get; set; }
        public Guid GuestId { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
