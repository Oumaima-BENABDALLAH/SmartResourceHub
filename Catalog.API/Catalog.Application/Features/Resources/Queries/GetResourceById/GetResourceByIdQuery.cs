using Catalog.Application.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Catalog.Application.Features.Resources.Queries.GetResourceById
{
   public sealed record  GetResourceByIdQuery (Guid id) : IRequest<ResourceDto>;
   
}
