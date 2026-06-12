namespace Catalog.API.Models
{
    public sealed record UpdateResourceRequest(
     string Name,
     string Description,
     int Capacity,
     string Building,
     string Floor,
     string RoomNumber
 );
}
