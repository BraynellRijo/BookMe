namespace Domain.Entities.Listings;
public class ListingLocation()
{
    public string Address { get; set; }
    public string City { get; set; }
    public string Country { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}