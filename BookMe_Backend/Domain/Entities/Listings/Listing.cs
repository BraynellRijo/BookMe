using Domain.Entities.Bookings;
using Domain.Enums.Listing;

namespace Domain.Entities.Listings;
public class Listing
{
    public Guid Id { get; set; }
    public Guid HostId { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public PropertyType Type { get; set; }
    public ListingCapacity Capacity { get; set; }
    public ListingLocation Location { get; set; }
    public ListingPricingRules PricingRules { get; set; }
    public ICollection<ListingImage> Images { get; set; } = new List<ListingImage>();
    public ICollection<Amenity>? Amenities { get; set; } = new List<Amenity>();
    public ICollection<Review>? Reviews { get; set; } = new List<Review>();
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    public bool IsAvailable { get; set; } 
}
