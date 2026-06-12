using System;
using System.Collections.Generic;
using System.Text;

namespace Catalog.Domain.ValueObjects
{
    public sealed class ResourceLocation
    {
        public string Building { get; } = string.Empty;
        public string Floor { get; } = string.Empty;
        public string RoomNumber { get; } = string.Empty;

        private ResourceLocation() { }
        public ResourceLocation (string building, string floor, string roomNumber)
        {
            if (string.IsNullOrWhiteSpace(building))
                throw new ArgumentNullException("Building cannot be empty", nameof(building));
            Building = building;
            Floor = floor;
            RoomNumber = roomNumber;
        }
        public override string ToString() => $"{Building} - Floor {Floor} - {RoomNumber}";

        public override bool Equals(object? obj)
        {
            if (obj is not ResourceLocation other) return false;

            return Building == other.Building &&
                   Floor == other.Floor &&
                   RoomNumber == other.RoomNumber ;
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(Building, Floor, RoomNumber);
        }
    }
}
