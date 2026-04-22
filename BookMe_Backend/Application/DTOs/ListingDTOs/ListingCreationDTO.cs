using Domain.Entities.Listings;
using Domain.Enums.Listing;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs.ListingDTOs
{
    public class ListingCreationDTO
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public PropertyType Type { get; set; }
        public ListingCapacity Capacity { get; set; }
        public ListingLocation Location { get; set; }
        public ListingPricingRules PricingRules { get; set; }
        public ICollection<Amenity>? Amenities { get; set; }
    }
}
