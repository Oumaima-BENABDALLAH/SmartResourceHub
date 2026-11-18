using Catalog.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Catalog.Application.DTOs
{
    // Classe de transfert (DTO) non héritable(sealed) et immuable (non modifiable record), servant uniquement à renvoyer les données .

    public sealed record ResourceDto (Guid Id,
    string Name,
    string Description,
    ResourceType Type,
    string TypeLabel,
    int Capacity,
    string Building,
    string Floor,
    string RoomNumber,
    string FullLocation,
    bool IsAvailable,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
    
}
