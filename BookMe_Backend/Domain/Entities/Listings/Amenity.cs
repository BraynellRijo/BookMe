namespace Domain.Entities.Listings;

public class Amenity
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public string? IconCode { get; set; }
    public ICollection<Listing>? Listings { get; set; }
}