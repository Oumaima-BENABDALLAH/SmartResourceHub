using System;
using System.Collections.Generic;
using System.Text;

namespace Notification.Domain.Events
{
    public record BookingRejectedEvent
    {
        public Guid CorrelationId { get; init; }
        public Guid UserId { get; init; }
        public string UserEmail { get; init; } = string.Empty;
        public string Reason { get; init; } = string.Empty;
    }
}
