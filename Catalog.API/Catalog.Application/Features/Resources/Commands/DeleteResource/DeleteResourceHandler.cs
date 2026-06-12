using Catalog.Application.Common.Interfaces;
using Catalog.Domain.Exceptions;
using Catalog.Domain.Repositories;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Catalog.Application.Features.Resources.Commands.DeleteResource
{
    public sealed class DeleteResourceHandler : IRequestHandler<DeleteResourceCommand>
    {
        private readonly IResourceRepository _repository;
        private readonly IUnitOfWork _unitOfWork;
        public DeleteResourceHandler(IResourceRepository repository, IUnitOfWork unitOfWork) {
            _repository = repository;
            _unitOfWork = unitOfWork;
        }
        public async Task Handle(DeleteResourceCommand request, CancellationToken cancellationToken)
        {
            var exists = await _repository.ExistsAsync(request.Id, cancellationToken);
            if (!exists) throw new ResourceNotFoundException(request.Id);

            await _repository.DeleteAsync(request.Id, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
