namespace Domain.Entities.Listings;
public class ListingPricingRules
{
    public decimal PricePerNight { get; set; }
    public decimal CleaningFee { get; set; }
    public TimeSpan CheckInTime { get; set; }
    public TimeSpan CheckOutTime { get; set; }
}
