using Notification.Domain.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace Notification.Worker.Services
{
    public interface  INotificationService
    {
        Task SendBookingConfirmedAsync(BookingConfirmedEvent bookingEvent,CancellationToken cancellationToken = default);
        Task SendBookingRejectedAsync(BookingRejectedEvent bookingEvent, CancellationToken cancellationToken = default);

    }
}
