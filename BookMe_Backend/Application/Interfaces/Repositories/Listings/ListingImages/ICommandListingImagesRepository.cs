using Domain.Entities.Listings;

namespace Application.Interfaces.Repositories.Listings.ListingImages
{
    public interface ICommandListingImagesRepository : ICommandRepository<ListingImage>
    {
        Task CreateRangeAsync(IEnumerable<ListingImage> entities);
        Task DeleteRangeAsync(IEnumerable<ListingImage> entities);
    }
}
