using AutoMapper;
using Catalog.Application.DTOs;
using Catalog.Domain.Repositories;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Catalog.Application.Features.Resources.Queries.GetAllResources
{
    public sealed class GetAllResourcesHandler
     : IRequestHandler<GetAllResourcesQuery, IReadOnlyList<ResourceDto>>
    {
        private readonly IResourceRepository _repository;
        private readonly IMapper _mapper;

        public GetAllResourcesHandler(IResourceRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<IReadOnlyList<ResourceDto>> Handle(
            GetAllResourcesQuery request,
            CancellationToken cancellationToken)
        {
            var resources = await _repository.GetAllAsync(cancellationToken);
            return _mapper.Map<IReadOnlyList<ResourceDto>>(resources);
        }
    }
}
