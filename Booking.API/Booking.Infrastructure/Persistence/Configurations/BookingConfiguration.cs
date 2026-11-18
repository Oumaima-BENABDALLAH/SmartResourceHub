using Booking.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Booking.Infrastructure.Persistence.Configurations
{
    //BookingConfiguration : fichier de configuration séparé pour garder le DbContext propre et organisé
    // IEntityTypeConfiguration : interface qui permet de respecter les principes du Clean Code au lieu de mettre  tout dans DbContext chaque entité a son propre fichier de configuration
    public class BookingConfiguration : IEntityTypeConfiguration<Domain.Entities.Booking>
    {
        // Configure : Définit la structure de la table, les types de colonnes, les contraintes et les index en base de données.
        public void Configure(EntityTypeBuilder<Domain.Entities.Booking> builder)
        {
            builder.ToTable("Bookings");

            builder.HasKey(b => b.Id);

            builder.Property(b => b.Id)
                .ValueGeneratedNever();

            builder.Property(b => b.ResourceId)
                .IsRequired();

            builder.Property(b => b.UserId)
                .IsRequired();

            builder.Property(b => b.ResourceName)
                .HasMaxLength(100);

            builder.Property(b => b.StartTime)
                .IsRequired();

            builder.Property(b => b.EndTime)
                .IsRequired();

            builder.Property(b => b.Status)
                .IsRequired()
                .HasConversion(
                    v => v.ToString(),
                    v => (BookingStatus)Enum.Parse(typeof(BookingStatus), v))
                .HasMaxLength(50);

            builder.Property(b => b.RejectionReason)
                .HasMaxLength(500);

            builder.Property(b => b.SagaCorrelationId)
                .IsRequired();

            builder.Property(b => b.CreatedAt)
                .IsRequired();

            builder.Property(b => b.UpdatedAt);

            // Index pour les requêtes fréquentes
            builder.HasIndex(b => b.UserId);
            builder.HasIndex(b => b.ResourceId);
            builder.HasIndex(b => b.Status);
            builder.HasIndex(b => b.SagaCorrelationId).IsUnique();

            // Index composite pour la détection de conflits et  optimiser les requêtes de vérification de disponibilité et éviter les conflits de réservation sur une même ressource
            // sans index la  base de données doit scanner toute la table Bookings ligne par ligne pour vérifier les dates. C'est très lent
            builder.HasIndex(b => new { b.ResourceId, b.StartTime, b.EndTime });
        }
    }
}
