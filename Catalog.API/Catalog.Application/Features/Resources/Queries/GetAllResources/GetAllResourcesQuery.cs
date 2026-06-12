using Catalog.Application.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Catalog.Application.Features.Resources.Queries.GetAllResources
{
    public sealed record GetAllResourcesQuery() : IRequest<IReadOnlyList<ResourceDto>>;

}
