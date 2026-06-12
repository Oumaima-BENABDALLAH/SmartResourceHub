using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Catalog.Application.Features.Resources.Commands.DeleteResource
{
    public sealed record DeleteResourceCommand(Guid Id) : IRequest;

}
