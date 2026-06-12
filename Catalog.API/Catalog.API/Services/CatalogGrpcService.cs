using Catalog.API.Protos;
using Catalog.Application.Features.Resources.Queries.GetResourceById;
using Catalog.Domain.Enums;
using Catalog.Domain.Exceptions;
using Catalog.Domain.Repositories;
using Grpc.Core;
using MediatR;

namespace Catalog.API.Services
{
    public class CatalogGrpcService: Protos.CatalogService.CatalogServiceBase
    {
        private readonly IMediator _mediator;
        private readonly IResourceRepository _repository;
        public CatalogGrpcService(IMediator mediator, IResourceRepository repository)
        {
            _mediator = mediator;
            _repository = repository;
        }

       public override async Task<ResourceResponse> GetResource(
       GetResourceRequest request, ServerCallContext context)
        {
            if (!Guid.TryParse(request.Id, out var id))
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid resource id format."));

            try
            {
                var resource = await _mediator.Send(new GetResourceByIdQuery(id));

                return new ResourceResponse
                {
                    Id = resource.Id.ToString(),
                    Name = resource.Name,
                    Description = resource.Description,
                    Type = (int)resource.Type,
                    Capacity = resource.Capacity,
                    Building = resource.Building,
                    Floor = resource.Floor,
                    RoomNumber = resource.RoomNumber,
                    IsAvailable = resource.IsAvailable
                };
            }
            catch (ResourceNotFoundException)
            {
                throw new RpcException(new Status(StatusCode.NotFound, $"Resource '{request.Id}' not found."));
            }
        }


        public override async Task<AvailabilityResponse> CheckAvailability(
         CheckAvailabilityRequest request,
         ServerCallContext context)
        {
            if (!Guid.TryParse(request.ResourceId, out var id))
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid resource id format."));

            var resource = await _repository.GetByIdAsync(id, context.CancellationToken);

            if (resource is null)
                throw new RpcException(new Status(StatusCode.NotFound, $"Resource '{request.ResourceId}' not found."));

            return new AvailabilityResponse
            {
                IsAvailable = resource.IsAvailable,
                ResourceName = resource.Name
            };
        }

        public override async Task<ResourceListResponse> ListResourcesByType(
         ListResourcesByTypeRequest request,
         ServerCallContext context)
        {
            var type = (ResourceType)request.Type;
            var resources = await _repository.GetByTypeAsync(type, context.CancellationToken);

            var response = new ResourceListResponse();

            foreach (var resource in resources)
            {
                response.Resources.Add(new ResourceResponse
                {
                    Id = resource.Id.ToString(),
                    Name = resource.Name,
                    Description = resource.Description,
                    Type = (int)resource.Type,
                    Capacity = resource.Capacity,
                    Building = resource.Location.Building,
                    Floor = resource.Location.Floor,
                    RoomNumber = resource.Location.RoomNumber,
                    IsAvailable = resource.IsAvailable
                });
            }

            return response;
        }

    }
}
