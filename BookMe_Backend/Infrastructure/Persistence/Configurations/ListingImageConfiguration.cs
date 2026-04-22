using Domain.Entities.Listings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Persistence.Configurations
{
    public class ListingImageConfiguration : IEntityTypeConfiguration<ListingImage>
    {
        public void Configure(EntityTypeBuilder<ListingImage> builder)
        {
            builder.ToTable("ListingImages");
            builder.HasKey(li => li.Id);

            builder.Property(li => li.Url)
                .IsRequired()
                .HasMaxLength(500);


            builder.Property(li => li.PublicId)
                .IsRequired()
                .HasMaxLength(200);
        
            builder.HasOne(li => li.Listing)
                .WithMany(l => l.Images)
                .HasForeignKey(li => li.ListingId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
