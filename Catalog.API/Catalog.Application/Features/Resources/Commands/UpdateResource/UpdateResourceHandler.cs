using Catalog.Application.Common.Interfaces;
using Catalog.Domain.Exceptions;
using Catalog.Domain.Repositories;
using Catalog.Domain.ValueObjects;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Catalog.Application.Features.Resources.Commands.UpdateResource
{
    public sealed class UpdateResourceHandler : IRequestHandler<UpdateResourceCommand>
    {
        private readonly IResourceRepository _repository;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateResourceHandler(IResourceRepository repository, IUnitOfWork unitOfWork)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
        }
        public async  Task Handle(UpdateResourceCommand request, CancellationToken cancellationToken)
        {
            var resource = await _repository.GetByIdAsync(request.id, cancellationToken)
             ?? throw new ResourceNotFoundException(request.id);

            var location = new ResourceLocation(request.Building, request.Floor, request.RoomNumber);
            resource.Update(request.Name, request.Description, request.Capacity, location);

            await _repository.UpdateAsync(resource, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
