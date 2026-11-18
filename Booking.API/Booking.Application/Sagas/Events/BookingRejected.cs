using System;
using System.Collections.Generic;
using System.Text;

namespace Booking.Application.Sagas.Events
{
    public record BookingRejected
    {
        public Guid CorrelationId { get; init; }
        public string Reason { get; init; } = string.Empty;
    }
}
