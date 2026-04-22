using Domain.Entities.Listings;

namespace Application.Interfaces.Repositories.Listings.ListingImages
{
    public interface IQueryListingImagesRepository : IQueryRepository<ListingImage>
    {
        IQueryable<ListingImage> GetImagesByListingId(Guid listingId);
    }
}
