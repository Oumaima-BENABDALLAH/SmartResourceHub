using Catalog.Domain.Entities;
using Catalog.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Catalog.Infrastructure.Persistence.Configurations
{

    //ResourceConfiguration : fichier de configuration séparé pour garder le DbContext propre et organisé
    // IEntityTypeConfiguration : interface qui permet de respecter les principes du Clean Code au lieu de mettre  tout dans DbContext chaque entité a son propre fichier de configuration

    public class ResourceConfiguration : IEntityTypeConfiguration<Resource>
    {
        // Configure : Définit la structure de la table, les types de colonnes, les contraintes et les index en base de données.

        public void Configure(EntityTypeBuilder<Resource> builder)
        {
            builder.ToTable("Resources");

            //indique explicitement à Entity Framework Core que la propriété Id est la clé primaire (Primary Key) de la table en base de données.
            builder.HasKey(x => x.Id);

            //// ValueGeneratedNever() : Indique à Entity Framework que la base de données ne doit PAS générer automatiquement l'identifiant.
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
            // HasDefaultValue(true) : pour attribuer une valeur par défaut automatique directement au niveau de la base de données
            // lorsqu'un nouvel enregistrement est inséré sans préciser explicitement cette valeur.
            builder.Property(r => r.IsAvailable)
                   .IsRequired()
                   .HasDefaultValue(true);

            builder.Property(r => r.CreatedAt)
                    .IsRequired();

            builder.Property(r => r.UpdatedAt);

            //// OwnsOne : Configure l'objet valeur (Value Object) Location. EF Core stocke ses propriétés directement en tant que colonnes dans la même table de la ressource (table aplatie),
            //tout en permettant de personnaliser leurs noms et contraintes.
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
