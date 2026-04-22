using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Entities.Listings
{
    public class ListingImage
    {
        public Guid Id { get; set; }
        public Guid ListingId { get; set; }

        public string Url { get; set; }
        public string PublicId { get; set; }
        
        public bool IsMain { get; set; }
        public int Order { get; set; }

        // Navigation property
        public Listing Listing { get; set; }
    }
}
