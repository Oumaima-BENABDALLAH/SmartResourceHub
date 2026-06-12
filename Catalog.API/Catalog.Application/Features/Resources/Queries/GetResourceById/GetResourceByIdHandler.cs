using AutoMapper;
using Catalog.Application.DTOs;
using Catalog.Domain.Exceptions;
using Catalog.Domain.Repositories;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Catalog.Application.Features.Resources.Queries.GetResourceById;
 public sealed class GetResourceByIdHandler
    : IRequestHandler<GetResourceByIdQuery, ResourceDto>
{
    private readonly IResourceRepository _repository;
    private readonly IMapper _mapper;

    public GetResourceByIdHandler(IResourceRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<ResourceDto> Handle(
        GetResourceByIdQuery request,
        CancellationToken cancellationToken)
    {
        var resource = await _repository.GetByIdAsync(request.id, cancellationToken)
            ?? throw new ResourceNotFoundException(request.id);

        return _mapper.Map<ResourceDto>(resource);
    }
 }
