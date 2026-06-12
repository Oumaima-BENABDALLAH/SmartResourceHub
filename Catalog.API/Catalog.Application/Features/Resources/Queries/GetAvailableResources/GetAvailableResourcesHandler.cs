using AutoMapper;
using Catalog.Application.DTOs;
using Catalog.Domain.Repositories;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Catalog.Application.Features.Resources.Queries.GetAvailableResources
{
    public sealed class GetAvailableResourcesHandler
     : IRequestHandler<GetAvailableResourcesQuery, IReadOnlyList<ResourceDto>>
    {
        private readonly IResourceRepository _repository;
        private readonly IMapper _mapper;

        public GetAvailableResourcesHandler(IResourceRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<IReadOnlyList<ResourceDto>> Handle(
            GetAvailableResourcesQuery request,
            CancellationToken cancellationToken)
        {
            var resources = await _repository.GetAvailableAsync(cancellationToken);
            return _mapper.Map<IReadOnlyList<ResourceDto>>(resources);
        }
    }
}