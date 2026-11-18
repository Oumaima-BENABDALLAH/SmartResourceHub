using Booking.API.Models;
using Booking.Application.DTOs;
using Booking.Application.Features.Bookings.Commands.CancelBooking;
using Booking.Application.Features.Bookings.Commands.CreateBooking;
using Booking.Application.Features.Bookings.Queries.GetBookingById;
using Booking.Application.Features.Bookings.Queries.GetBookingsByUser;
using Booking.Domain.Exceptions;
using Catalog.Application.DTOs;
using Catalog.Application.Features.Resources.Queries.GetResourceById;
using Catalog.Domain.Exceptions;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using DomainException = Booking.Domain.Exceptions.DomainException;
namespace Booking.API.Controllers
{
    //Active les comportements par défaut pour une API Web , il gère tout seul la validation des données et la lecture des requêtes JSON envoyées par les clients.
    [ApiController]

    //Définit l'URL de base
    [Route("api/[controller]")]
    public class BookingsController : ControllerBase  // Hérite de ControllerBase pour obtenir les méthodes utilitaires de réponse HTTP (Ok, NotFound, BadRequest)
    {
        //C'est l'outil indispensable pour envoyer nos requêtes/commandes vers la couche Application sans que le contrôleur ait besoin de connaître la logique métier ou la base de données.
        private readonly IMediator _mediator;

        public BookingsController(IMediator mediator)
        => _mediator = mediator;



        [HttpPost] //utilisée pour créer une ressource
        [ProducesResponseType(typeof(BookingDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]

        //[FromBody]  force .NET à lire les données envoyées par le client dans le corps de la requête JSON pour les mapper vers CreateBookingCommand.
        //CancellationToken : Permet d'annuler l'opération si le client ferme sa page ou interrompt la requête en cours (très bon pour la performance).
        public async Task<ActionResult<BookingDto>> Create([FromBody] CreateBookingCommand command, CancellationToken cancellationToken) 
        {

            try
            {
             
                var result = await  _mediator.Send(command, cancellationToken);

                return Ok(result);
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



        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(BookingDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<BookingDto>> GetById(Guid id, CancellationToken cancellationToken)
        {

            try
            {
                //C'est une requête de lecture MediatR. Contrairement à une commande, elle sert uniquement à récupérer des données.
                var result = await  _mediator.Send(new GetBookingByIdQuery(id), cancellationToken);
                return Ok(result);
            }
            catch (ResourceNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });

            }
        }

        [HttpGet("user/{userId:guid}")]
        [ProducesResponseType(typeof(IReadOnlyList<BookingDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IReadOnlyList<BookingDto>>> GetByUser( Guid userId, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetBookingsByUserQuery(userId), cancellationToken);
            return Ok(result);

        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Cancel(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                await _mediator.Send(new CancelBookingCommand(id), cancellationToken);
                return NoContent();
            }
            catch (BookingNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (DomainException ex)
            {
                return BadRequest(new {message = ex.Message });
            }
        }

    }
}
