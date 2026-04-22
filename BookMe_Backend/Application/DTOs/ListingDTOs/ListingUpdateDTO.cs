using Domain.Enums.Listing;

namespace Application.DTOs.ListingDTOs
{
    public class ListingUpdateDTO
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public PropertyType Type { get; set; }

        public int MaxGuests { get; set; }
        public int BedroomsQuantity { get; set; }
        public int BedsQuantity { get; set; }
        public double BathroomsQuantity { get; set; }

        public string Country { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public decimal PricePerNight { get; set; }
        public decimal CleaningFee { get; set; }
        public TimeSpan CheckInTime { get; set; }
        public TimeSpan CheckOutTime { get; set; }

        public List<Guid> AmenityIds { get; set; } = new();
    }
}