namespace Application.DTOs.ListingDTOs
{
    public class AmenityDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? IconCode { get; set; }
    }
}