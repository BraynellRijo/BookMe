using Domain.Entities.Listings;

namespace Application.Interfaces.Repositories.Listings.ListingBlocks
{
    public interface IQueryListingBlockRepository : IQueryRepository<ListingBlock>
    {
        Task<bool> HasOverlappingAsync(Guid listingId, DateTime startDate, DateTime endDate);
        IQueryable<ListingBlock> GetActiveBlockForListingAsync(Guid listingId);

    }
}
