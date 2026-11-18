using System;
using System.Collections.Generic;
using System.Text;

namespace Booking.Domain.Exceptions
{
    //Exception spécifique levée lorsqu'une entité Booking (Réservation) , recherchée par son identifiant unique est introuvable dans le système.
    public class BookingNotFoundException : DomainException
    {
        public BookingNotFoundException (Guid id) : base($"Booking with id '{id}' not found ") {}
    }
}
