using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Booking.Application.Features.Bookings.Commands.CancelBooking
{
    // Classe de commande qui contient uniquement les données nécessaires (l'Id) pour demander l'annulation d'une réservation.
    public sealed record CancelBookingCommand(Guid Id) : IRequest;

}
