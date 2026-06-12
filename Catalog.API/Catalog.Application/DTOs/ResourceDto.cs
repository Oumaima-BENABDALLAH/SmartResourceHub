using Catalog.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Catalog.Application.DTOs
{
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
