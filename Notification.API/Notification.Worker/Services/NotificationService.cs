using Notification.Domain.Entities;
using Notification.Domain.Events;
using Notification.Infrastructure.Repositories;



namespace Notification.Worker.Services
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _repository;
        private readonly ILogger<NotificationService> _logger;
        public Task SendBookingConfirmedAsync(BookingConfirmedEvent bookingEvent, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task SendBookingRejectedAsync(BookingRejectedEvent bookingEvent, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
