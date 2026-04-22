using Application.DTOs.PhotoDTOs;
using Domain.Entities.Bookings;
using Domain.Entities.Listings;
using Domain.Enums.Listing;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs.ListingDTOs
{
    public class ListingDTO
    {
        public Guid Id { get; set; }
        public Guid HostId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public PropertyType Type { get; set; }
        public ListingCapacity Capacity { get; set; }
        public ListingLocation Location { get; set; }
        public ListingPricingRules PricingRules { get; set; }
        public IEnumerable<PhotoUploadResultDTO>? Images { get; set; }
        public decimal AverageRating { get; set; }
        public int ReviewsCount { get; set; }
        public bool IsAvailable { get; set; }
    }
}
