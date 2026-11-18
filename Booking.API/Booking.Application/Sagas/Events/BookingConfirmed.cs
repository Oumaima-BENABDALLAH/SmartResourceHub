using System;
using System.Collections.Generic;
using System.Text;

namespace Booking.Application.Sagas.Events
{
    public record BookingConfirmed
    {
        public Guid CorrelationId { get; init; }
        public Guid ResourceId { get; init; }
        public Guid UserId { get; init; }
        public string ResourceName { get; init; } = string.Empty;
        public DateTime StartTime { get; init; }
        public DateTime EndTime { get; init; }
    }
}
