
using Domain.Entities.Listings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetTopologySuite.Geometries;

namespace Infrastructure.Persistence.Configurations
{
    public class ListingConfiguration : IEntityTypeConfiguration<Listing>
    {
        public void Configure(EntityTypeBuilder<Listing> builder)
        {
            builder.ToTable("Listings");
            builder.HasKey(l => l.Id);

            builder.HasQueryFilter(l => l.IsAvailable);

            builder.Property(l => l.Title)
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(l => l.Description)
                .HasMaxLength(2500)
                .IsRequired();

            builder.Property(l => l.Type)
                .HasConversion<string>()
                .HasMaxLength(50)
                .IsRequired();

            builder.OwnsOne(l => l.Capacity, capacityBuilder =>
            {
                capacityBuilder.Property(cl => cl.MaxGuests)
                    .HasColumnName("MaxGuests")
                    .IsRequired();
                capacityBuilder.Property(cl => cl.BedroomsQuantity)
                    .HasColumnName("BedroomsQuantity")
                    .IsRequired();
                capacityBuilder.Property(cl => cl.BedsQuantity)
                    .HasColumnName("BedsQuantity")
                    .IsRequired();
                capacityBuilder.Property(cl => cl.BathroomsQuantity)
                    .HasColumnName("BathroomsQuantity")
                    .HasPrecision(3, 1)
                    .IsRequired();
            });

            builder.OwnsOne(l => l.Location, locationBuilder =>
            {
                locationBuilder.Property(ll => ll.Country)
                    .HasColumnName("Country")
                    .IsRequired();
                locationBuilder.Property(ll => ll.City)
                    .HasColumnName("City")
                    .IsRequired();
                locationBuilder.Property(ll => ll.Address)
                    .HasColumnName("Address")
                    .IsRequired();
                locationBuilder.Property(ll => ll.Latitude)
                    .HasColumnName("Latitude")
                    .IsRequired();
                locationBuilder.Property(ll => ll.Longitude)
                    .HasColumnName("Longitude")
                    .IsRequired();

                locationBuilder.Property<Point>("SpatialLocation")
                    .HasColumnName("SpatialLocation")
                    .HasColumnType("geography")
                    .HasComputedColumnSql("geography::Point([Latitude], [Longitude], 4326)", stored: true);
            });

            builder.OwnsOne(l => l.PricingRules, pricingRulesBuilder =>
            {
                pricingRulesBuilder.Property(pr => pr.PricePerNight)
                    .HasColumnName("PricePerNight")
                    .HasColumnType("decimal(18,2)")    
                    .IsRequired();
                pricingRulesBuilder.Property(pr => pr.CleaningFee)
                    .HasColumnName("CleaningFee")
                    .HasColumnType("decimal(18,2)")
                    .IsRequired();
                pricingRulesBuilder.Property(pr => pr.CheckInTime)
                    .HasColumnName("CheckInTime")
                    .IsRequired();
                pricingRulesBuilder.Property(pr => pr.CheckOutTime)
                    .HasColumnName("CheckOutTime")
                    .IsRequired();
            });

            builder.HasMany(l => l.Amenities)
                   .WithMany(a => a.Listings)
                   .UsingEntity(j => j.ToTable("ListingAmenities"));

            builder.HasMany(l => l.Reviews)
                .WithOne()
                .HasForeignKey(r => r.ListingId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(l => l.Bookings)
                .WithOne(b => b.Listing)
                .HasForeignKey(b => b.ListingId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Property(l => l.IsAvailable)
                .HasColumnName("IsAvailable")
                .HasDefaultValue(true);       
        }
    }
}
