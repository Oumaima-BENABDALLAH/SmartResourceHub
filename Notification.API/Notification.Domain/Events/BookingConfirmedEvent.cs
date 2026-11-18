using System;
using System.Collections.Generic;
using System.Text;

namespace Notification.Domain.Events
{
    public record BookingConfirmedEvent
    {
        public Guid CorrelationId { get; init; }
        public Guid ResourceId { get; init; }
        public Guid UserId { get; init; }
        public string ResourceName { get; init; } = string.Empty;
        public string UserEmail { get; init; } = string.Empty;
        public DateTime StartTime { get; init; }
        public DateTime EndTime { get; init; }
    }
}
