using Booking.Application.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Booking.Application.Features.Bookings.Commands.CreateBooking
{
    // le classe Commande représente le "Quoi" , un paquet de données regroupées ensemble

    public sealed record CreateBookingCommand(
    Guid ResourceId,
    Guid UserId,
    DateTime StartTime,
    DateTime EndTime): IRequest<BookingDto>; //C'est une interface de la bibliothèque MediatR.
                                              //Elle indique que cette commande va être envoyée à un gestionnaire (Handler)
                                              //et qu'elle attend en retour un objet de réponse de type BookingDto.
}
