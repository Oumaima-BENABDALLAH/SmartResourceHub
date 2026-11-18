using Catalog.Domain.Common;
using Catalog.Domain.Enums;
using Catalog.Domain.Exceptions;
using Catalog.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace Catalog.Domain.Entities
{
    public  class Resource : BaseEntity  ///Resourcer représente l'entité Resource (Aggregate Root) dans le microservice Catalog.
                                         /// Encapsule les règles métiers et garantit l'intégrité des données associées.
    {
        public string Name { get; private set; } = default!;
        public string Description { get; private set; } = string.Empty;
        public ResourceType Type { get; private set; }
        public int Capacity { get; private set; }
        public ResourceLocation Location { get; private set; } = default!;
        public bool IsAvailable { get; private set; } = true;

        // Constructeur privé requis par Entity Framework Core pour la matérialisation des entités, tout en empêchant une instanciation libre depuis l'extérieur (respect du DDD).
        private Resource () { }

        //Create () Factory Method : Valide les invariants métiers et crée une nouvelle instance valide de Resource.
        public static Resource Create(
                             string name,
                             string description,
                             ResourceType type,
                             int capacity,
                             ResourceLocation location)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new DomainException("Resource name cannot be empty.");

            if (capacity < 0) throw new DomainException("Capacity cannot be negative.");

            return new Resource
            {
                Id = Guid.NewGuid(),
                Name = name,
                Description = description,
                Type = type,
                Capacity = capacity,
                Location = location,
                IsAvailable = true,
                CreatedAt = DateTime.UtcNow
            };

        }

        //Update () Met à jour les informations de la ressource après validation des règles métiers.
        public void Update(string name, string description, int capacity, ResourceLocation location)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new DomainException("Resource name cannot be empty.");

            if (capacity < 0) throw new DomainException("Capacity cannot be negative.");

            Name = name;
            Description = description;
            Capacity = capacity;
            Location = location;
            SetUpdatedAt();

        }

        //MarkAsAvailable () et MarkAsUnavailable () : Modifient l'état de disponibilité de la ressource et mettent à jour le timestamp UpdatedAt.
        public void MarkAsAvailable()
        {
            IsAvailable = true;
            SetUpdatedAt();
        }

        public void MarkAsUnavailable()
        {
            IsAvailable = false;
            SetUpdatedAt();

        }

    }
}
