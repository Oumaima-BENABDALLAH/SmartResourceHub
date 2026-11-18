using Booking.Application.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Booking.Application.Features.Bookings.Queries.GetBookingsByUser
{
    // Classe de requête (Query) qui contient uniquement les données nécessaires (l'UserId) pour demander la récupération d'une réservation.

    public sealed record   GetBookingsByUserQuery(Guid UserId) : IRequest<IReadOnlyList<BookingDto>>;
   
}
