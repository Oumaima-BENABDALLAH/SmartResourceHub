using Catalog.Application.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Catalog.Application.Features.Resources.Queries.GetAvailableResources
{
    public sealed record GetAvailableResourcesQuery() : IRequest<IReadOnlyList<ResourceDto>>;

}
