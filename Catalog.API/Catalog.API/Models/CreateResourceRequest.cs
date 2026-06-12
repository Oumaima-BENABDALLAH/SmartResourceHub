using Catalog.Domain.Enums;

namespace Catalog.API.Models
{
    public sealed record CreateResourceRequest(
     string Name,
     string Description,
     ResourceType Type,
     int Capacity,
     string Building,
     string Floor,
     string RoomNumber
 );
}
