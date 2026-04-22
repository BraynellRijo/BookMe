using Application.Interfaces.Repositories.Bookings;
using Domain.Entities.Bookings;
using Domain.Enums.Bookings;
using Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories
{
    public class BookingRepository(AppDbContext dbContext) : IQueryBookingRepository,
        ICommandBookingRepository
    {
        private readonly AppDbContext _dbContext = dbContext;

        public IQueryable<Booking> GetAllAsync()
        {
            return _dbContext.Bookings.AsNoTracking();
        }

        public IQueryable<Booking> GetAllByGuest(Guid guestId)
        {
            return _dbContext.Bookings.AsNoTracking()
                .Include(b => b.Listing)
                    .ThenInclude(l => l.Images)
                .Where(b => b.GuestId == guestId);
        }

        public IQueryable<Booking> GetAllByListing(Guid listingId)
        {
            return _dbContext.Bookings.AsNoTracking()
                .Where(b => b.ListingId == listingId);
        }

        public IQueryable<Booking> GetByStatus(BookingStatus status)
        {
            return _dbContext.Bookings.AsNoTracking()
                .Where(b => b.Status == status);
        }

        public async Task<Booking?> GetByIdAsync(Guid id)
        {
            return await _dbContext.Bookings
                .FirstOrDefaultAsync(b => b.Id == id);
        }

        public async Task<Booking?> GetBookingWithListingAsync(Guid id)
        {
            return await _dbContext.Bookings
                .Include(b => b.Listing)
                .FirstOrDefaultAsync(b => b.Id == id);
        }

        public IQueryable<Booking> FilterBookingsByDateRange(DateTime checkIn, DateTime checkOut)
        {
            return _dbContext.Bookings.AsNoTracking()
                .Where(b => (b.CheckInDate < checkOut && b.CheckOutDate > checkIn)
                && (b.Status == BookingStatus.Pending || b.Status == BookingStatus.Confirmed));
        }

        public async Task<bool> HasOverlappingAsync(Guid listingId, DateTime checkIn, DateTime checkOut, Guid? bookingId = null)
        {
            var query = _dbContext.Bookings
                .Where(b => b.ListingId == listingId
                     && b.Status != BookingStatus.Cancelled
                     && b.CheckInDate < checkOut
                     && b.CheckOutDate > checkIn);

            if (bookingId.HasValue && bookingId.Value != Guid.Empty)
            {
                query = query.Where(b => b.Id != bookingId.Value);
            }

            bool hasOverlap = await query.AnyAsync();

            return hasOverlap;
        }

        public IQueryable<Booking> GetActiveBookingsByListing(Guid listingId)
        {
            return _dbContext.Bookings.AsNoTracking()
                .Where(b => b.ListingId == listingId
                    && b.CheckOutDate >= DateTime.UtcNow.Date
                    && (b.Status == BookingStatus.Pending || b.Status == BookingStatus.Confirmed));
        }


        public async Task CreateAsync(Booking entity)
        {
            await _dbContext.Bookings.AddAsync(entity);
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            await _dbContext.Bookings
                .Where(b => b.Id == id)
                .ExecuteDeleteAsync();
        }

        public async Task UpdateAsync(Guid id, Booking entity)
        {
            await _dbContext.Bookings
                .Where(b => b.Id == id)
                .ExecuteUpdateAsync(b => b
                    .SetProperty(p => p.CheckInDate, entity.CheckInDate)
                    .SetProperty(p => p.CheckOutDate, entity.CheckOutDate)
                    .SetProperty(p => p.Status, entity.Status));
        }

        public async Task UpdateStatusAsync(Guid bookingId, BookingStatus newStatus)
        {
            await _dbContext.Bookings
                .Where(b => b.Id == bookingId)
                .ExecuteUpdateAsync(b => b
                    .SetProperty(p => p.Status, newStatus));
        }

        public async Task CancelBookingAsync(Guid bookingId)
        {
            await _dbContext.Bookings
                .Where(b => b.Id == bookingId)
                .ExecuteUpdateAsync(b => b
                    .SetProperty(p => p.Status, BookingStatus.Cancelled)
                    .SetProperty(p => p.CancelledAt, DateTime.UtcNow));
        }
    }
}