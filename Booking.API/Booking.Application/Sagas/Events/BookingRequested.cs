using System;
using System.Collections.Generic;
using System.Text;

namespace Booking.Application.Sagas.Events
{
    public record BookingRequested
    {
        public Guid CorrelationId { get; init; }
        public Guid ResourceId { get; init; }
        public Guid UserId { get; init; }
        public DateTime StartTime { get; init; }
        public DateTime EndTime { get; init; }

    }
}
