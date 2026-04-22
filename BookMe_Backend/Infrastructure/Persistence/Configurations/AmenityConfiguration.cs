using Domain.Entities.Listings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class AmenityConfiguration : IEntityTypeConfiguration<Amenity>
    {
        public void Configure(EntityTypeBuilder<Amenity> builder)
        {
            builder.ToTable("Amenities");
            builder.HasKey(a => a.Id);

            builder.Property(a => a.Name)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(a => a.IconCode)
                .HasMaxLength(50);
        }
    }
}
