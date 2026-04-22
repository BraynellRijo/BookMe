namespace Domain.Entities.Listings
{
    public class ListingBlock
    {
        public Guid Id { get; set; }
        public Guid ListingId { get; set; }
        public Guid HostId { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public string Reason { get; set; } = string.Empty; 
        public DateTime CreatedAt { get; set; }

        public Listing Listing { get; set; } = null!;
    }
}
