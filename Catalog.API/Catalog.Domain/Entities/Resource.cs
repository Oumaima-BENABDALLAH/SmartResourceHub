using Catalog.Domain.Common;
using Catalog.Domain.Enums;
using Catalog.Domain.Exceptions;
using Catalog.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace Catalog.Domain.Entities
{
    public  class Resource : BaseEntity
    {
        public string Name { get; private set; } = default!;
        public string Description { get; private set; } = string.Empty;
        public ResourceType Type { get; private set; }
        public int Capacity { get; private set; }
        public ResourceLocation Location { get; private set; } = default!;
        public bool IsAvailable { get; private set; } = true;

        private Resource () { }

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
