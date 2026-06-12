using AutoMapper;
using Catalog.Application.Common.Interfaces;
using Catalog.Application.DTOs;
using Catalog.Domain.Entities;
using Catalog.Domain.Repositories;
using Catalog.Domain.ValueObjects;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Catalog.Application.Features.Resources.Commands.CreateResource
{
    public sealed class CreateResourceHandler
     : IRequestHandler<CreateResourceCommand, ResourceDto>
    {
        private readonly IResourceRepository _repository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public CreateResourceHandler(IResourceRepository repository,
        IUnitOfWork unitOfWork,
        IMapper mapper)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        public async Task<ResourceDto> Handle(CreateResourceCommand request, CancellationToken cancellationToken)
        {

            var location = new ResourceLocation(request.Building, request.Floor, request.RoomNumber);

            var resource = Resource.Create(
                request.Name,
                request.Description,
                request.Type,
                request.Capacity,
                location

                );

            await _repository.AddAsync(resource, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            
            return _mapper.Map<ResourceDto>(resource);
        }
    }
}
