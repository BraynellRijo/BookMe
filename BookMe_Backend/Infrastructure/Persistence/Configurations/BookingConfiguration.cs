using Domain.Entities.Bookings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class BookingConfiguration : IEntityTypeConfiguration<Booking>
    {
        public void Configure(EntityTypeBuilder<Booking> builder)
        {
            builder.HasKey(b => b.Id);

            builder.HasOne(b => b.Listing)
                .WithMany(l => l.Bookings)
                .HasForeignKey(b => b.ListingId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(b => b.Guest)
                .WithMany()
                .HasForeignKey(b => b.GuestId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Property(b => b.CheckInDate).IsRequired();
            builder.Property(b => b.CheckOutDate).IsRequired();

            builder.Property(b => b.TotalGuests)
                .IsRequired();

            builder.Property(b => b.CleaningFee)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(b => b.TotalPrice)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(b => b.Status)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();
        }
    }
}
