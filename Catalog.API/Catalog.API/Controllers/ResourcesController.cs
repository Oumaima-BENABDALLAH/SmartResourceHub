using Catalog.API.Models;
using Catalog.Application.DTOs;
using Catalog.Application.Features.Resources.Commands.CreateResource;
using Catalog.Application.Features.Resources.Commands.DeleteResource;
using Catalog.Application.Features.Resources.Commands.UpdateResource;
using Catalog.Application.Features.Resources.Queries.GetAllResources;
using Catalog.Application.Features.Resources.Queries.GetAvailableResources;
using Catalog.Application.Features.Resources.Queries.GetResourceById;
using Catalog.Domain.Exceptions;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Catalog.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ResourcesController : ControllerBase
    {
        private readonly IMediator _mediator;
        public ResourcesController(IMediator mediator) 
        {
            _mediator = mediator;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IReadOnlyList<ResourceDto>), StatusCodes.Status200OK)]

        public async  Task<ActionResult<IReadOnlySet<ResourceDto>>> GetAll(CancellationToken cancellationToken) {

            var result = _mediator.Send(new GetAllResourcesQuery(), cancellationToken);
            return Ok(result);
        }


        [HttpGet("available")]
        [ProducesResponseType(typeof(IReadOnlyList<ResourceDto>), StatusCodes.Status200OK)]

        public async Task<ActionResult<IReadOnlySet<ResourceDto>>> GetAvailable(CancellationToken cancellationToken)
        {

            var result = _mediator.Send(new GetAvailableResourcesQuery(), cancellationToken);
            return Ok(result);
        }


        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ResourceDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ResourceDto>> GetById(Guid id ,CancellationToken cancellationToken)
        {

            try
            {
                var result = _mediator.Send(new GetResourceByIdQuery(id), cancellationToken);
                return Ok(result);
            }
            catch (ResourceNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });

            }
        }

        [HttpPost]
        [ProducesResponseType(typeof(ResourceDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ResourceDto>> Create([FromBody] CreateResourceRequest request, CancellationToken cancellationToken)
        {

            try
            {
                var command = new CreateResourceCommand(
                    
                     request.Name,
                     request.Description,
                     request.Type,
                     request.Capacity,
                     request.Building,
                     request.Floor,
                     request.RoomNumber );

                var result = _mediator.Send(command, cancellationToken);

                return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { errors = ex.Errors.Select(e => e.ErrorMessage) });
            }
            catch (DomainException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Update(Guid id,
        [FromBody] UpdateResourceRequest request,
         CancellationToken cancellationToken)
        {
            try
            {
                var command = new UpdateResourceCommand(
                    id,
                    request.Name,
                    request.Description,
                    request.Capacity,
                    request.Building,
                    request.Floor,
                    request.RoomNumber);

                await _mediator.Send(command, cancellationToken);

                return NoContent();
            }
            catch (ResourceNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { errors = ex.Errors.Select(e => e.ErrorMessage) });
            }
            catch (DomainException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ResourceDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ResourceDto>> Delte(Guid id , CancellationToken cancellationToken)
        {

            try
            {
                

                var result = _mediator.Send(new DeleteResourceCommand (id), cancellationToken);

                return NoContent();
            }
            catch (ResourceNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
    }
}
