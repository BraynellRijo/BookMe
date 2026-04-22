namespace Application.DTOs.ListingDTOs
{
    public class BlockDatesDTO
    {
        public Guid ListingId { get; set; }
        public string Reason { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
