using Catalog.Domain.Enums;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Catalog.Application.Features.Resources.Commands.UpdateResource
{
    public sealed record   UpdateResourceCommand
    ( Guid id , 
      string Name,
      string Description,
      int Capacity,
      string Building,
      string Floor,
      string RoomNumber) : IRequest;
}
