using Catalog.Domain.Entities;
using Catalog.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Catalog.Infrastructure.Persistence.Configurations
{
    public class ResourceConfiguration : IEntityTypeConfiguration<Resource>
    {
        public void Configure(EntityTypeBuilder<Resource> builder)
        {
            builder.ToTable("Resources");

            builder.HasKey(x => x.Id);

            builder.Property(r => r.Id)
                   .ValueGeneratedNever();

            builder.Property(r => r.Name)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.Property(r => r.Description)
                   .HasMaxLength(500);

            builder.Property(r => r.Type)
                    .IsRequired()
                    .HasConversion(
                      v => v.ToString(),
                      v => (ResourceType)Enum.Parse(typeof(ResourceType), v))
                    .HasMaxLength(50);

            builder.Property(r => r.Capacity)
                    .IsRequired();

            builder.Property(r => r.IsAvailable)
                   .IsRequired()
                   .HasDefaultValue(true);

            builder.Property(r => r.CreatedAt)
                    .IsRequired();

            builder.Property(r => r.UpdatedAt);

            builder.OwnsOne(r => r.Location, Loc =>
            {
                Loc.Property(l => l.Building)
                     .HasColumnName("Building")
                     .IsRequired()
                     .HasMaxLength(100);

                Loc.Property(l => l.Floor)
                      .HasColumnName("Floor")
                      .HasMaxLength(20);

                Loc.Property(l => l.RoomNumber)
                     .HasColumnName("RoomNumber")
                     .HasMaxLength(20);
            }
            );
            builder.HasIndex(r => r.Type);
            builder.HasIndex(r => r.IsAvailable);

        }
    }
}
