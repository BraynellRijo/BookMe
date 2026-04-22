using Domain.Entities.Bookings;
using Domain.Entities.Listings;
using Domain.Entities.Notifications;
using Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Infrastructure.Persistence.Contexts
{
    public class AppDbContext(DbContextOptions<AppDbContext> op) : DbContext(op)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Listing> Listings { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<ListingBlock> ListingBlocks { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Amenity> Amenities { get; set; }
        public DbSet<ListingImage> ListingImages { get; set; }
        public DbSet<Notification> Notifications { get; set; }
    }
}
