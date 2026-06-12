using Catalog.Application.DTOs;
using Catalog.Domain.Enums;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Catalog.Application.Features.Resources.Commands.CreateResource
{
    public sealed record  CreateResourceCommand (
    string Name,
    string Description,
    ResourceType Type,
    int Capacity,
    string Building,
    string Floor,
    string RoomNumber) : IRequest<ResourceDto>;
}
