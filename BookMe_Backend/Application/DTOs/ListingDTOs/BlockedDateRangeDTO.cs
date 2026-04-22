
namespace Application.DTOs.ListingDTOs
{
    public class BlockedDateRangeDTO
    {
        public Guid? Id { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsManualBlock { get; set; }
    }
}
