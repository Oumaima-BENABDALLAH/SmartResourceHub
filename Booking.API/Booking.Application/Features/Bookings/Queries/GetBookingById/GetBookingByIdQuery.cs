using Booking.Application.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Booking.Application.Features.Bookings.Queries.GetBookingById
{
    // Classe de requête (Query) qui contient uniquement les données nécessaires (l'Id) pour demander la récupération d'une réservation.
    public sealed record  GetBookingByIdQuery (Guid id ) : IRequest<BookingDto>;
   
}
